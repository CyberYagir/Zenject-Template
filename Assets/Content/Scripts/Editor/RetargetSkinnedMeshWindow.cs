#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Assertions;
using UnityEngine.Rendering;

public class RetargetSkinnedMeshWindow : EditorWindow
{
    [MenuItem("Tools/Skinning/Retarget Skinned Mesh...")]
    private static void Open() => GetWindow<RetargetSkinnedMeshWindow>("Retarget Skinned Mesh");

    [Header("Source (what to transfer)")]
    [SerializeField] private SkinnedMeshRenderer sourceSMR;

    [Header("Target (which bones to bind to)")]
    [SerializeField] private Transform targetRoot;     // root of bone hierarchy
    [SerializeField] private Animator targetAnimator;  // optional — for Humanoid mapping

    [Space(8)]
    [SerializeField] private bool mapByHumanoid = false;
    [SerializeField] private bool tryByNameIfHumanoidNotFound = true;
    [SerializeField] private bool caseInsensitive = true;
    [SerializeField] private string trimFromSource = "";
    [SerializeField] private string trimFromTarget = "";

    [Space(8)]
    [SerializeField] private bool duplicateMeshAsset = true;
    [SerializeField] private bool recalcBindposes = false; // rebind using target's current pose
    [SerializeField] private bool copyMaterials = true;
    [SerializeField] private bool copyBoundsFromSource = true;

    private Transform[] _mappedBones;       // mapping result
    private string[]    _boneNamesSource;   // source bone names
    private bool        _mappingBuilt;

    private Vector2     _scroll;

