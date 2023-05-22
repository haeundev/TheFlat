using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using Proto.UISystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace UI
{
    public class UIWindowPathMaker : MonoBehaviour
    {
#if UNITY_EDITOR
        public UIContainer m_containerSO;
        public List<UIContainer> m_JoinContainerSOs;
        public List<UIWindow> windows = new List<UIWindow>();
        
        public void Collect()
        {
            if (m_containerSO == null) return;
            m_containerSO.UIList = new List<UIKeyValue>();
            foreach (var window in windows)
            {
                var uiKeyValue = new UIKeyValue();
                try
                {
                    uiKeyValue.Window = SetPrefab(window.gameObject, out uiKeyValue.Path);
                    if (FindKey(uiKeyValue.Window.GetType())) continue;
                    m_containerSO.UIList.Add(uiKeyValue);
                    window.gameObject.SetActive(false);
                }
                catch
                {
                    Debug.LogError("Checked Console");
                    break;
                }
               
            }
            EditorUtility.SetDirty(m_containerSO);
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        public void Join()
        {
            if (m_containerSO == null) return;
            if (m_JoinContainerSOs == null) return;

            foreach (var container in m_JoinContainerSOs)
            {
                foreach (var joinKeyValue in container.UIList)
                {
                    if (FindKey(joinKeyValue.Window.GetType())) continue;
                    m_containerSO.UIList.Add(joinKeyValue);
                }
            }
            EditorUtility.SetDirty(m_containerSO);
            AssetDatabase.SaveAssets();
        }

        public void CheckMissionComponent()
        {
            foreach (var uiWindow in windows)
            {
                if (uiWindow == null)
                {
                    Debug.LogError("UI Window Mission Component in [" + gameObject.name + "] Check UI Window");
                }
            }
        }

        private bool FindKey(Type type)
        {
            var index = 0;
            foreach (var keyValue in m_containerSO.UIList)
            {
                index++;
                if (keyValue.Window.GetType() == type)
                {
                    Debug.Log("***** 중첩된 UI Type 따라서 해당 UI는 컨테이너에 등록되지 않았습니다 : [Index : " + index + " ][ Type : " +
                              type.ToString() + " ]");
                    return true;
                }
            }

            return false;
        }

        private UIWindow SetPrefab(GameObject windowObject, out string path)
        {
            Debug.Log("Window Path : " +
                      AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(windowObject)));
            path = AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(windowObject));

            var FindPrefab = (GameObject) AssetDatabase.LoadAssetAtPath(
                AssetDatabase.GetAssetPath(PrefabUtility.GetCorrespondingObjectFromSource(windowObject)),
                typeof(GameObject));
            return FindPrefab.GetComponent<UIWindow>();
        }
#endif
    }
}