using UnityEditor;
using UnityEngine;

namespace Proto.UISystem
{
    [CustomEditor(typeof(UIWindow), true)]
    public class UIWindowEditor : UnityEditor.Editor
    {
    }


    [CustomPropertyDrawer(typeof(UIWindow.UIComponent))]
    public class UIWindowComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("component"), GUIContent.none);
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
        }
    }
    
    [CustomPropertyDrawer(typeof(UIWindowScriptMaker.UIComponent))]
    public class UIWindowScriptMakerComponentDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.indentLevel++;
            var enable = property.FindPropertyRelative("enable");
            EditorGUI.PropertyField(position, enable, GUIContent.none);
            EditorGUI.BeginDisabledGroup(!enable.boolValue);
            EditorGUI.PropertyField(position, property.FindPropertyRelative("uiComponent"), GUIContent.none);
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            EditorGUI.EndProperty();
            property.serializedObject.ApplyModifiedProperties();
        }
    }
}