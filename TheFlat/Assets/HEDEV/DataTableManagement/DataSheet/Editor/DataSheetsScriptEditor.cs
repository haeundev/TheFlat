using System;
using UnityEditor;
using UnityEngine;

namespace Datas
{
    public abstract class DataSheetsScriptEditor : Editor
    {
        public abstract string FileID { get; }
        public virtual string[] SheetNames => default;
        bool editForTest;
        bool tryRebuild;
        public abstract Type SubClassType { get; }
        public virtual DataScript.DataType DataType => DataScript.DataType.Table;

        public override void OnInspectorGUI()
        {
            EditorGUI.BeginDisabledGroup(!editForTest);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
            EditorGUI.BeginDisabledGroup(tryRebuild);
            if (GUILayout.Button("Download"))
            {
                StartDownload();
            }
            OnAdditiveInspectorGUI();
            EditorGUI.EndDisabledGroup();
            editForTest = GUILayout.Toggle(editForTest, "Edit for test.");
        }

        protected virtual void OnAdditiveInspectorGUI()
        {
        }

        public abstract void ClearAssetData();
        public abstract void SetAssetData(string sheetName, string json);

        public void StartRebuild()
        {
            tryRebuild = true;
            OpenRequest(Rebuild);
        }

        public void StartDownload()
        {
            OpenRequest(DownLoad);
        }

        public void Rebuild(Editors.Datas.ClassBuilder builder, string sheetName, string json)
        {
            tryRebuild = false;
            var type = SubClassType;
            // builder.GenerateClasses(type.Namespace, Editors.DataScriptMaker.GetScriptDirectory(builder.DataType));
            Debug.Log($"Rebuild Done. ({SheetNames})");
        }

        public void DownLoad(Editors.Datas.ClassBuilder builder, string sheetName, string json)
        {
            SetAssetData(sheetName, json);
            var type = target.GetType();
            var method = type.GetMethod("OnDownloaded");
            method?.Invoke(target, null);
            Editors.DataScriptMaker.SaveAsset(target as ScriptableObject);
            EditorUtility.SetDirty(target as ScriptableObject);
            Debug.Log($"DownLoad Done. ({SheetNames})");
        }

        public void OpenRequest(Action<Editors.Datas.ClassBuilder, string, string> action)
        {
            ClearAssetData();
            
            DataScript.OpenRequest(FileID, target.GetType().ToString(), SheetNames, action);
        }
    }
}
