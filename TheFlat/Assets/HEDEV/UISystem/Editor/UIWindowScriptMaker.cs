using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Proto.Util;
using TMPro;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using UnityEngine.UI;
using Type = Proto.Util.Type;

#if UNITY_EDITOR
namespace Proto.UISystem
{
    public class UIWindowScriptMaker : EditorWindow
    {
        private const string BaseDirectory = "Assets/Scripts/UI";
        private const string DefaultNamespaceName = "UI";
        private bool _analyzeFold;
        private bool _forceGenerate;

        private GameObjectTree _gameObjectTree;
        private bool _includeInactiveObject;
        private bool _includeUIWindowObject;
        private Vector2 _scrollPos;
        private UIDataHolder _uiDataHolder;

        private void OnGUI()
        {
            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            GuiTargetTree();
            GuiAnalyze();
            GuiBuild();
            GuiAttach();
            EditorGUILayout.EndScrollView();
        }

        // [MenuItem("Window/UI Window Script Maker (Old)")]
        // public static void ShowWindow()
        // {
        //     GetWindow(typeof(UIWindowScriptMaker));
        // }

        public static void OpenWindow(UIWindow window)
        {
            var maker = GetWindow(typeof(UIWindowScriptMaker)) as UIWindowScriptMaker;
            if (maker._gameObjectTree == null)
                maker._gameObjectTree = CreateInstance<GameObjectTree>();
            maker._gameObjectTree.Root.Target = window.gameObject;
            foreach (var sub in window.SubWindowSources) maker._gameObjectTree.Root.AddChild().Target = sub.gameObject;
            maker._includeInactiveObject = true;
            maker.Analyze();
            maker._analyzeFold = true;
            var coms = maker._uiDataHolder.uiDatas[0].components;
            coms.ForEach(p => p.enable = false);
            for (var i = 0; i < window.GetUICount; i++)
            {
                var ui = window.GetUI(i);
                if (ui == null)
                    continue;
                var target = coms.FirstOrDefault(p => p.uiComponent.component.gameObject == ui.gameObject);
                if (target != null)
                    target.enable = true;
            }
        }

        private void GuiTargetTree()
        {
            if (_gameObjectTree == null)
                _gameObjectTree = CreateInstance<GameObjectTree>();

            GUILayout.Label("Target Settings", EditorStyles.boldLabel);
            var editor = Editor.CreateEditor(_gameObjectTree);
            editor.OnInspectorGUI();
        }

        private void GuiAnalyze()
        {
            if ((_uiDataHolder?.uiDatas?.Count ?? 0) == 0)
                _analyzeFold = false;

            GUILayout.Space(10);
            GUILayout.Label("Analyze", EditorStyles.boldLabel);
            _includeInactiveObject = GUILayout.Toggle(_includeInactiveObject, "Include inactive gameObjects");
            _includeUIWindowObject = GUILayout.Toggle(_includeUIWindowObject, "Include UI window gameObjects");
            if (GUILayout.Button("Analyze"))
            {
                Analyze();
                EditorUtility.SetDirty(_uiDataHolder);
                _analyzeFold = true;
            }

            _analyzeFold = EditorGUILayout.Foldout(_analyzeFold, "--- analyzed result ---");
            if (_analyzeFold && _uiDataHolder != null)
            {
                var uiEditor = Editor.CreateEditor(_uiDataHolder);
                uiEditor.DrawDefaultInspectorWithoutScriptField();
            }
        }

        private void GuiBuild()
        {
            GUILayout.Space(10);
            GUILayout.Label("Build", EditorStyles.boldLabel);
            _forceGenerate = GUILayout.Toggle(_forceGenerate, "Force Regenerate All Files");

            if (GUILayout.Button("Build"))
            {
                Build();
                EditorUtility.SetDirty(_uiDataHolder);
                AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                CompilationPipeline.RequestScriptCompilation();
                Debug.Log("Build Done.");
            }
        }

