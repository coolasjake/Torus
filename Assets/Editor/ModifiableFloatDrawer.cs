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
        //SerializedProperty endValue = property.FindPropertyRelative("_value");
        float percent = property.FindPropertyRelative("percentage").floatValue;
        float mult = property.FindPropertyRelative("multiplier").floatValue;
        float add = property.FindPropertyRelative("addition").floatValue;
        float value = ModifiableFloat.GenerateModifiedValue(defaultVal.floatValue, mult, percent, add, min.floatValue, max.floatValue);
        EditorGUI.PropertyField(position, defaultVal, new GUIContent(ObjectNames.NicifyVariableName(property.name) + " (" + value.ToString() + ")"));
        defaultVal.floatValue = Mathf.Clamp(defaultVal.floatValue, min.floatValue, max.floatValue);
    }
}