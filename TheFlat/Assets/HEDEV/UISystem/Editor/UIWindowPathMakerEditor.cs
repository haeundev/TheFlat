#if UNITY_EDITOR

using UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Proto.UISystem
{
    [CustomEditor(typeof(UIWindowPathMaker))]
    public class UIWindowPathMakerEditor : Editor
    {
        private UIWindowPathMaker _target;

        private const string EditorScenePath = "Assets/Scenes/UIPathSetting.unity";
    
        private void OnEnable()
        {
            _target = (UIWindowPathMaker) target;
        }

        [MenuItem("Tools/UI/UI Path Setting Scene", false, priority = 99)]
        public static void OpenSettingScene()
        {
            EditorSceneManager.OpenScene(EditorScenePath);
        }
    
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            OnCollect();
        }

        private void OnCollect()
        {
            if (GUILayout.Button("Collect"))
            {
                serializedObject.Update();
                var windows = serializedObject.FindProperty("windows");
                windows.arraySize = _target.transform.childCount;
                for (var i = 0; i < _target.transform.childCount; ++i)
                {
                    try
                    {
                        var window = _target.transform.GetChild(i).GetComponent<UIWindow>();
                        if (window != null)
                        {
                            var element = windows.GetArrayElementAtIndex(i);
                            element.objectReferenceValue = window;
                        }
                        else
                        {
                            Debug.LogError(_target.transform.GetChild(i).gameObject.name + " does not Contain UI window.");
                        }
                    }
                    catch
                    {
                        Debug.LogError(_target.transform.GetChild(i).gameObject.name + " does not Contain UI window.");
                        break;
                    }
               
                }
            
                serializedObject.ApplyModifiedProperties();
                _target.Collect();
                _target.Join();
            }
        }
    }
}

#endif