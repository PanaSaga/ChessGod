using UnityEditor;
using UnityEngine;

// Shows each condition group's Event Type as the list entry's foldout title, instead of Unity's
// default "Element 0", "Element 1"...
[CustomPropertyDrawer(typeof(AchievementSO.ConditionGroup))]
public class AchievementConditionGroupDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string smartLabel = InspectorListLabelUtility.GetSmartLabel(property, label, "eventType");
        EditorGUI.PropertyField(position, property, new GUIContent(smartLabel), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}
