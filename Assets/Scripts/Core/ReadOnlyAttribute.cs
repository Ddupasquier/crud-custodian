// This attribute lets us mark Inspector fields as read-only for display purposes.
// Place this file in any Editor or Scripts folder; Unity will pick it up automatically.
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// Marks a serialized field as read-only in the Unity Inspector.
/// The field is still serialized and visible, but cannot be edited at design time.
/// Use this to display runtime-computed or constant values for quick reference.
/// </summary>
public class ReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        bool previousGuiEnabled = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(position, property, label, includeChildren: true);
        GUI.enabled = previousGuiEnabled;
    }
}
#endif
