using UnityEditor;

#if UNITY_EDITOR
namespace Proto.Util
{
    public static class EditorExtensions
    {
        public static void DrawDefaultInspectorWithoutScriptField(this Editor inspector)
        {
            EditorGUI.BeginChangeCheck();
            inspector.serializedObject.Update();
            var iterator = inspector.serializedObject.GetIterator();
            iterator.NextVisible(true);
            while (iterator.NextVisible(false))
                EditorGUILayout.PropertyField(iterator, true);
            inspector.serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}
#endif