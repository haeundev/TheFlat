using System;
using System.Collections;
using System.Collections.Generic;
using Proto.Enums;
using Proto.UISystem;
using Proto.Util;
using UnityEngine;
using Type = System.Type;

namespace UI
{
    public abstract class UIController
    {
        protected Action<UIController> _completeWindowSetting;
        private bool _waitClose;
        public UIWindow window;
        public int ID;
        public bool IsOpenedWindow;

        protected UIController()
        {
            SetChildrenControllers();
        }

        public abstract Type UIWindowType { get; }
        public virtual UIType UIType => UIType.None;
        public Transform WindowTransform => window.transform;
        public WindowOption WindowOption { get; private set; } = WindowOption.None;

        public bool LoadComplete { get; private set; }

        public int SortingOrder
        {
            get
            {
                if (window == null)
                    return int.MaxValue;
                return window.SortingOrder;
            }
        }

        public int SubWindowCount => m_childrenBodies.Count;
        
        public virtual void SetWindowOption(int id, Action<UIController> completeWindowSetting, WindowOption option)
        {
            ID = id;
            _completeWindowSetting = completeWindowSetting;
            WindowOption = option;
        }


        protected void CreateWindow<T>() where T : UIWindow
        {
            UIWindowManager.OpenWindow<T>(this, InitWindow, ID, SubWindowCount, WindowOption);
        }

        public virtual void InitWindow(UIWindow window)
        {
            this.window = window;
            this.window.isRenewalUI = true;
            this.window.SetSortingOrder();
            UIWindowManager.GetSubWindows(() =>
            {
                if (!_waitClose)
                {
                    Awake();
                }

                LoadComplete = true;
            }, this);
        }

        protected abstract void Awake();

        private void CheckedDisableUI()
        {
            var inputBlock = window.GetComponent<UIInputBlock>();
            if (inputBlock != null) inputBlock.OnOpenAndCheckedDisable();
        }

        public virtual void Hide()
        {
            if (window == null) return;
            window.gameObject.SetActive(false);
            OnHide();
        }

        public virtual void Show()
        {
            window.gameObject.SetActive(true);
            CheckedDisableUI();
            OnShow();
        }

        public virtual void TemporaryHide()
        {
            window.HideLowSortingOrder();
        }

        public virtual void ReturnPrevOpenState()
        {
            window.PrevVisibleState();
        }

        public virtual void Close()
        {
            UIWindowService.Close(this);
        }

        public void CloseAction()
        {
            _waitClose = true;
            if (LoadComplete)
            {
                foreach (var body in m_childrenBodies) body.CloseAction();
                OnDestroyAfterClose();
                window.Close();
                OnClose();
            }
            else
            {
                CoroutineManager.ExecuteCoroutine(DelayClose());
            }
        }

        public void OnDestroyAfterClose()
        {
            UIWindowManager.CloseUIWindowBody(this);
        }

        private IEnumerator DelayClose()
        {
            yield return new WaitUntil(() => LoadComplete);
            CloseAction();
            window.Close();
            OnClose();
        }

        #region Tree

        protected List<UIController> m_childrenBodies = new();
        public List<UIController> ChildrenBodies => m_childrenBodies;

        protected virtual void SetChildrenControllers()
        {
        }

        protected void AddChildBody(UIController body)
        {
            m_childrenBodies.Add(body);
        }

        #endregion

        #region Window Event

        public virtual void OnOpen()
        {
            if (LoadComplete)
                foreach (var body in m_childrenBodies)
                    body.OnOpen();
        }

        public virtual void OnClose()
        {
        }

        public virtual void OnShow()
        {
            if (LoadComplete)
                foreach (var body in m_childrenBodies)
                    if (body.window.IsOpen)
                        body.OnShow();

            IsOpenedWindow = true;
        }

        public virtual void OnHide()
        {
            if (LoadComplete)
                foreach (var body in m_childrenBodies)
                    if (body.window.IsOpen)
                        body.OnHide();

            IsOpenedWindow = false;
        }

        public virtual void OnDestroy()
        {
            if (LoadComplete)
                foreach (var body in m_childrenBodies)
                    body.OnDestroy();
            else
                CoroutineManager.ExecuteCoroutine(DelayOnDestroy());
        }

        private IEnumerator DelayOnDestroy()
        {
            yield return new WaitUntil(() => LoadComplete);
            foreach (var body in m_childrenBodies) body.OnDestroy();
        }

        #endregion
    }
}