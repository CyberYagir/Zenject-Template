using System;
using System.Collections.Generic;
using Content.Scripts.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

namespace Content.Scripts.Services
{
    public enum ESceneName
    {
        Boot,
        Game
    }

    public class ScenesService : MonoBehaviour
    {
        [SerializeField] private UIFader fader;

        private List<ESceneName> overlayLoadedScenes = new List<ESceneName>();
        public event Action<ESceneName> OnChangeActiveScene;
        public event Action<ESceneName> OnLoadOtherScene;
        public event Action<ESceneName> OnUnLoadOtherScene;

        public void ChangeScene(ESceneName name)
        {
            OnLoadOtherScene?.Invoke(name);
            overlayLoadedScenes.Clear();
            SceneManager.LoadScene(name.ToString());
        }

        public AsyncOperation ChangeSceneAsync(ESceneName name)
        {
            return SceneManager.LoadSceneAsync(name.ToString());
        }

        public void FadeScene(ESceneName name)
        {
            fader.Fade(delegate
            {
                ChangeScene(name);
            });
        }
        
        public void FadeScene(ESceneName name, Action onFadeEnd)
        {
            fader.Fade(delegate
            {
                onFadeEnd?.Invoke();
                ChangeScene(name);
            });
        }

        public AsyncOperation AddScene(ESceneName name)
        {
            if (!overlayLoadedScenes.Contains(name))
            {
                overlayLoadedScenes.Add(name);
                return SceneManager.LoadSceneAsync(name.ToString(), LoadSceneMode.Additive);
            }
            return null;
        }

        public void UnloadScene(ESceneName name)
        {
            if (overlayLoadedScenes.Contains(name))
            {
                OnUnLoadOtherScene?.Invoke(name);
                SceneManager.UnloadSceneAsync(name.ToString());
                overlayLoadedScenes.Remove(name);
            }
        }


        public void Fade(Action action)
        {
            fader.Fade(action);
        }


        public void ChangeActiveScene(ESceneName sceneName)
        {
            if (GetActiveScene() != sceneName)
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName.ToString()));
                OnChangeActiveScene?.Invoke(sceneName);
                Debug.Log("[Scene Change] Scene changed to " + sceneName);
            }
        }

        public ESceneName GetActiveScene()
        {
            try
            {
                return Enum.Parse<ESceneName>(SceneManager.GetActiveScene().name);
            }
            catch {
                return ESceneName.Boot;
            }
        }
    }
}
