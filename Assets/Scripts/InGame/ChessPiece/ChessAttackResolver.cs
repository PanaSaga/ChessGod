using System.Collections.Generic;
using UnityEngine;

// Shared by combat logic and board highlighting. Sliding attacks intentionally pass through pieces.
public static class ChessAttackResolver
{
    private const int BoardSize = 8;

    public static HashSet<Vector2Int> GetAttackCells(ChessPieceSO data, Vector2Int origin)
    {
        var cells = new HashSet<Vector2Int>();
        if (data == null) return cells;

        AttackPatternType pattern = ResolvePattern(data);
        int range = Mathf.Clamp(data.attackRange, 1, BoardSize - 1);

        void Add(int x, int z)
        {
            Vector2Int cell = origin + new Vector2Int(x, z);
            if (cell.x >= 0 && cell.x < BoardSize && cell.y >= 0 && cell.y < BoardSize)
                cells.Add(cell);
        }

        void Slide(int x, int z)
        {
            for (int distance = 1; distance <= range; distance++)
                Add(x * distance, z * distance);
        }

        switch (pattern)
        {
            case AttackPatternType.King:
                for (int z = -1; z <= 1; z++)
                    for (int x = -1; x <= 1; x++)
                        if (x != 0 || z != 0) Add(x, z);
                break;

            case AttackPatternType.Pawn:
                int forward = data.pawnForwardZ == 0 ? -1 : data.pawnForwardZ;
                Add(-1, forward);
                Add(1, forward);
                break;

            case AttackPatternType.Knight:
                foreach (Vector2Int move in KnightMoves) Add(move.x, move.y);
                break;

            case AttackPatternType.Bishop:
                Slide(1, 1); Slide(1, -1); Slide(-1, 1); Slide(-1, -1);
                break;

            case AttackPatternType.Rook:
                Slide(1, 0); Slide(-1, 0); Slide(0, 1); Slide(0, -1);
                break;

            case AttackPatternType.Queen:
                Slide(1, 0); Slide(-1, 0); Slide(0, 1); Slide(0, -1);
                Slide(1, 1); Slide(1, -1); Slide(-1, 1); Slide(-1, -1);
                break;
        }
        return cells;
    }

    // Chebyshev distance from the origin to the farthest attack cell - used to time the ring-by-ring reveal animation.
    public static int GetMaxRing(ChessPieceSO data, Vector2Int origin)
    {
        int maxRing = 0;
        foreach (Vector2Int cell in GetAttackCells(data, origin))
            maxRing = Mathf.Max(maxRing, Mathf.Max(Mathf.Abs(cell.x - origin.x), Mathf.Abs(cell.y - origin.y)));
        return maxRing;
    }

    private static AttackPatternType ResolvePattern(ChessPieceSO data)
    {
        if (data.attackPattern != AttackPatternType.UsePieceType)
            return data.attackPattern;

        return data.pieceType switch
        {
            ChessPieceType.King => AttackPatternType.King,
            ChessPieceType.Pawn => AttackPatternType.Pawn,
            ChessPieceType.Knight => AttackPatternType.Knight,
            ChessPieceType.Bishop => AttackPatternType.Bishop,
            ChessPieceType.Rook => AttackPatternType.Rook,
            ChessPieceType.Queen => AttackPatternType.Queen,
            _ => AttackPatternType.King
        };
    }

    private static readonly Vector2Int[] KnightMoves =
    {
        new(1, 2), new(2, 1), new(2, -1), new(1, -2),
        new(-1, -2), new(-2, -1), new(-2, 1), new(-1, 2)
    };
}
