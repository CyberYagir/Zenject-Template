using System;
using System.IO;
using System.Text;
using Sirenix.OdinInspector;
using UnityEngine;
using Zenject;

namespace Content.Scripts.Scriptable
{
    [CreateAssetMenu(menuName = "SO/SaveDataSO", fileName = "SaveDataSO", order = 0)]
    public sealed partial class SaveDataSo : ScriptableObjectInstaller
    {

        public override void InstallBindings()
        {
            Container.Bind<SaveDataSo>().FromInstance(this).AsSingle();
            LoadFile();
        }

        public string GetPathFolder()
        {
#if UNITY_EDITOR || PLATFORM_STANDALONE_WIN
            return Directory.GetParent(Application.dataPath)?.FullName + @"\Saves\";
#endif
#if UNITY_ANDROID
            return Application.persistentDataPath;
#else
            return null;
#endif
        }

        public string GetFilePath()
        {
            if (!Directory.Exists(GetPathFolder()))
            {
                Directory.CreateDirectory(GetPathFolder());
            }

            return GetPathFolder() + @"\data.dat";
        }

        [Button]
        public void SaveFile()
        {
            string json = "";

#if UNITY_EDITOR || UNITY_ANDROID || PLATFORM_STANDALONE_WIN
            if (!string.IsNullOrEmpty(GetPathFolder()))
            {
                json = JsonUtility.ToJson(this);
                File.WriteAllText(GetFilePath(), json, Encoding.Unicode);
#if UNITY_EDITOR
                // File.WriteAllText(
                //     GetPathFolder() + @"\Backups\backup" + DateTime.Now.ToString("yyyy-M-d dddd-HH-mm-ss") + ".dat",
                //     json, Encoding.Unicode);
#endif
            }
            else
            {
                Debug.LogError("Path error!");
            }
#endif
#if UNITY_WEBGL
            json = JsonUtility.ToJson(this);
            PlayerPrefs.SetString(PrefsSaveKey, json);
#endif
        }

        [Button]
        public void LoadFile()
        {
#if UNITY_EDITOR || UNITY_ANDROID || PLATFORM_STANDALONE_WIN
            var file = GetFilePath();
            if (File.Exists(file))
            {
                try
                {
                    JsonUtility.FromJsonOverwrite(File.ReadAllText(file), this);
                    return;
                }
                catch
                {
                    Debug.LogError("Save Parse Error");
                }
            }
#endif
#if UNITY_WEBGL
            var json = PlayerPrefs.GetString(PrefsSaveKey);
            if (!string.IsNullOrEmpty(json))
            {
                JsonUtility.FromJsonOverwrite(json, this);
                return;
            }
#endif

            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(CreateInstance<SaveDataSo>()), this);

            SaveFile();

        }

        [Button]
        public void DeleteFile()
        {
#if UNITY_EDITOR || UNITY_ANDROID || PLATFORM_STANDALONE_WIN

            Debug.Log(GetFilePath());
            var file = GetFilePath();
            if (File.Exists(file))
            {
                File.Delete(file);
            }
#endif
            LoadFile();
        }

        [Button]
        public void OpenFile()
        {
            Application.OpenURL(Directory.GetParent(GetFilePath())?.FullName);
        }
        
        
        public void ReplaceJson(string json)
        {
            if (!string.IsNullOrEmpty(json))
            {
                var file = GetFilePath();
                if (File.Exists(file))
                {
                    File.Delete(file);
                }

                File.WriteAllText(file, json);
                LoadFile();
            }
        }


        private string GetFilePathByName(string fileName)
        {
            var files = Directory.GetFiles(GetPathFolder(), $"*{fileName}*.dat");

            if (files.Length == 0)
            {
                return GetPathFolder() + @"\" + fileName + ".dat";
            }
            
            return files[0];
        }

        public void ClearFileByName(string fileName)
        {
            var fullPath = GetFilePathByName(fileName);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);

                var files = Directory.GetFiles(GetPathFolder(), $"*{fileName}*.jpg");

                if (files.Length != 0)
                {
                    File.Delete(files[0]);
                }
            }
        }
    }

}