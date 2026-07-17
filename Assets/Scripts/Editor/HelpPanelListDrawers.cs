using UnityEditor;
using UnityEngine;

// Shows the topic title / a description preview as the list entry title for HelpPanel's
// Topics and each topic's Pages, instead of Unity's default "Element 0", "Element 1"...
[CustomPropertyDrawer(typeof(HelpPanel.HelpTopic))]
public class HelpTopicDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string smartLabel = InspectorListLabelUtility.GetSmartLabel(property, label, "title");
        EditorGUI.PropertyField(position, property, new GUIContent(smartLabel), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}

[CustomPropertyDrawer(typeof(HelpPanel.HelpPage))]
public class HelpPageDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string smartLabel = InspectorListLabelUtility.GetSmartLabel(property, label, "description");
        EditorGUI.PropertyField(position, property, new GUIContent(smartLabel), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}