        private void GuiAttach()
        {
            GUILayout.Space(10);
            GUILayout.Label("Attach", EditorStyles.boldLabel);
            if (GUILayout.Button("Attach"))
                try
                {
                    Attach();
                    Debug.Log("Attach Done.");
                }
                catch (Exception e)
                {
                    Debug.LogError("Attach Failed.\n" + e);
                }
        }

        private void Analyze()
        {
            if (_uiDataHolder == null)
                _uiDataHolder = CreateInstance<UIDataHolder>();

            _uiDataHolder.uiDatas = new List<UIData>();

            var node = _gameObjectTree.Root;

            Analyze(node, _uiDataHolder.uiDatas, _includeInactiveObject, _includeUIWindowObject);
        }

        private static void Analyze(GameObjectTree.Node node, List<UIData> uiDatas, bool includeInactives,
            bool includeUIWindowChild)
        {
            var uiData = new UIData();
            uiDatas.Add(uiData);

            uiData.target = node.Target;
            var ignores = node.Children.Select(p => p.Target).ToList();
            CollectUIComponents(node.Target.transform, uiData, ignores, includeInactives, includeUIWindowChild);

            foreach (var child in node.Children)
                if (includeUIWindowChild || child.Target.GetComponent<UIWindow>() == null)
                    Analyze(child, uiDatas, includeInactives, includeUIWindowChild);
        }

        public static void CollectUIComponents(Transform tran, UIData data, List<GameObject> ignores,
            bool includeInactives, bool includeUIWindowChild)
        {
            var ui = FindUICompoenet(tran);
            if (ui != null && tran.gameObject != data.target)
            {
                data.components.Add(ui);
                if (includeUIWindowChild == false && ui.uiComponent.type == UIWindow.UIComponent.Types.UIWindow)
                    return;
            }

            var count = tran.childCount;
            for (var i = 0; i < count; i++)
            {
                var child = tran.GetChild(i);
                if (child != null)
                {
                    var obj = child.gameObject;
                    if (obj.activeInHierarchy == false && !includeInactives)
                        continue;
                    if (ignores.Find(p => p.Equals(obj)))
                        continue;
                    CollectUIComponents(obj.transform, data, ignores, includeInactives, includeUIWindowChild);
                }
            }
        }

        public static UIComponent FindUICompoenet(Transform tran)
        {
            var components = tran.GetComponents<Component>();
            UIComponent component = null;
            foreach (var com in components)
            {
                var temp = CreateUIComponent(com);
                if (temp == null)
                    continue;

                var type = temp.uiComponent.type;
                if (type == UIWindow.UIComponent.Types.Image
                    || type == UIWindow.UIComponent.Types.RawImage
                    || type == UIWindow.UIComponent.Types.Animator)
                {
                    if (component == null || type > component.uiComponent.type)
                        component = temp;
                    continue;
                }

                component = temp;
                break;
            }

            return component;
        }

        public static UIComponent CreateUIComponent(Component component)
        {
            return component switch
            {
                Image ui => new UIComponent(UIWindow.UIComponent.Types.Image, ui),
                RawImage ui => new UIComponent(UIWindow.UIComponent.Types.RawImage, ui),
                Text ui => new UIComponent(UIWindow.UIComponent.Types.Text, ui),
                TextMeshProUGUI ui => new UIComponent(UIWindow.UIComponent.Types.TextMeshProUGUI, ui),
                Button ui => new UIComponent(UIWindow.UIComponent.Types.Button, ui),
                Toggle ui => new UIComponent(UIWindow.UIComponent.Types.Toggle, ui),
                Slider ui => new UIComponent(UIWindow.UIComponent.Types.Slider, ui),
                Scrollbar ui => new UIComponent(UIWindow.UIComponent.Types.Scrollbar, ui),
                Dropdown ui => new UIComponent(UIWindow.UIComponent.Types.Dropdown, ui),
                TMP_Dropdown ui => new UIComponent(UIWindow.UIComponent.Types.TMP_Dropdown, ui),
                InputField ui => new UIComponent(UIWindow.UIComponent.Types.InputField, ui),
                ScrollRect ui => new UIComponent(UIWindow.UIComponent.Types.ScrollRect, ui),
                UIGameObject ui => new UIComponent(UIWindow.UIComponent.Types.UIGameObject, ui),
                TransText ui => new UIComponent(UIWindow.UIComponent.Types.TransText, ui),
                UIChildSelector ui => new UIComponent(UIWindow.UIComponent.Types.UIChildSelector, ui),
                UIWindow ui => new UIComponent(UIWindow.UIComponent.Types.UIWindow, ui),
                Animator ui => new UIComponent(UIWindow.UIComponent.Types.Animator, ui),
                UIComponentReferences ui => new UIComponent(UIWindow.UIComponent.Types.UIComponentReferences, ui),
                _ => null
            };
        }

