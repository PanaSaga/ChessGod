using UnityEditor;
using UnityEngine;

// Shows each Tutorial Manager step's own Editor Label (or its dialogue text as a fallback)
// as the list entry's foldout title, instead of Unity's default "Element 0", "Element 1"...
[CustomPropertyDrawer(typeof(TutorialManager.TutorialStep))]
public class TutorialStepDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string smartLabel = InspectorListLabelUtility.GetSmartLabel(property, label, "editorLabel", "speakerName", "dialogueText");
        EditorGUI.PropertyField(position, property, new GUIContent(smartLabel), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}

// Shows "<piece data name> (x,y)" for each forced-placement entry.
[CustomPropertyDrawer(typeof(TutorialManager.ForcedPiece))]
public class ForcedPieceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        SerializedProperty pieceDataProp = property.FindPropertyRelative("pieceData");
        SerializedProperty positionProp = property.FindPropertyRelative("position");

        string pieceName = pieceDataProp != null && pieceDataProp.objectReferenceValue != null
            ? pieceDataProp.objectReferenceValue.name
            : label.text;
        string posText = positionProp != null
            ? $" ({positionProp.vector2IntValue.x},{positionProp.vector2IntValue.y})"
            : "";

        EditorGUI.PropertyField(position, property, new GUIContent(pieceName + posText), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}
