using UnityEditor;
using UnityEngine;

// Shows the piece type as the list entry title for InGameUIManager's Defeated Count Slots
// and Alive Count Slots, instead of Unity's default "Element 0", "Element 1"...
[CustomPropertyDrawer(typeof(InGameUIManager.DefeatedCountSlot))]
public class DefeatedCountSlotDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string smartLabel = InspectorListLabelUtility.GetSmartLabel(property, label, "pieceType");
        EditorGUI.PropertyField(position, property, new GUIContent(smartLabel), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}

[CustomPropertyDrawer(typeof(InGameUIManager.AliveCountSlot))]
public class AliveCountSlotDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string smartLabel = InspectorListLabelUtility.GetSmartLabel(property, label, "pieceType");
        EditorGUI.PropertyField(position, property, new GUIContent(smartLabel), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}
