using UnityEngine;

// Shared grid-to-world conversion and screen-depth sorting for all chess pieces.
public static class ChessBoardUtility
{
    private const int BoardSize = 8;
    private const float TileCenterOffset = 3.5f;
    private const float PieceHeight = 0.101f;
    private const float PieceForwardOffset = 0.08f;

    public static Vector3 GridToWorld(Vector2Int gridPosition) =>
        new(gridPosition.x - TileCenterOffset, PieceHeight, gridPosition.y - TileCenterOffset + PieceForwardOffset);

    // Pieces further down (lower grid z) and further right (higher grid x) on screen draw on top.
    // The player gets a tie-break edge over a black piece that lands on the same tile.
    public static int GetSortingOrder(Vector2Int gridPosition, bool isPlayer)
    {
        int depthOrder = (BoardSize - 1 - gridPosition.y) * BoardSize + gridPosition.x;
        return depthOrder * 2 + (isPlayer ? 1 : 0);
    }

    public static void ApplySortingOrder(SpriteRenderer renderer, Vector2Int gridPosition, bool isPlayer)
    {
        if (renderer != null) renderer.sortingOrder = GetSortingOrder(gridPosition, isPlayer);
    }
}
