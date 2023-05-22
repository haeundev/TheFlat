using UnityEngine;
using UnityEditor;
using System;

namespace Proto.Data
{
    public abstract class DataScriptEditor : Editor
    {
        public abstract string FileID { get; }
        public virtual string SheetName => SubClassType.Name;
        private bool _editForTest;
        // private bool _uploadToServer;
        private bool _tryRebuild;
        public abstract Type SubClassType { get; }
        public virtual DataScript.DataType DataType => DataScript.DataType.Table;

        public override void OnInspectorGUI()
        {
            //_uploadToServer = GUILayout.Toggle(_uploadToServer, "uploadDataToServer");
            EditorGUI.BeginDisabledGroup(!_editForTest);
            base.OnInspectorGUI();
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Rebuild"))
            {
                StartRebuild();
            }
            GUILayout.Space(10);
            EditorGUI.BeginDisabledGroup(_tryRebuild);
            if (GUILayout.Button("Download"))
            {
                StartDownload();
            }
            OnAdditiveInspectorGUI();
            EditorGUI.EndDisabledGroup();
            _editForTest = GUILayout.Toggle(_editForTest, "Edit for test.");
        }

        protected virtual void OnAdditiveInspectorGUI()
        {
        }

        public abstract void SetAssetData(string json);

        public void StartRebuild()
        {
            _tryRebuild = true;
            OpenRequest(Rebuild);
        }

        public void StartDownload()
        {
            OpenRequest(DownLoad);
        }

        public void Rebuild(Editors.Datas.ClassBuilder builder, string json, string rawJson)
        {
            _tryRebuild = false;
            var type = SubClassType;
            builder.GenerateClasses(type.Namespace, Editors.DataScriptMaker.GetScriptDirectory(builder.DataType));
            Debug.Log($"Rebuild Done. ({SheetName})");
        }

        public void DownLoad(Editors.Datas.ClassBuilder builder, string json, string rawJson)
        {
            SetAssetData(json);
            var type = target.GetType();
            var method = type.GetMethod("OnDownloaded");
            method?.Invoke(target, null);
            Editors.DataScriptMaker.SaveAsset(target as ScriptableObject);
            EditorUtility.SetDirty(target as ScriptableObject);

            // if (_uploadToServer)
            // {
            //     var downloadEvent = AssetDatabase.LoadAssetAtPath<OnDownloadEvent>("Assets/Plugins/DataSheet/Editor/OnDownloadEvent.asset");
            //     downloadEvent.downloadEvent?.Invoke(new OnDownloadEventParam { SheetName = SheetName, RawJson = rawJson, ObjectJson = json });
            // }
            Debug.Log($"DownLoad Done. ({SheetName})");
        }

        public void OpenRequest(DataScript.OnOpenEvent action)
        {
            DataScript.OpenRequest(FileID, SheetName, DataType, action);
        }
    }
}
