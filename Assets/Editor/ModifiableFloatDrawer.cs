using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

[CustomPropertyDrawer(typeof(ModifiableFloat))]
public class ModifiableFloatDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty defaultVal = property.FindPropertyRelative("defaultValue");
        SerializedProperty min = property.FindPropertyRelative("min");
        SerializedProperty max = property.FindPropertyRelative("max");
        EditorGUI.PropertyField(position, defaultVal, new GUIContent(ObjectNames.NicifyVariableName(property.name)));
        defaultVal.floatValue = Mathf.Clamp(defaultVal.floatValue, min.floatValue, max.floatValue);
    }
}