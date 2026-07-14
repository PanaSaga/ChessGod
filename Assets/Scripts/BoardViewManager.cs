using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Shows gameplay attack data on the board; it does not change combat outcomes.
public class BoardViewManager : MonoBehaviour
{
    [Header("Attack range materials")]
    [SerializeField] private Material playerRangeMaterial;
    [SerializeField] private Material enemyRangeMaterial;
    [SerializeField] private Material overlapRangeMaterial;
    [SerializeField, Min(0.1f)] private float enemyPreviewInterval = 1f;

    private BoardManager boardManager;
    private SpawnManager spawnManager;
    private PlayerPiece playerPiece;
    private float nextEnemyPreviewTime;
    private int enemyPreviewIndex;

    private void Start()
    {
        boardManager = FindFirstObjectByType<BoardManager>();
        spawnManager = FindFirstObjectByType<SpawnManager>();
        playerPiece = FindFirstObjectByType<PlayerPiece>();
    }

    private void LateUpdate()
    {
        if (boardManager == null || spawnManager == null || playerPiece == null) return;

        foreach (BoardTileVisual tile in boardManager.GetAllTiles()) tile.ClearOverlay();

        HashSet<Vector2Int> playerCells = ChessAttackResolver.GetAttackCells(playerPiece.CurrentAttackData, playerPiece.gridPos);
        List<BlackEnemyPiece> enemies = spawnManager.activePieces.OfType<BlackEnemyPiece>().ToList();
        HashSet<Vector2Int> enemyCells = GetVisibleEnemyCells(enemies);

        foreach (Vector2Int cell in playerCells)
            Paint(cell, enemyCells.Contains(cell) ? overlapRangeMaterial : playerRangeMaterial);

        foreach (Vector2Int cell in enemyCells)
            if (!playerCells.Contains(cell)) Paint(cell, enemyRangeMaterial);
    }

    private HashSet<Vector2Int> GetVisibleEnemyCells(List<BlackEnemyPiece> enemies)
    {
        var result = new HashSet<Vector2Int>();
        if (enemies.Count == 0) return result;

        if (GameManager.Instance != null && GameManager.Instance.isSettling)
        {
            foreach (BlackEnemyPiece enemy in enemies)
                result.UnionWith(ChessAttackResolver.GetAttackCells(enemy.pieceData, enemy.gridPos));
            return result;
        }

        if (Time.time >= nextEnemyPreviewTime)
        {
            enemyPreviewIndex = (enemyPreviewIndex + 1) % enemies.Count;
            nextEnemyPreviewTime = Time.time + enemyPreviewInterval;
        }

        BlackEnemyPiece previewEnemy = enemies[enemyPreviewIndex % enemies.Count];
        result.UnionWith(ChessAttackResolver.GetAttackCells(previewEnemy.pieceData, previewEnemy.gridPos));
        return result;
    }

    private void Paint(Vector2Int position, Material material)
    {
        if (boardManager.TryGetTile(position, out BoardTileVisual tile)) tile.SetOverlay(material);
    }
}
