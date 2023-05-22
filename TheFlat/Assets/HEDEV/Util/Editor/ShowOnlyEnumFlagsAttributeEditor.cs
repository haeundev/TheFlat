using Proto.UISystem;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ShowOnlyEnumFlagsAttribute))]
public class ShowOnlyEnumFlagsDrawer : PropertyDrawer
{
    private float propertyHeight = 0f;
    private string stringToPrint = string.Empty;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Enum)
            return 0f;

        var strings = ((WindowOption)property.intValue).ToString().Split(',');
        stringToPrint = string.Join("\n", strings).Replace("\n ", "\n");
        return propertyHeight = EditorGUIUtility.singleLineHeight * strings.Length;
    }

    public override void OnGUI(Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Enum)
            return;

        GUI.enabled = false;

        EditorGUI.LabelField(position, label);

        position.x += EditorGUIUtility.labelWidth + 2f;
        position.height = propertyHeight;
        EditorGUI.TextArea(position, stringToPrint);

        GUI.enabled = true;
    }
}