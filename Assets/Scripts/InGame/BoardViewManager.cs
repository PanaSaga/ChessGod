using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Shows gameplay attack data on the board; it does not change combat outcomes.
public class BoardViewManager : MonoBehaviour
{
    private struct RevealSession
    {
        public Vector2Int origin;
        public ChessPieceSO attackData;
        public bool isPlayer;
        public float startTime;
        public float ringInterval;
    }

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

    private readonly List<RevealSession> activeReveals = new();

    private void Start()
    {
        boardManager = FindFirstObjectByType<BoardManager>();
        spawnManager = FindFirstObjectByType<SpawnManager>();
        playerPiece = FindFirstObjectByType<PlayerPiece>();
    }

    // Called by GameManager the instant a piece lands from its attack jump. That piece's own
    // range then spreads outward ring by ring, independently of every other piece's reveal.
    public void RegisterReveal(Vector2Int origin, ChessPieceSO attackData, bool isPlayer, float ringInterval)
    {
        activeReveals.Add(new RevealSession
        {
            origin = origin,
            attackData = attackData,
            isPlayer = isPlayer,
            startTime = Time.time,
            ringInterval = Mathf.Max(0.01f, ringInterval)
        });
    }

    public void ClearReveals() => activeReveals.Clear();

    private void LateUpdate()
    {
        if (boardManager == null || spawnManager == null || playerPiece == null) return;

        foreach (BoardTileVisual tile in boardManager.GetAllTiles()) tile.ClearOverlay();

        if (GameManager.Instance != null && GameManager.Instance.isSettling)
        {
            PaintActiveReveals();
            return;
        }

        HashSet<Vector2Int> playerCells = ChessAttackResolver.GetAttackCells(playerPiece.CurrentAttackData, playerPiece.gridPos);
        List<BlackEnemyPiece> enemies = spawnManager.activePieces.OfType<BlackEnemyPiece>().ToList();
        HashSet<Vector2Int> enemyCells = GetVisibleEnemyCells(enemies);

        foreach (Vector2Int cell in playerCells)
            Paint(cell, enemyCells.Contains(cell) ? overlapRangeMaterial : playerRangeMaterial);

        foreach (Vector2Int cell in enemyCells)
            if (!playerCells.Contains(cell)) Paint(cell, enemyRangeMaterial);
    }

    // Each registered piece reveals its own attack cells outward from its own tile, at its own pace,
    // independently of when any other piece landed or how far its own range reaches.
    private void PaintActiveReveals()
    {
        Dictionary<Vector2Int, bool> playerCovered = new();
        Dictionary<Vector2Int, bool> enemyCovered = new();

        foreach (RevealSession session in activeReveals)
        {
            float revealedRing = (Time.time - session.startTime) / session.ringInterval;
            foreach (KeyValuePair<Vector2Int, int> cellRing in GetRings(session.attackData, session.origin))
            {
                if (cellRing.Value > revealedRing) continue;
                if (session.isPlayer) playerCovered[cellRing.Key] = true;
                else enemyCovered[cellRing.Key] = true;
            }
        }

        foreach (Vector2Int cell in playerCovered.Keys)
            Paint(cell, enemyCovered.ContainsKey(cell) ? overlapRangeMaterial : playerRangeMaterial);

        foreach (Vector2Int cell in enemyCovered.Keys)
            if (!playerCovered.ContainsKey(cell)) Paint(cell, enemyRangeMaterial);
    }

    // Chebyshev distance from the attacker's own tile: how many "rings" out a cell sits.
    private static Dictionary<Vector2Int, int> GetRings(ChessPieceSO data, Vector2Int origin)
    {
        Dictionary<Vector2Int, int> rings = new();
        foreach (Vector2Int cell in ChessAttackResolver.GetAttackCells(data, origin))
            rings[cell] = Mathf.Max(Mathf.Abs(cell.x - origin.x), Mathf.Abs(cell.y - origin.y));
        return rings;
    }

    private HashSet<Vector2Int> GetVisibleEnemyCells(List<BlackEnemyPiece> enemies)
    {
        var result = new HashSet<Vector2Int>();
        if (enemies.Count == 0) return result;

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
