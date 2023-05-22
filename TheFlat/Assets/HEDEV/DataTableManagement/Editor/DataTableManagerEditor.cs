using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Proto.Data
{
    [CustomEditor(typeof(DataTableManager))]
    public class DataTableManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Collect"))
            {
                var obj = target as DataTableManager;
                //obj.Collect();
                EditorUtility.SetDirty(obj);
            }

            if (GUILayout.Button("Rebuild All"))
            {
                var obj = target as DataTableManager;
                var @type = target.GetType();
                var fields = @type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var field in fields)
                {
                    dynamic data = field.GetValue(target);
                    dynamic dataEditor = CreateEditor(data);
                    Debug.Log($"try rebuild : {data.GetType().Name}");
                    dataEditor.StartRebuild();
                }

                EditorUtility.SetDirty(obj);
            }

            if (GUILayout.Button("Download All"))
            {
                var obj = target as DataTableManager;
                var @type = target.GetType();
                var fields = @type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                foreach (var field in fields)
                {
                    dynamic data = field.GetValue(target);
                    dynamic dataEditor = CreateEditor(data);
                    dataEditor.StartDownload();
                }

                EditorUtility.SetDirty(obj);
            }
        }
    }
}