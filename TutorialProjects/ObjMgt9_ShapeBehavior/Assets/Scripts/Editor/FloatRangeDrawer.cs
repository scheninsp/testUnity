using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(FloatRange))]
public class FloatRangeDrawer : PropertyDrawer
{
    public override void OnGUI(
        Rect position, SerializedProperty property, GUIContent label)
    {
        int originalIndentLevel = EditorGUI.indentLevel;
        float originalLabelWidth = EditorGUIUtility.labelWidth;

        EditorGUI.BeginProperty(position, label, property);
        //set prefix label not highlighted when selected
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive),label);
        //return remaining space after setting prefix label

        position.width = position.width / 2f;
        EditorGUIUtility.labelWidth = position.width / 2f;
        EditorGUI.indentLevel = 0;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("min"));
        position.x += position.width;
        EditorGUI.PropertyField(position, property.FindPropertyRelative("max"));
        EditorGUI.EndProperty();

        EditorGUI.indentLevel = originalIndentLevel;
        EditorGUIUtility.labelWidth = originalLabelWidth;
    }
}
