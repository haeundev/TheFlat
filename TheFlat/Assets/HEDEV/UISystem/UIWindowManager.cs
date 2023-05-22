using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Proto.CameraSystem;
using Proto.Util;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Type = System.Type;

namespace Proto.UISystem
{
    public class UIWindowManager : MonoBehaviour, IUIWindowManager
    {
        public const float DefaultPlaneDistance = 20f;

        [SerializeField] private UIContainer uiContainer;
        [SerializeField] private Material grayscaleMaterial;
        private readonly Dictionary<Type, Dictionary<int, List<Action<UIWindow>>>> _actionsByType = new();

        private readonly List<UIWindow> _createdWindows = new();
        private readonly Dictionary<Type, UIBodyContainer> _uiBodyByType = new();
        private Dictionary<Type, string> _pathContainer;
        public static UIWindowManager Instance { get; private set; }
        public static Material GrayscaleMaterial => Instance.grayscaleMaterial;

        private void Awake()
        {
            Instance = this;
            UIWindow.manager = this;
            InitContainer();
            InitPool();
        }

        private string Find<T>() where T : UIWindow
        {
            _pathContainer.TryGetValue(typeof(T), out var windowName);
            return windowName;
        }

        private string Find(Type type)
        {
            _pathContainer.TryGetValue(type, out var windowName);
            return windowName;
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

        #region Open

        public static void OpenWindow<T>(UIController body, Action<T> openAction = null, int windowID = 0,
            int chileCanvasCount = 0, WindowOption option = WindowOption.None) where T : UIWindow
        {
            var isAlreadyWindow = false;
            if (Instance._uiBodyByType.ContainsKey(body.GetType()))
                isAlreadyWindow = Instance._uiBodyByType[body.GetType()].IsHasWindow(windowID);
            else
                Instance._uiBodyByType[body.GetType()] = new UIBodyContainer();

            if (isAlreadyWindow)
                openAction?.Invoke(Instance._uiBodyByType[body.GetType()].GetWindow(windowID) as T);
            else
                Instance.StartCoroutine(Instance.CreateWindow<T>(window =>
                {
                    var isKeepSortingOrder = option.HasFlag(WindowOption.KeepSortingOrder);
                    Instance.GetSortingOrder(window, chileCanvasCount, isKeepSortingOrder);
                    Instance._uiBodyByType.GetOrCreate(body.GetType())
                        .RegisterWindow(windowID, window);

                    openAction?.Invoke(window as T);
                }, windowID, option));
        }

        #endregion

        #region SetCamera

        public static void ResetUICameraAllUIWindowInCanvas()
        {
            foreach (var window in Instance._createdWindows) Instance.SetUICamera(window);
        }

        #endregion

        #region Sorting

        private int _lastSortingOrder = 1;
        private const int AddSortingOrder = 10;

        /// <summary>
        ///     생성된 창들에서 최대 sorting order 값을 찾아 반환합니다.
        /// </summary>
        /// <param name="includeKeepSortingOrderWindows">고정 sorting order 값으로 열린 창을 포함하는지 여부</param>
        /// <returns>최대 sorting order 값</returns>
        public int GetMaxSortingOrder(bool includeKeepSortingOrderWindows)
        {
            if (!_createdWindows.Any())
                return 0;

            return includeKeepSortingOrderWindows
                ? _createdWindows.Max(window => window.SortingOrder)
                : _createdWindows.Where(window => !window.IsKeepSortingOrderWindow).Max(window => window.SortingOrder);
        }

        private delegate void ReleaseSortingOrderDelegate(int standardSortingOrder, int order);

        private event ReleaseSortingOrderDelegate OnReleaseSortingOrder;

        private void GetSortingOrder(UIWindow window, int chileCanvasCount = 0, bool isKeepSortingOrder = false)
        {
            window.IsKeepSortingOrderWindow = isKeepSortingOrder;
            if (isKeepSortingOrder) return;
            _lastSortingOrder += chileCanvasCount + AddSortingOrder;
            OnReleaseSortingOrder += window.UpdateCanvasSortingOrder;
        }

        public static void ReleaseSortingOrder(UIController body)
        {
            if (body.WindowOption.HasFlag(WindowOption.KeepSortingOrder))
                return;

            Instance._lastSortingOrder -= body.SubWindowCount + AddSortingOrder;
            if (Instance._lastSortingOrder < 1) Instance._lastSortingOrder = 1;
            if (Instance.OnReleaseSortingOrder != null)
                Instance.OnReleaseSortingOrder(body.SortingOrder, body.SubWindowCount);
        }

        public static void ResetSortingOrder()
        {
            Instance._lastSortingOrder = 1;
            Instance.OnReleaseSortingOrder = null;
        }

        #endregion

        #region Close

        public void Close(UIWindow window)
        {
            CloseWindow(window);
        }

        public void DestroyWindow(UIWindow window)
        {
            OnReleaseSortingOrder -= window.UpdateCanvasSortingOrder;
            if (_createdWindows.Contains(window)) _createdWindows.Remove(window);
        }

        public static void CloseWindow(UIWindow window)
        {
            if (window.Nullable()?.gameObject == null)
                return;
            Destroy(window.gameObject);
        }

        public static void CloseUIWindowBody(UIController body)
        {
            if (Instance._uiBodyByType.ContainsKey(body.GetType()))
            {
                var createdBodyInWindowCount = Instance._uiBodyByType[body.GetType()].RemoveWindow(body.ID);
                if (createdBodyInWindowCount <= 0) Instance._uiBodyByType.Remove(body.GetType());
            }
            ReleaseSortingOrder(body);
        }

        #endregion

        #region Tree Open

        public static void GetSubWindows(Action onComplete, UIController root)
        {
            Instance.StartCoroutine(Instance.GetTreeWindow(onComplete, root));
        }

        private IEnumerator GetTreeWindow(Action onComplete, UIController root)
        {
            var addSortingOrder = 0;
            foreach (var body in root.ChildrenBodies)
            {
                yield return CreateSubWindow(body.UIWindowType, body.InitWindow, root.SortingOrder + addSortingOrder,
                    root.WindowTransform, root.WindowOption);
                yield return GetTreeWindow(() => { }, body);
                addSortingOrder++;
            }

            onComplete?.Invoke();
        }

        #endregion

        #region Create Window

        private const float InstantiateWaitTime = 5f;

        public static string GetUIPath<T>() where T : UIWindow
        {
            return Instance.Find<T>();
        }

        private static string GetUIPath(Type type)
        {
            return Instance.Find(type);
        }

        private IEnumerator CreateWindow<T>(Action<UIWindow> resultWindow, int id,
            WindowOption option = WindowOption.None) where T : UIWindow
        {
            var requestTime = Time.deltaTime;
            Debug.Log(string.Format("[New] Create Window Request [Type] : {0} - Request Time : {1}", typeof(T),
                requestTime));
            if (_actionsByType.ContainsKey(typeof(T)))
            {
                if (_actionsByType[typeof(T)].ContainsKey(id))
                {
                    _actionsByType[typeof(T)][id].Add(resultWindow);
                    yield break;
                }

                _actionsByType[typeof(T)][id] = new List<Action<UIWindow>>();
                _actionsByType[typeof(T)][id].Add(resultWindow);
            }
            else
            {
                _actionsByType[typeof(T)] = new Dictionary<int, List<Action<UIWindow>>>();
                _actionsByType[typeof(T)][id] = new List<Action<UIWindow>>();
                _actionsByType[typeof(T)][id].Add(resultWindow);
            }

            var path = GetUIPath<T>();
            if (ObjectExtensions.IsNullOrEmpty(path))
            {
                Debug.LogError("Don't Created Window Because Path is null or empty");
                yield break;
            }

            var currentWaitTime = 0f;
            var windowCreate = false;

            UIWindow window = default;

            Addressables.LoadAssetAsync<GameObject>(path).Completed += op =>
            {
                Debug.Log("[UI Window] Complete load Async UI - path: " + path);
                var windowObject = Instantiate(op.Result, transform);
                window = windowObject.GetComponent<UIWindow>();
                window.gameObject.SetActive(false);
                window.m_WindowOption = option;
                window.AddressableHandle = op;
                windowCreate = true;
            };

            while (!windowCreate)
            {
                currentWaitTime += Time.deltaTime;
                yield return YieldInstructionCache.WaitForEndOfFrame;
                if (currentWaitTime > InstantiateWaitTime)
                    Debug.LogError("[New]Open UIWindow very long delay [Type] : " + typeof(T) + "RequestTime : " +
                                   requestTime
                                   + " delay Time :" + Time.time + "Delta :" + (Time.time - requestTime));
            }

            _createdWindows.Add(window);
            SetUICamera(window);
            window.gameObject.SetActive(false);

            window.gameObject.AddComponent<UIInputBlock>();
            foreach (var windowAction in _actionsByType[typeof(T)][id]) windowAction?.Invoke(window);

            _actionsByType[typeof(T)].Remove(id);
            if (_actionsByType[typeof(T)].Count <= 0) _actionsByType.Remove(typeof(T));
        }

        private IEnumerator CreateSubWindow(Type type, Action<UIWindow> resultWindow, int sortingOrder,
            Transform parent = null, WindowOption option = WindowOption.None)
        {
            var requestTime = Time.deltaTime;
            Debug.Log($"[New - sub] Create Sub Window Request [Type] : {type} - Request Time : {requestTime}");
            var path = GetUIPath(type);
            if (ObjectExtensions.IsNullOrEmpty(path))
            {
                Debug.LogError("Don't Created Window Because Path is null or empty");
                yield break;
            }

            var currentWaitTime = 0f;
            var windowCreate = false;

            UIWindow window = default;

            Addressables.LoadAssetAsync<GameObject>(path).Completed += op =>
            {
                var windowObject = Instantiate(op.Result, transform);
                window = windowObject.GetComponent<UIWindow>();
                window.gameObject.transform.SetParent(parent, false);
                window.gameObject.SetActive(false);
                window.SortingOrder = sortingOrder;
                window.AddressableHandle = op;
                if ((option & WindowOption.KeepSortingOrder) == 0)
                    OnReleaseSortingOrder += window.UpdateCanvasSortingOrder;

                if (parent != null && parent.GetComponent<Canvas>() != null &&
                    window.GetComponent<Canvas>() != null)
                {
                    RemoveCanvas(window);
                    SetAnchor(window.GetComponent<RectTransform>());
                }

                windowCreate = true;
            };

            while (!windowCreate)
            {
                currentWaitTime += Time.deltaTime;
                yield return YieldInstructionCache.WaitForEndOfFrame;
                if (currentWaitTime > InstantiateWaitTime)
                    Debug.LogError("[New - sub] Open UIWindow very long delay [Type] : " + type + "RequestTime : " +
                                   requestTime + " delay Time :" + Time.time + "Delta :" + (Time.time - requestTime));
            }

            _createdWindows.Add(window);
            SetUICamera(window);
            window.gameObject.SetActive(false);
            resultWindow?.Invoke(window);
        }

        private void RemoveCanvas(UIWindow window)
        {
            var windowObject = window.transform;

            var raycaster = windowObject.GetComponent<GraphicRaycaster>();
            if (raycaster != null) Destroy(raycaster);
            var canvasScaler = windowObject.GetComponent<CanvasScaler>();
            if (canvasScaler != null) Destroy(canvasScaler);
            var canvas = windowObject.GetComponent<Canvas>();
            if (canvas != null) Destroy(canvas);
        }

        private void SetAnchor(RectTransform rectTr)
        {
            rectTr.anchorMax = Vector2.one;
            rectTr.anchorMin = Vector2.zero;
            rectTr.pivot = new Vector2(0.5f, 0.5f);
            rectTr.localScale = Vector3.one;
            rectTr.offsetMax = Vector2.zero;
            rectTr.offsetMin = Vector2.zero;
        }


        private void SetUICamera(UIWindow window)
        {
            if (CameraManager.UI == default || window == default || window.isUseUICamera == false)
                return;

            var canvas = window.GetComponent<Canvas>();
            if (canvas == null)
                return;

            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = CameraManager.UI;
            canvas.planeDistance = DefaultPlaneDistance;
        }

        #endregion

        #region UI Sub Pool

        private Transform pool;

        private readonly List<Transform> subWindows = new();


        private void InitPool()
        {
            if (pool != null) return;
            var emptyObject = new GameObject();
            emptyObject.transform.SetParent(transform);
            emptyObject.name = "Sub Window Pool";
            pool = emptyObject.transform;
        }

        public void InitSubWindow(Transform window)
        {
            Instance.subWindows.Add(window);
            window.SetParent(Instance.pool, false);
        }

        public void ReleaseSubWindow(Transform window)
        {
            if (window != null)
            {
                if (subWindows.Contains(window)) subWindows.Remove(window);
                Destroy(window.gameObject);
            }
        }

        #endregion
    }
}