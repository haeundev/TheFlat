#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Proto.UISystem
{
    #region SubWindows

    public class UISubWindowBody : ScriptableObject
    {
        [System.Serializable]
        public class Node
        {
            public Object targetScript;
        }

        public List<Node> nodes = new List<Node>();

        public void CreateNewNode()
        {
            var Node = new Node();
            nodes.Add(Node);
        }

        public void DeleteNode(Node node)
        {
            if (nodes.Contains(node))
            {
                nodes.Remove(node);
            }
        }
    }

    #endregion

    public class UIControllerMaker : EditorWindow
    {
        private readonly string defaultNamespaceName = "UI";
        private const string scriptPath = "Assets/Scripts/UI/UIControllers/";
        public GameObject TargetUIWindowObject = null;

        private UISubWindowBody subWindowBody = null;

        private string m_className = "";

        private string uiWindowType = "";

        [MenuItem("Tools/UI/UI Controller Maker", false , 2)]
        public static void ShowWindow()
        {
            GetWindow(typeof(UIControllerMaker));
        }

        private void OnGUI()
        {
            ClassNameGUI();
            TargetUIGUI();
            SubWindowGUI();
            WriteGUI();
        }

        private void ClassNameGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("클래스 이름 : ");
            m_className = EditorGUILayout.TextField(m_className);
            EditorGUILayout.EndHorizontal();
        }

        private void TargetUIGUI()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Target UI Window Prefab ");
            TargetUIWindowObject =
                (GameObject) EditorGUILayout.ObjectField(TargetUIWindowObject, typeof(GameObject), true);
            GUILayout.EndHorizontal();
        }

        private void SubWindowGUI()
        {
            if (subWindowBody == null) subWindowBody = CreateInstance<UISubWindowBody>();
            GUILayout.Label("Sub UI Controller");
            var editor = Editor.CreateEditor(subWindowBody);
            editor.OnInspectorGUI();
        }

        private void WriteGUI()
        {
            if (GUILayout.Button("Build"))
            {
                WriteFile();
                WriteGeneratorFile();
            }
        }

        #region Wirte Class

        void WriteFile()
        {
            var path = scriptPath + m_className + "_Controller.cs";
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("using System;");
                writer.WriteLine("");
                writer.WriteLine("namespace " + defaultNamespaceName);
                writer.WriteLine("{");
                WriteClass(writer);
                writer.WriteLine("}");
                writer.WriteLine("");
                writer.Close();
            }
            AssetDatabase.Refresh();
        }

        private void WriteClass(StreamWriter writer)
        {
            uiWindowType = TargetUIWindowObject.GetComponent<UIWindow>().GetType().ToString();
            writer.WriteLine($"    public partial class {m_className}_Controller : UIController");
            writer.WriteLine("    {");
            writer.WriteLine("");
            WriteOnCompleteSetting(writer);
            writer.WriteLine("    }");
        }

        private void WriteOnCompleteSetting(StreamWriter writer)
        {
            writer.WriteLine("        // UiWindow Create Completed Call Awake");
            writer.WriteLine("        protected override void Awake()");
            writer.WriteLine("        {");
            writer.WriteLine("            Show();");
            writer.WriteLine("            _completeWindowSetting?.Invoke(this);");
            writer.WriteLine("        }");
        }

        #endregion

        #region Generator

        private void WriteGeneratorFile()
        {
            var path = scriptPath + m_className + "_Controller_Generator.cs";
            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine($"//This is Auto Generated Code, Don't modify this script.");
                writer.WriteLine("using System;");
                writer.WriteLine("using Proto.UISystem;");
                writer.WriteLine("");
                writer.WriteLine("namespace " + defaultNamespaceName);
                writer.WriteLine("{");
                WriteGeneratorClass(writer);
                writer.WriteLine("}");
                writer.WriteLine("");
                writer.Close();
            }
            Debug.Log($"{path}  Create Complete");
            AssetDatabase.Refresh();
        }

        private void WriteGeneratorClass(StreamWriter writer)
        {
            uiWindowType = TargetUIWindowObject.GetComponent<UIWindow>().GetType().ToString();
            writer.WriteLine($"    //This is Auto Generated Code");
            writer.WriteLine($"    public partial class {m_className}_Controller : UIController");
            writer.WriteLine("    {");
            writer.WriteLine("");
            writer.WriteLine($"        public override System.Type UIWindowType => typeof({uiWindowType});");
            writer.WriteLine($"        public {uiWindowType} Window => window as {uiWindowType};");
            WriteChildren(writer);
            WriteOpen(writer);
            writer.WriteLine("    }");
        }

        private void WriteChild(StreamWriter writer)
        {
            foreach (var subWindow in subWindowBody.nodes)
            {
                var subWindowType = subWindow.targetScript.name;
                var typeName = subWindowType.ToLower();

                writer.WriteLine($"            _{typeName} = new {subWindowType}();");
                writer.WriteLine($"            AddChildBody(_{typeName});");
            }
        }

        private void WriteChildren(StreamWriter writer)
        {
            if (subWindowBody.nodes.Count > 0)
            {
                foreach (var subWindow in subWindowBody.nodes)
                {
                    var subWindowType = subWindow.targetScript.name;
                    writer.WriteLine($"        private {subWindowType} _{subWindowType.ToLower()};");
                }

                writer.WriteLine("        protected override void SetChildrenControllers()");
                writer.WriteLine("        {");
                WriteChild(writer);
                writer.WriteLine("        }");
            }
        }

        private void WriteOpen(StreamWriter writer)
        {
            writer.WriteLine("        public override void SetWindowOption(int id, Action<UIController> completeWindowSetting, WindowOption option)");
            writer.WriteLine("        {");
            writer.WriteLine("            base.SetWindowOption(id, completeWindowSetting, option);");
            writer.WriteLine($"           CreateWindow<{uiWindowType}>();");
            writer.WriteLine("        }");
        }
        
        #endregion
    }
}
#endif