        private void Build()
        {
            if (_uiDataHolder.uiDatas.Count == 0)
                return;

            BuildClass(_gameObjectTree.Root);
        }

        private void BuildClass(GameObjectTree.Node node)
        {
            var className = node.Target.name;
            var targetDirectory = BaseDirectory;
            var targetPath = Path.Combine(targetDirectory, className + ".cs");
            var targetGeneratedPath = Path.Combine(targetDirectory, className + "_generated.cs");
            Directory.CreateDirectory(targetDirectory);
            WriteFile(node, targetGeneratedPath, true);
            if (_forceGenerate || File.Exists(targetPath) == false)
                WriteFile(node, targetPath, false);
            node.Children.ForEach(p =>
            {
                if (p.Target.GetComponent<UIWindow>() == null)
                    BuildClass(p);
            });
        }

        private void WriteFile(GameObjectTree.Node node, string path, bool isGenerated)
        {
            const int indentLevel = 1;
            using var writer = new StreamWriter(path);
            if (isGenerated)
                writer.WriteLine("//Generated file. Don't modify this script.");

            writer.WriteLine("using UnityEngine;");
            writer.WriteLine("using UnityEngine.UI;");
            writer.WriteLine("using Proto.UISystem;");
            writer.WriteLine("using System.Collections.Generic;");
            writer.WriteLine("using TMPro;");
            writer.WriteLine("");
            writer.WriteLine("namespace " + DefaultNamespaceName);
            writer.WriteLine("{");
            WriteClass(writer, node, indentLevel, isGenerated);
            writer.WriteLine("}");
            writer.WriteLine("");
            writer.Close();
        }

        private UIData GetUIData(GameObjectTree.Node node)
        {
            var result = _uiDataHolder.uiDatas.Find(p => p.target.Equals(node.Target));
            return result;
        }

        private void WriteClass(StreamWriter writer, GameObjectTree.Node node, int indentLevel, bool isGenerated)
        {
            var className = node.Target.name;
            var indent = GetIndentString(indentLevel);
            writer.WriteLine($"{indent}public partial class {className} : UIWindow");
            writer.WriteLine($"{indent}{{");
            if (isGenerated) WriteComponentEnumAndProperty(writer, node, indentLevel + 1);
            if ((node.Children?.Count ?? 0) > 0)
                if (isGenerated)
                {
                    WriteSubWindowEnum(writer, node, indentLevel + 1);
                    WriteSubWindowList(writer, node, indentLevel + 1);
                    WriteSubWindowAdder(writer, node, indentLevel + 1);
                }

            writer.WriteLine($"{indent}}}");
            writer.WriteLine("");
        }