    private void OnGUI()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Source SMR", GUILayout.Width(120));
            sourceSMR = (SkinnedMeshRenderer)EditorGUILayout.ObjectField(sourceSMR, typeof(SkinnedMeshRenderer), true);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Target Root", GUILayout.Width(120));
            targetRoot = (Transform)EditorGUILayout.ObjectField(targetRoot, typeof(Transform), true);
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.LabelField("Target Animator (Humanoid)", GUILayout.Width(120));
            targetAnimator = (Animator)EditorGUILayout.ObjectField(targetAnimator, typeof(Animator), true);
        }

        EditorGUILayout.Space(6);
        mapByHumanoid = EditorGUILayout.ToggleLeft("Try mapping via Humanoid (if available)", mapByHumanoid);
        tryByNameIfHumanoidNotFound = EditorGUILayout.ToggleLeft("If Humanoid mapping fails for a bone — try by name", tryByNameIfHumanoidNotFound);

        EditorGUILayout.Space(4);
        caseInsensitive = EditorGUILayout.ToggleLeft("Ignore case when matching names", caseInsensitive);
        using (new EditorGUILayout.HorizontalScope())
        {
            trimFromSource = EditorGUILayout.TextField(new GUIContent("Trim from source names"), trimFromSource);
            trimFromTarget = EditorGUILayout.TextField(new GUIContent("Trim from target names"), trimFromTarget);
        }

        EditorGUILayout.Space(8);
        duplicateMeshAsset = EditorGUILayout.ToggleLeft("Duplicate mesh asset (recommended)", duplicateMeshAsset);
        recalcBindposes    = EditorGUILayout.ToggleLeft("Recalculate bindposes for target's current pose (rebind)", recalcBindposes);
        copyMaterials      = EditorGUILayout.ToggleLeft("Copy source materials", copyMaterials);
        copyBoundsFromSource = EditorGUILayout.ToggleLeft("Copy bounds from source", copyBoundsFromSource);

        EditorGUILayout.Space(8);
        using (new EditorGUI.DisabledScope(sourceSMR == null || (targetRoot == null && !mapByHumanoid)))
        {
            if (GUILayout.Button("Scan and build mapping", GUILayout.Height(28)))
            {
                BuildMapping();
            }
        }

        EditorGUILayout.Space(8);
        DrawMappingPreview();

        EditorGUILayout.Space(8);
        using (new EditorGUI.DisabledScope(!_mappingBuilt || _mappedBones == null || _mappedBones.Length == 0))
        {
            if (GUILayout.Button("Create new SkinnedMesh on target bones", GUILayout.Height(36)))
            {
                ApplyRetarget();
            }
        }
    }

    void BuildMapping()
    {
        _mappingBuilt = false;

        if (sourceSMR == null || sourceSMR.sharedMesh == null)
        {
            ShowNotification(new GUIContent("No source or mesh is missing"));
            return;
        }

        var bonesSrc = sourceSMR.bones;
        _mappedBones = new Transform[bonesSrc.Length];
        _boneNamesSource = new string[bonesSrc.Length];

        // Prepare a dictionary of target bones by name
        Dictionary<string, Transform> targetByName = new Dictionary<string, Transform>();
        if (targetRoot != null)
        {
            foreach (var t in targetRoot.GetComponentsInChildren<Transform>(true))
            {
                var name = NormalizeName(t.name, trimFromTarget, caseInsensitive);
                if (!targetByName.ContainsKey(name))
                    targetByName.Add(name, t);
            }
        }

        // If humanoid exists — build a dictionary humanBone -> Transform
        Dictionary<string, Transform> humanoidMap = null;
        if (mapByHumanoid && targetAnimator != null && targetAnimator.isHuman)
        {
            humanoidMap = new Dictionary<string, Transform>();
            foreach (HumanBodyBones hbb in Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (hbb == HumanBodyBones.LastBone) continue;
                var tf = targetAnimator.GetBoneTransform(hbb);
                if (tf) humanoidMap[hbb.ToString()] = tf;
            }
        }

        for (int i = 0; i < bonesSrc.Length; i++)
        {
            var srcTf = bonesSrc[i];
            var srcNameRaw = srcTf ? srcTf.name : $"bone_{i}";
            _boneNamesSource[i] = srcNameRaw;

            Transform mapped = null;

            // 1) Humanoid (if the name looks like a human-bone)
            if (humanoidMap != null)
            {
                var hbb = GuessHumanBone(srcNameRaw);
                if (hbb != null && humanoidMap.TryGetValue(hbb, out var tf))
                    mapped = tf;
            }

            // 2) By name
            if (mapped == null && targetByName.Count > 0)
            {
                var key = NormalizeName(srcNameRaw, trimFromSource, caseInsensitive);
                targetByName.TryGetValue(key, out mapped);
            }

            _mappedBones[i] = mapped; // may be null — will show this in the table
        }

        _mappingBuilt = true;
        Repaint();
    }

    string NormalizeName(string s, string trim, bool ci)
    {
        if (!string.IsNullOrEmpty(trim))
            s = s.Replace(trim, "");
        return ci ? s.ToLowerInvariant() : s;
    }

    // Simple heuristic: try to match common bone names to HumanBodyBones
    string GuessHumanBone(string srcName)
    {
        var n = srcName.ToLowerInvariant();
        // arms
        if (n.Contains("upperarm_l") || n.Contains("shoulder_l") || n.EndsWith("l_shoulder")) return HumanBodyBones.LeftUpperArm.ToString();
        if (n.Contains("upperarm_r") || n.Contains("shoulder_r") || n.EndsWith("r_shoulder")) return HumanBodyBones.RightUpperArm.ToString();
        if (n.Contains("lowerarm_l") || n.Contains("forearm_l")) return HumanBodyBones.LeftLowerArm.ToString();
        if (n.Contains("lowerarm_r") || n.Contains("forearm_r")) return HumanBodyBones.RightLowerArm.ToString();
        if (n.Contains("hand_l")) return HumanBodyBones.LeftHand.ToString();
        if (n.Contains("hand_r")) return HumanBodyBones.RightHand.ToString();

        // legs
        if (n.Contains("thigh_l") || n.Contains("upleg_l")) return HumanBodyBones.LeftUpperLeg.ToString();
        if (n.Contains("thigh_r") || n.Contains("upleg_r")) return HumanBodyBones.RightUpperLeg.ToString();
        if (n.Contains("calf_l") || n.Contains("lowerleg_l") || n.Contains("shin_l")) return HumanBodyBones.LeftLowerLeg.ToString();
        if (n.Contains("calf_r") || n.Contains("lowerleg_r") || n.Contains("shin_r")) return HumanBodyBones.RightLowerLeg.ToString();
        if (n.Contains("foot_l")) return HumanBodyBones.LeftFoot.ToString();
        if (n.Contains("foot_r")) return HumanBodyBones.RightFoot.ToString();

        // torso/head
        if (n.Contains("hips") || n.Contains("pelvis")) return HumanBodyBones.Hips.ToString();
        if (n.Contains("spine2") || n.Contains("chest")) return HumanBodyBones.Chest.ToString();
        if (n.Contains("spine")) return HumanBodyBones.Spine.ToString();
        if (n.Contains("neck")) return HumanBodyBones.Neck.ToString();
        if (n.Contains("head")) return HumanBodyBones.Head.ToString();

        return null;
    }

    void DrawMappingPreview()
    {
        if (!_mappingBuilt || _mappedBones == null || sourceSMR == null) return;

        var bonesSrc = sourceSMR.bones;
        if (bonesSrc == null) return;

        EditorGUILayout.LabelField("Mapping (Source → Target):", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll, GUILayout.MinHeight(200));

        for (int i = 0; i < bonesSrc.Length; i++)
        {
            var srcName = _boneNamesSource[i];
            var dst = _mappedBones[i];
            var ok = dst != null;

            using (new EditorGUILayout.HorizontalScope())
            {
                GUI.color = ok ? Color.white : new Color(1f, .85f, .3f);
                EditorGUILayout.ObjectField(bonesSrc[i], typeof(Transform), true);
                GUI.color = Color.white;
                EditorGUILayout.LabelField("→", GUILayout.Width(18));
                _mappedBones[i] = (Transform)EditorGUILayout.ObjectField(_mappedBones[i], typeof(Transform), true);
            }
        }

        EditorGUILayout.EndScrollView();

        int missing = _mappedBones.Count(t => t == null);
        if (missing > 0)
        {
            EditorGUILayout.HelpBox($"Missing {missing} bones. Fill them manually or adjust the matching rules.", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("All bones are mapped ✅", MessageType.Info);
        }
    }

    void ApplyRetarget()
    {
        if (sourceSMR == null || sourceSMR.sharedMesh == null)
        {
            EditorUtility.DisplayDialog("Error", "No source or the source has no Mesh.", "OK");
            return;
        }
        if (_mappedBones == null || _mappedBones.Any(b => b == null))
        {
            if (!EditorUtility.DisplayDialog("Warning", "There are unmapped bones. Continue?", "Continue", "Cancel"))
                return;
        }

        // Create receiver GameObject
        var go = new GameObject(sourceSMR.name + "_Retargeted");
        Undo.RegisterCreatedObjectUndo(go, "Create Retargeted SMR");

        if (targetRoot != null)
            go.transform.SetParent(targetRoot, false);

        var dstSMR = Undo.AddComponent<SkinnedMeshRenderer>(go);

        // Copy/duplicate Mesh
        Mesh dstMesh;
        if (duplicateMeshAsset)
        {
            dstMesh = sourceSMR.sharedMesh;
            // optionally — save as asset in the project:
            // var path = EditorUtility.SaveFilePanelInProject("Save Mesh", dstMesh.name + ".asset", "asset", "");
            // if (!string.IsNullOrEmpty(path)) AssetDatabase.CreateAsset(dstMesh, path);
        }
        else
        {
            dstMesh = sourceSMR.sharedMesh;
        }

        dstSMR.sharedMesh = dstMesh;

        // Materials
        if (copyMaterials)
            dstSMR.sharedMaterials = sourceSMR.sharedMaterials;

        // Bones
        dstSMR.bones = _mappedBones;

        // RootBone — try to pick from target, otherwise use targetRoot
        Transform newRoot = targetRoot;
        // If source has a rootBone, try to find a bone with the same name in the target
        if (sourceSMR.rootBone != null)
        {
            var key = NormalizeName(sourceSMR.rootBone.name, trimFromSource, caseInsensitive);
            var found = _mappedBones.FirstOrDefault(b => b != null && NormalizeName(b.name, trimFromTarget, caseInsensitive) == key);
            if (found != null) newRoot = found;
        }
        dstSMR.rootBone = newRoot != null ? newRoot : targetRoot;

        // Rebind bindposes to the target skeleton's current pose
        if (recalcBindposes && dstMesh != null && dstSMR.bones != null && dstSMR.rootBone != null)
        {
            var bones = dstSMR.bones;
            var bindposes = new Matrix4x4[bones.Length];
            var rootL2W = dstSMR.rootBone.localToWorldMatrix;
            for (int i = 0; i < bones.Length; i++)
            {
                var b = bones[i] ? bones[i] : dstSMR.rootBone;
                bindposes[i] = b.worldToLocalMatrix * rootL2W;
            }
            dstMesh.bindposes = bindposes;
        }

        // Bounds
        if (copyBoundsFromSource)
            dstSMR.localBounds = sourceSMR.localBounds;
        else
            dstSMR.sharedMesh.RecalculateBounds();

        EditorGUIUtility.PingObject(go);
        Selection.activeObject = go;
        EditorUtility.DisplayDialog("Done", "Skinned Mesh has been transferred to the target skeleton.", "OK");
    }
}
#endif
