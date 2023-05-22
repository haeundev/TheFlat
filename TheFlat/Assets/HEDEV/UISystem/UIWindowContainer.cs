using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.CameraSystem;
using Proto.Util;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Type = System.Type;

namespace Proto.UISystem
{
    public class UIWindowContainer : MonoBehaviour
    {
        private const float InstantiateWaitTime = 5f;
        [SerializeField] private UIContainer uiContainer;
        public bool isUsedUICamera = true;
        public Transform subUIWindowsPool;
        private readonly List<UIWindow> _windows = new();
        private Dictionary<Type, string> _pathContainer;
        
        private void Awake()
        {
            Init();
        }

        public int GetMaxSortingOrder(bool includeKeepSortingOrderWindows)
        {
            if (!_windows.Any())
                return 0;

            return includeKeepSortingOrderWindows
                ? _windows.Max(window => window.SortingOrder)
                : _windows.Where(window => !window.IsKeepSortingOrderWindow).Max(window => window.SortingOrder);
        }

        public string Find<T>() where T : UIWindow
        {
            string value = null;
            _pathContainer.TryGetValue(typeof(T), out value);
            return value;
        }

        public bool CheckCreatedWindow<T>() where T : UIWindow
        {
            return _createdWindows.ContainsKey(typeof(T));
        }

        public bool CheckReservedWindow<T>(Action<UIWindow> action) where T : UIWindow
        {
            if (!_actionOnCreation.ContainsKey(typeof(T)))
                return false;
            _actionOnCreation[typeof(T)].Add(action);
            return true;
        }

        #region Extension

        public IEnumerator<UIWindow> GetEnumerator()
        {
            return _createdWindows.Select(item => item.Value).GetEnumerator();
        }

        #endregion


        public void ResetUIWindows()
        {
            foreach (var window in _windows) window.ResetWindow();
        }

        public void SetUICamera()
        {
            var windowCanvas = gameObject.transform.GetComponentsInChildren<Canvas>(true);
            foreach (var canvas in windowCanvas) canvas.worldCamera = CameraManager.UI;
        }

        #region Init

        private void Init()
        {
            InitContainer();
            SetWindowPool();
        }

        private void SetWindowPool()
        {
            if (subUIWindowsPool != null)
                return;
            var windowPoolObject = new GameObject();
            windowPoolObject.transform.SetParent(gameObject.transform);
            windowPoolObject.transform.localPosition = Vector3.zero;
            windowPoolObject.name = "SubWindowsPool";
            subUIWindowsPool = windowPoolObject.transform;
        }

        private void InitContainer()
        {
            _pathContainer = new Dictionary<Type, string>();

            var index = 0;
            try
            {
                foreach (var uiKeyValue in uiContainer.UIList)
                {
                    index++;
                    _pathContainer[uiKeyValue.Window.GetType()] = uiKeyValue.Path;
                }
            }
            catch
            {
                Debug.LogError("UIContainer Error Index : " + index);
            }
        }

        #endregion

        #region Addressable Used UIWindow

        private readonly Dictionary<Type, UIWindow> _createdWindows = new();

        private readonly Dictionary<Type, List<Action<UIWindow>>> _actionOnCreation = new();

        public UIWindow GetWindow<T>() where T : UIWindow
        {
            return _createdWindows.ContainsKey(typeof(T)) ? _createdWindows[typeof(T)] : null;
        }

        public IEnumerator GetWindow<T>(Action<UIWindow> window, WindowOption option) where T : UIWindow
        {
            if (_createdWindows.ContainsKey(typeof(T)))
                window?.Invoke(_createdWindows[typeof(T)]);
            else
                yield return CreateWindow<T>(window, option);
        }

        public bool IsInAddressable<T>() where T : UIWindow
        {
            var path = Find<T>();
            return !ObjectExtensions.IsNullOrEmpty(path);
        }

        public string GetPath<T>() where T : UIWindow
        {
            var path = Find<T>();
            return ObjectExtensions.IsNullOrEmpty(path) ? null : path;
        }

        private IEnumerator CreateWindow<T>(Action<UIWindow> windowAction, WindowOption option) where T : UIWindow
        {
            var requestTime = Time.deltaTime;
            Debug.Log($" Create Window Request [Type] : {typeof(T)} - Request Time : {requestTime}");
            UIWindow window = null;
            var path = Find<T>();

            var currentWaitTime = 0f;

            if (_actionOnCreation.ContainsKey(typeof(T)))
            {
                _actionOnCreation[typeof(T)].Add(windowAction);
                yield break;
            }

            _actionOnCreation[typeof(T)] = new List<Action<UIWindow>>();
            _actionOnCreation[typeof(T)].Add(windowAction);

            Addressables.LoadAssetAsync<GameObject>(path).Completed += op =>
            {
                var windowObject = Instantiate(op.Result, transform);
                window = windowObject.GetComponent<UIWindow>();
                window.AddressableHandle = op;
                window.m_WindowOption = option;
                window.gameObject.SetActive(false);
                _windows.Add(window);
                _createdWindows[typeof(T)] = window;
            };

            var isDelay = false;
            while (!isDelay && window == null)
            {
                currentWaitTime += Time.deltaTime;
                yield return YieldInstructionCache.WaitForEndOfFrame;
                if (currentWaitTime > InstantiateWaitTime)
                {
                    Debug.LogError(
                        $"Open UIWindow very long delay [Type] :{typeof(T)} - delay Time : {Time.time} - delta : {Time.time - requestTime}");
                    isDelay = true;
                }
            }

            while (window == null)
                yield return null;
            //yield return new WaitUntil(() => window != null);

            SetUICamera(window);

            foreach (var action in _actionOnCreation[typeof(T)])
                action?.Invoke(window);

            window.gameObject.AddComponent<UIInputBlock>();
            _actionOnCreation[typeof(T)].Clear();
            if (_actionOnCreation.ContainsKey(typeof(T))) _actionOnCreation.Remove(typeof(T));
        }

        private void SetUICamera(UIWindow window)
        {
            if (CameraManager.UI == null || !isUsedUICamera)
                return;

            if (!window.isUseUICamera) return;
            var canvas = window.GetComponent<Canvas>();
            if (canvas == null) return;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = CameraManager.UI;
            canvas.planeDistance = UIWindowManager.DefaultPlaneDistance;
        }

        #endregion

        #region Close Window

        public void CloseWindow<T>() where T : UIWindow
        {
            CloseWindow(typeof(T));
        }

        public void CloseWindow(UIWindow window)
        {
            if (_createdWindows.ContainsValue(window))
            {
                var getKey = _createdWindows.FirstOrDefault(x => x.Value == window).Key;
                CloseWindow(getKey);
            }
            if (_windows.Contains(window))
                _windows.Remove(window);
            CloseWindow(window.GetType());
        }

        public void CloseWindow(Type type)
        {
            if (!_createdWindows.ContainsKey(type))
                return;
            
            var window = _createdWindows[type];
            foreach (var subWindow in window.SubWindowSources)
                if (subWindow != null)
                    Destroy(subWindow.gameObject);
            _createdWindows.Remove(type);
            Destroy(window.gameObject);
        }

        #endregion
    }
}