        private void WriteComponentEnumAndProperty(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var uiData = GetUIData(node);
            var uiComponets = uiData.components;
            writer.WriteLine($"{indent}public enum UIComponents : int");
            writer.WriteLine($"{indent}{{");
            foreach (var component in uiComponets.Where(component => component.enable != false))
                writer.WriteLine($"{indent}    {component.uiComponent.component.name},");
            
            writer.WriteLine($"{indent}}}");
            writer.WriteLine("");
            var index = 0;
            foreach (var component in uiComponets)
            {
                if (component.enable == false)
                    continue;

                var type = component.uiComponent.type.ToString();
                if (component.uiComponent.type == UIWindow.UIComponent.Types.UIWindow || component.uiComponent.type ==
                    UIWindow.UIComponent.Types.UIComponentReferences)
                    type = component.uiComponent.component.GetType().Name;
                writer.WriteLine(
                    $"{indent}public {type} {component.uiComponent.component.name} => GetUI({index}) as {type};");
                index++;
            }
        }

        private void WriteSubWindowEnum(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var subWindows = node.Children;
            writer.WriteLine($"{indent}private enum SubWindows : int");
            writer.WriteLine($"{indent}{{");
            foreach (var window in subWindows)
                writer.WriteLine($"{indent}    {window.Target.name},");
            writer.WriteLine($"{indent}}}");
            writer.WriteLine("");
        }

        private void WriteSubWindowList(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var subWindows = node.Children;
            foreach (var window in subWindows)
                writer.WriteLine(
                    $"{indent}public List<{window.Target.name}> {window.Target.name}s = new List<{window.Target.name}>();");
        }

        private void WriteSubWindowAdder(StreamWriter writer, GameObjectTree.Node node, int indentLevel)
        {
            var indent = GetIndentString(indentLevel);
            var subWindows = node.Children;
            foreach (var window in subWindows)
            {
                writer.WriteLine($"{indent}public {window.Target.name} Add{window.Target.name}()");
                writer.WriteLine($"{indent}{{");
                writer.WriteLine(
                    $"{indent}    var window = AddSubWindow((int)SubWindows.{window.Target.name}) as {window.Target.name};");
                writer.WriteLine($"{indent}    {window.Target.name}s.Add(window);");
                writer.WriteLine($"{indent}    return window;");
                writer.WriteLine($"{indent}}}");
                writer.WriteLine("");
            }
        }

        private string GetIndentString(int indentLevel)
        {
            return new string(' ', indentLevel * 4);
        }

        private void Attach()
        {
            const string baseName = DefaultNamespaceName + ".";
            var node = _gameObjectTree.Root;
            var typeName = baseName + node.Target.name;
            var t = Type.GetType(typeName);
            if (t == null)
                return;
            
            Attach(baseName, node);
            Debug.Log("Attach Done.");
            EditorUtility.SetDirty(node.Target);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void Attach(string baseName, GameObjectTree.Node node)
        {
            var uiData = GetUIData(node);
            if (uiData == null)
                return;

            var target = node.Target;
            var typeName = baseName + target.name;
            var t = Type.GetType(typeName);
            var com = target.GetComponent(t);
            if (com == null)
                com = target.AddComponent(t);

            var window = com as UIWindow;
            window.SetUIComponents(uiData.components.Where(p => p.enable).Select(p => p.uiComponent).ToList());
            window.SetSubWindowTransforms(new TransformList(node.Children.Select(p => p.Target.transform).ToList()));
            window.SetSubWindowParents(
                new TransformList(node.Children.Select(p => p.Target.transform.parent).ToList()));

            //baseName = typeName + ".";
            foreach (var child in node.Children) Attach(baseName, child);
        }

        [Serializable]
        public class UIComponent
        {
            public bool enable = true;
            public UIWindow.UIComponent uiComponent;

            public UIComponent(UIWindow.UIComponent.Types type, Component component)
            {
                enable = true;
                uiComponent = new UIWindow.UIComponent(type, component);
            }
        }

        [Serializable]
        public class UIData
        {
            public GameObject target;
            public List<UIComponent> components = new();
        }

        public class UIDataHolder : ScriptableObject
        {
            public List<UIData> uiDatas;
        }
    }
}
#endif