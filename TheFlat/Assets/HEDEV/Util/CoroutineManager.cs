using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Proto.Util
{
    public class CoroutineManager : MonoBehaviour
    {
        private readonly Dictionary<string, Coroutine> _runningCoroutine = new();
        private static CoroutineManager Instance { get; set; }

        private void Awake()
        {
            Instance = this;
        }

        public static void ExecuteCoroutine(IEnumerator coroutine)
        {
            Instance.StartCoroutine(coroutine);
        }

        public static void ExecuteControllableCoroutine(string key, IEnumerator coroutine)
        {
            if (Instance._runningCoroutine.ContainsKey(key))
                Debug.Log("Same key already Running Coroutine");
            else
                Instance._runningCoroutine[key] = Instance.StartCoroutine(Instance.StartControllableCoroutine(key, coroutine));
        }
        
        private IEnumerator StartControllableCoroutine(string key, IEnumerator mainCoroutine)
        {
            yield return mainCoroutine;
            StopCoroutine(_runningCoroutine[key]);
            _runningCoroutine.Remove(key);
        }
        
        public static void StopControllableCoroutine(string key)
        {
            if (!Instance._runningCoroutine.ContainsKey(key)) return;
            Instance.StopCoroutine(Instance._runningCoroutine[key]);
            Instance._runningCoroutine.Remove(key);
        }

        public static void StopAll()
        {
            foreach (var coroutine in Instance._runningCoroutine.Values)
                Instance.StopCoroutine(coroutine);
            Instance._runningCoroutine.Clear();
        }
    }
}