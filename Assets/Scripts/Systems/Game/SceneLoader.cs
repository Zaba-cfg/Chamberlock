using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Systems.Game
{
    public class SceneLoader : MonoBehaviour
    {
        public static SceneLoader Instance { get; private set; }

        public bool IsLoading { get; private set; }

        public event Action<float> ProgressChanged;
        public event Action<SceneId> SceneLoaded;

        private void Awake()
        {
            if (Instance != null && Instance != this)
                throw new InvalidOperationException($"{name}: only one SceneLoader is allowed.");

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void Load(SceneId id, Action onComplete = null)
        {
            if (IsLoading)
                throw new InvalidOperationException($"{name}: a scene load is already in progress.");

            string sceneName = id.ToString();

            if (!Application.CanStreamedLevelBeLoaded(sceneName))
                throw new InvalidOperationException(
                    $"{name}: scene '{sceneName}' is not in Build Settings. Add it before loading.");

            StartCoroutine(LoadRoutine(id, sceneName, onComplete));
        }

        private IEnumerator LoadRoutine(SceneId id, string sceneName, Action onComplete)
        {
            IsLoading = true;
            ProgressChanged?.Invoke(0f);

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!operation.isDone)
            {
                ProgressChanged?.Invoke(operation.progress);
                yield return null;
            }

            IsLoading = false;
            ProgressChanged?.Invoke(1f);
            SceneLoaded?.Invoke(id);

            onComplete?.Invoke();
        }
    }
}
