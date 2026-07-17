using UnityEditor;
using UnityEngine;

// Shared by every "list element" PropertyDrawer in this project: picks the first meaningful
// value among a set of candidate field names (in priority order) to use as that element's
// collapsed list label, instead of Unity's default "Element 0", "Element 1"...
public static class InspectorListLabelUtility
{
    public static string GetSmartLabel(SerializedProperty property, GUIContent defaultLabel, params string[] fieldNamesInPriorityOrder)
    {
        foreach (string fieldName in fieldNamesInPriorityOrder)
        {
            SerializedProperty field = property.FindPropertyRelative(fieldName);
            if (field == null) continue;

            string preview = GetPreviewText(field);
            if (!string.IsNullOrEmpty(preview)) return preview;
        }
        return defaultLabel.text;
    }

    private static string GetPreviewText(SerializedProperty field)
    {
        switch (field.propertyType)
        {
            case SerializedPropertyType.String:
                return Truncate(field.stringValue, 40);
            case SerializedPropertyType.Enum:
                return field.enumValueIndex >= 0 && field.enumValueIndex < field.enumDisplayNames.Length
                    ? field.enumDisplayNames[field.enumValueIndex]
                    : null;
            case SerializedPropertyType.ObjectReference:
                return field.objectReferenceValue != null ? field.objectReferenceValue.name : null;
            case SerializedPropertyType.Vector2Int:
                return $"({field.vector2IntValue.x},{field.vector2IntValue.y})";
            case SerializedPropertyType.Integer:
                return field.intValue.ToString();
            default:
                return null;
        }
    }

    private static string Truncate(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text)) return text;
        text = text.Replace("\n", " ");
        return text.Length <= maxLength ? text : text[..maxLength] + "...";
    }
}
