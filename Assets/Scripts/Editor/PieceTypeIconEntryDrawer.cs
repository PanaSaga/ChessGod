using UnityEditor;
using UnityEngine;

// Shared by StatusBarSpriteLibrary's Black/White Type Icons and InGameUIManager's Transform Icons -
// all three use this same struct. Shows the piece type (King, Pawn, ...) as the list entry title.
[CustomPropertyDrawer(typeof(PieceTypeIconEntry))]
public class PieceTypeIconEntryDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        string smartLabel = InspectorListLabelUtility.GetSmartLabel(property, label, "pieceType");
        EditorGUI.PropertyField(position, property, new GUIContent(smartLabel), true);
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
        EditorGUI.GetPropertyHeight(property, label, true);
}
