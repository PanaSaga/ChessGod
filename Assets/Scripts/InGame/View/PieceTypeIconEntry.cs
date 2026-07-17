using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct PieceTypeIconEntry
{
    public ChessPieceType pieceType;
    public Sprite icon;
}

// Shared lookup used by any UI that maps a ChessPieceType to its icon sprite.
public static class PieceTypeIconLookup
{
    public static Sprite Find(List<PieceTypeIconEntry> icons, ChessPieceType pieceType)
    {
        foreach (PieceTypeIconEntry entry in icons)
            if (entry.pieceType == pieceType) return entry.icon;
        return null;
    }
}
