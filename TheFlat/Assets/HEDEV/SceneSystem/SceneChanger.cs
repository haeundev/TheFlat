using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Util;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

namespace Proto.SceneSystem
{
    public class SceneChanger : MonoBehaviour
    {
        private static SceneChanger instance;

        public static SceneChanger Instance
        {
            get
            {
                if (instance.IsNull())
                {
                    instance = FindObjectOfType<SceneChanger>();
                    if (instance.IsNull()) instance = new GameObject("SceneChanger").AddComponent<SceneChanger>();
                }

                return instance;
            }
        }

        private static bool isLoading;
        private readonly Dictionary<string, SceneInstance> loadedScenes = new();

        private void Awake()
        {
            Initialize();
        }

        private void Initialize()
        {
            DontDestroyOnLoad(this);
        }

        public static void AddScene(string scenePath, LoadSceneMode mode,
            Action<AsyncOperationHandle<SceneInstance>> onComplete = null)
        {
            if (isLoading)
                return;

            Instance.StartCoroutine(Instance.LoadSceneAsync(scenePath, mode, true, onComplete));
        }

        public static IEnumerator AddScene(string scene)
        {
            var handle = Addressables.LoadSceneAsync(scene, LoadSceneMode.Additive);
            yield return handle;
            Instance.loadedScenes.Add(scene, handle.Result);
        }

        private IEnumerator LoadSceneAsync(string scenePath, LoadSceneMode loadSceneMode, bool activeOnLoad,
            Action<AsyncOperationHandle<SceneInstance>> onComplete)
        {
            isLoading = true;
            yield return null;
            var operation = Addressables.LoadSceneAsync(scenePath, loadSceneMode, activeOnLoad);
            if (onComplete != null)
                operation.Completed += onComplete;
            yield return operation;
            isLoading = false;
        }

        public static void RemoveScene(AsyncOperationHandle<SceneInstance> handle)
        {
            Addressables.UnloadSceneAsync(handle);
        }

        public static IEnumerator RemoveScene(string scene)
        {
            var found = Instance.loadedScenes.TryGetValue(scene, out var sceneInstance);
            if (!found) yield break;
            yield return Addressables.UnloadSceneAsync(sceneInstance);
            Instance.loadedScenes.Remove(scene);
        }
    }
}