using System;
using System.Collections.Generic;
using System.Linq;
using Proto.Enums;
using Proto.Util;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace Proto.UISystem
{
    [Flags]
    public enum WindowOption
    {
        None = 0,
        IsBackOfObject = 1 << 2,
        KeepSortingOrder = 1 << 3,
    }

    public class UIWindow : MonoBehaviour
    {
        public bool isRenewalUI;
        public bool isUseUICamera = true;
        public static IUIWindowManager manager = null;
        [ShowOnlyEnumFlags] public WindowOption m_WindowOption;
        public AsyncOperationHandle AddressableHandle { get; set; }

        [Serializable]
        public class UIComponent
        {
            public UIComponent(Types type, Component component)
            {
                this.type = type;
                this.component = component;
            }

            public enum Types
            {
                Text,
                TextMeshProUGUI,
                Image,
                RawImage,
                Button, // OnClick
                Toggle, // OnValueChanged(bool)
                Slider, // OnValueChanged
                Scrollbar, // OnValueChanged
                Dropdown, // OnValueChanged(int)
                TMP_Dropdown, // OnValueChanged(int)
                InputField, // OnValueChanged(string) OnEndEdit(string)
                ScrollRect, // OnValueChanged(Vector2)
                UIGameObject, // for showing tools
                TransText,
                UIChildSelector,
                UITweenObject,
                UIWindow,
                Animator,
                EventTrigger,
                UIComponentReferences
            }

            public void RemoveAllListeners()
            {
                switch (component)
                {
                    case Image ui: return;
                    case RawImage ui: return;
                    case Text ui: return;
                    case TextMeshProUGUI ui: return;
                    case Button ui:
                        ui.onClick?.RemoveAllListeners();
                        break;
                    case Toggle ui:
                        ui.onValueChanged?.RemoveAllListeners();
                        break;
                    case Slider ui:
                        ui.onValueChanged?.RemoveAllListeners();
                        break;
                    case Scrollbar ui:
                        ui.onValueChanged?.RemoveAllListeners();
                        break;
                    case Dropdown ui:
                        ui.onValueChanged?.RemoveAllListeners();
                        break;
                    case TMP_Dropdown ui:
                        ui.onValueChanged?.RemoveAllListeners();
                        break;
                    case InputField ui:
                        ui.onValueChanged?.RemoveAllListeners();
                        ui.onEndEdit?.RemoveAllListeners();
                        break;
                    case ScrollRect ui:
                        ui.onValueChanged?.RemoveAllListeners();
                        break;
                }
            }

            public Types type;
            public Component component;
        }

        public Canvas Canvas { get; private set; }

        public int KeepSortingOrder = 1000;
        private int sortingOrder;

        public int SortingOrder
        {
            get
            {
                if (IsKeepSortingOrderWindow)
                {
                    if (isRenewalUI) return KeepSortingOrder;

                    InitCanvas();
                    return Canvas.sortingOrder;
                }

                return sortingOrder;
            }
            set
            {
                sortingOrder = value;
                SetSortingOrder();
            }
        }

        public bool IsKeepSortingOrderWindow { get; set; } = false;

        private Transform windowsPool;

        [SerializeField] private List<UIComponent> uiComponents;
        [SerializeField] private TransformList subWindowSources;
        [SerializeField] private TransformList subWindowParents;
        public TransformList SubWindowSources => subWindowSources;

        public virtual UIType eType => UIType.None;

        private bool isInitializeSubWindows { get; set; }

        public bool IsOpen => gameObject.activeSelf;

        public RectTransform UIWindowRectTransform { get; protected set; }
        
        protected virtual void Awake()
        {
            InitCanvas();
            UIWindowRectTransform = GetComponent<RectTransform>();
            InitializeSubWindows();
        }

        private void InitCanvas()
        {
            try
            {
                if (Canvas == null) Canvas = GetComponent<Canvas>();
            }
            catch (Exception e)
            {
                Debug.LogWarning("UI Window Init Canvas Error :" + e);
            }
        }

        public void SetSortingOrder()
        {
            InitCanvas();
            try
            {
                if (Canvas != default) Canvas.sortingOrder = SortingOrder;
            }
            catch (Exception e)
            {
                Debug.LogWarning("UI Window Init Canvas Error :" + e);
                throw;
            }
        }

        public void UpdateCanvasSortingOrder(int standardOrder, int order)
        {
            if (this == null)
                return;
            if (SortingOrder > standardOrder) SortingOrder -= order;

            SetSortingOrder();
        }

        private void InitializeSubWindows()
        {
            if (isInitializeSubWindows)
                return;

            foreach (var subWindow in subWindowSources) manager.InitSubWindow(subWindow);
            isInitializeSubWindows = true;
        }

        protected virtual void OnDestroy()
        {
            foreach (var subWindow in subWindowSources) manager.ReleaseSubWindow(subWindow);
            subWindowSources.Clear();
            RemoveAllUIEventListeners();
            if (manager != null) manager.DestroyWindow(this);

            if (AddressableHandle.IsValid())
                Addressables.Release(AddressableHandle);
        }

        private List<UIComponent> UIComponents
        {
            get => uiComponents;
            set => uiComponents = value;
        }

        public void SetUIComponents(List<UIComponent> components)
        {
            UIComponents = components;
        }

        public void SetSubWindowTransforms(TransformList transforms)
        {
            subWindowSources = transforms;
        }

        public void SetSubWindowParents(TransformList transforms)
        {
            subWindowParents = transforms;
        }

        public Component GetUI(int index)
        {
            if (UIComponents == null)
                return null;

            if (UIComponents.Count <= index)
                return null;

            return UIComponents[index].component;
        }

        public int GetUICount => UIComponents?.Count ?? 0;

        public void RemoveAllUIEventListeners()
        {
            if (UIComponents == null)
                return;

            foreach (var uiComponent in UIComponents) uiComponent?.RemoveAllListeners();
        }

        protected UIWindow AddSubWindow(int index)
        {
            if (subWindowSources == null)
                return null;

            if (subWindowSources.Count <= index)
                return null;

            var subWindow = subWindowSources[index];
            var parent = subWindowParents[index];
            var newSubWindow = Instantiate(subWindow, parent, false);
            var uiWindow = newSubWindow.GetComponent<UIWindow>();

            return uiWindow;
        }

        protected void DeleteSubWindow(UIWindow ui)
        {
            ui.gameObject.SetActive(false);
            ui.transform.SetParent(null);
            Destroy(ui.gameObject);
        }

        public virtual void OnOpen()
        {
        }

        public virtual void OnClosing()
        {
        }

        protected virtual void OnHide(bool hide)
        {
            UIComponents.Where(p => p.type == UIComponent.Types.UIWindow).Select(p => p.component as UIWindow)
                .ForEach(p => p.OnHide(hide));
        }

        public virtual void Close()
        {
            manager?.Close(this);
        }

        public void Hide()
        {
            SetHide(true);
        }

        public void Show()
        {
            SetHide(false);
        }

        public void HideLowSortingOrder()
        {
            SetHideAndSaveVisibleState(true);
        }

        public void PrevVisibleState()
        {
            SetHideAndSaveVisibleState(m_isHide);
        }

        private bool m_isHide;
        public virtual bool IsShow => Canvas != default ? Canvas.enabled : gameObject.activeSelf;

        protected virtual void SetHide(bool hide)
        {
            m_isHide = hide;
            if (Canvas != default)
                Canvas.enabled = !hide;
            else
                gameObject.SetActive(!hide);

            OnHide(hide);
        }

        private void SetHideAndSaveVisibleState(bool hide)
        {
            if (Canvas != default)
                Canvas.enabled = !hide;
            else if (gameObject.activeSelf == hide)
                gameObject.SetActive(!hide);
        }

        public virtual void ResetWindow()
        {
        }


#if UNITY_EDITOR
        public void CheckCanvas()
        {
            if (Canvas == default)
                Canvas = GetComponent<Canvas>();
        }
#endif
    }
}