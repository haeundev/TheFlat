using System;
using System.Collections.Generic;
using Proto.UISystem;
using UnityEngine;

namespace UI
{
    public class UIWindowController : MonoBehaviour
    {
        [SerializeField] private UIContainer m_uiContainer = null;
        private Dictionary<System.Type, string> m_pathContainer = null;

        private void Awake()
        {
            Init();
        }

        private void Init()
        {
            InitContainer();
        }

        private void InitContainer()
        {
            m_pathContainer = new Dictionary<Type, string>();

            foreach (var uiKeyValue in m_uiContainer.UIList)
            {
                m_pathContainer[uiKeyValue.Window.GetType()] = uiKeyValue.Path;
            }
        }

        public string Find<T>() where T : UIWindow
        {
            string value = null;
            m_pathContainer.TryGetValue(typeof(T), out value);
            return value;
        }
    }
}