using System.Collections.Generic;
using UnityEngine;

// Put one instance in the scene and assign the six UI sprites once.
public class StatusBarSpriteLibrary : MonoBehaviour
{
    [Header("HP")]
    [Tooltip("Filled heart: UI_HP_Heart")]
    public Sprite hpHeart;
    [Tooltip("Empty heart: UI_HP_Empty")]
    public Sprite hpEmpty;

    [Header("Buff (Up)")]
    public Sprite upBar;
    public Sprite upLine;

    [Header("Transform (Tr)")]
    public Sprite trBar;
    public Sprite trLine;

    [Header("Piece type icons (shown next to HP hearts)")]
    [Tooltip("Black piece icons, e.g. Icon_ChessPB_*")]
    public List<PieceTypeIconEntry> blackTypeIcons = new();
    [Tooltip("White/player piece icons, e.g. Icon_ChessPW_*")]
    public List<PieceTypeIconEntry> whiteTypeIcons = new();

    public Sprite GetBlackTypeIcon(ChessPieceType pieceType) => PieceTypeIconLookup.Find(blackTypeIcons, pieceType);
    public Sprite GetWhiteTypeIcon(ChessPieceType pieceType) => PieceTypeIconLookup.Find(whiteTypeIcons, pieceType);
}
