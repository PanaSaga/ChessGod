using System;
using System.Collections.Generic;
using UnityEngine;

// Put one instance in the scene and assign the six UI sprites once.
public class StatusBarSpriteLibrary : MonoBehaviour
{
    [Serializable]
    public struct PieceTypeIcon
    {
        public ChessPieceType pieceType;
        public Sprite icon;
    }

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
    public List<PieceTypeIcon> blackTypeIcons = new();
    [Tooltip("White/player piece icons, e.g. Icon_ChessPW_*")]
    public List<PieceTypeIcon> whiteTypeIcons = new();

    public Sprite GetBlackTypeIcon(ChessPieceType pieceType) => Find(blackTypeIcons, pieceType);
    public Sprite GetWhiteTypeIcon(ChessPieceType pieceType) => Find(whiteTypeIcons, pieceType);

    private static Sprite Find(List<PieceTypeIcon> icons, ChessPieceType pieceType)
    {
        foreach (PieceTypeIcon entry in icons)
            if (entry.pieceType == pieceType) return entry.icon;
        return null;
    }
}
