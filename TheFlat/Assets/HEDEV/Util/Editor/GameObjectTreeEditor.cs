using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameObjectTree))]
public class GameObjectTreeEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var tree = target as GameObjectTree;
        var root = tree.Root;
        Display(root);
    }

    private void Display(GameObjectTree.Node node)
    {
        EditorGUILayout.BeginHorizontal();
        node.Target = EditorGUILayout.ObjectField(node.Target, typeof(GameObject), true) as GameObject;
        if (node.Parent == null)
        {
            if (GUILayout.Button("Add child", GUILayout.Width(150)))
            {
                node.AddChild();
                EditorUtility.SetDirty(target);
            }
        }
        else
        {
            if (GUILayout.Button("Add child", GUILayout.Width(80)))
            {
                node.AddChild();
                EditorUtility.SetDirty(target);
            }

            if (GUILayout.Button("delete", GUILayout.Width(70)))
            {
                node.Delete();
                EditorUtility.SetDirty(target);
            }
        }

        EditorGUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Space(30);
        GUILayout.BeginVertical();

        foreach (var child in node.Children)
            Display(child);

        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }
}