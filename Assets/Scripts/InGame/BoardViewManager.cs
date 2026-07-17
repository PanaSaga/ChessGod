using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Shows gameplay attack data on the board; it does not change combat outcomes.
public class BoardViewManager : MonoBehaviour
{
    // One ring (one "step" of a piece's range reveal) with its own independent appear/expire timing.
    private struct RingReveal
    {
        public HashSet<Vector2Int> cells;
        public bool isPlayer;
        public float appearTime;
        public float expireTime;
    }

    [Header("Attack range overlay sprites")]
    [Tooltip("Board_ChessW_ATK: player-only range.")]
    [SerializeField] private Sprite playerRangeSprite;
    [Tooltip("Board_ChessB_ATK: enemy-only range.")]
    [SerializeField] private Sprite enemyRangeSprite;
    [Tooltip("Board_ChessWB_ATK: player and enemy ranges overlapping.")]
    [SerializeField] private Sprite overlapRangeSprite;
    [SerializeField, Min(0.1f)] private float enemyPreviewInterval = 1f;

    private BoardManager boardManager;
    private SpawnManager spawnManager;
    private PlayerPiece playerPiece;
    private float nextEnemyPreviewTime;
    private int enemyPreviewIndex;

    private readonly List<RingReveal> activeRings = new();

    private void Start()
    {
        boardManager = FindFirstObjectByType<BoardManager>();
        spawnManager = FindFirstObjectByType<SpawnManager>();
        playerPiece = FindFirstObjectByType<PlayerPiece>();
    }

    // Called by GameManager the instant a piece lands from its attack jump. Splits that piece's
    // range into per-ring entries: ring 0 appears immediately, ring 1 appears ringInterval later,
    // ring 2 another ringInterval later, and so on. Each ring then lingers for ringLingerDuration
    // seconds counted from when THAT ring appeared, and disappears on its own - independently of
    // every other ring, whether from the same piece or a different one.
    public void RegisterReveal(Vector2Int origin, ChessPieceSO attackData, bool isPlayer, float ringInterval, float ringLingerDuration)
    {
        ringInterval = Mathf.Max(0.01f, ringInterval);
        ringLingerDuration = Mathf.Max(0f, ringLingerDuration);
        float now = Time.time;

        Dictionary<int, HashSet<Vector2Int>> cellsByRing = new();
        foreach (KeyValuePair<Vector2Int, int> cellRing in GetRings(attackData, origin))
        {
            if (!cellsByRing.TryGetValue(cellRing.Value, out HashSet<Vector2Int> cells))
            {
                cells = new HashSet<Vector2Int>();
                cellsByRing[cellRing.Value] = cells;
            }
            cells.Add(cellRing.Key);
        }

        foreach (KeyValuePair<int, HashSet<Vector2Int>> ring in cellsByRing)
        {
            float appearTime = now + ring.Key * ringInterval;
            activeRings.Add(new RingReveal
            {
                cells = ring.Value,
                isPlayer = isPlayer,
                appearTime = appearTime,
                expireTime = appearTime + ringLingerDuration
            });
        }
    }

    public void ClearReveals() => activeRings.Clear();

    private void LateUpdate()
    {
        if (boardManager == null || spawnManager == null || playerPiece == null) return;
        if (GameManager.Instance != null && GameManager.Instance.isPaused) return;

        foreach (BoardTileVisual tile in boardManager.GetAllTiles()) tile.ClearOverlay();

        float now = Time.time;
        activeRings.RemoveAll(ring => now >= ring.expireTime);

        Dictionary<Vector2Int, bool> playerCovered = new();
        Dictionary<Vector2Int, bool> enemyCovered = new();

        // Leftover attack-range rings keep fading out on their own schedule no matter what the
        // game is doing right now - settling or not - so nothing ever gets cut off abruptly.
        foreach (RingReveal ring in activeRings)
        {
            if (now < ring.appearTime) continue;
            foreach (Vector2Int cell in ring.cells)
            {
                if (ring.isPlayer) playerCovered[cell] = true;
                else enemyCovered[cell] = true;
            }
        }

        if (GameManager.Instance == null || !GameManager.Instance.isSettling)
        {
            // Normal gameplay: the player's live range is always shown, plus one enemy's range cycling.
            foreach (Vector2Int cell in ChessAttackResolver.GetAttackCells(playerPiece.CurrentAttackData, playerPiece.gridPos))
                playerCovered[cell] = true;

            List<BlackEnemyPiece> enemies = spawnManager.activePieces.OfType<BlackEnemyPiece>().ToList();
            foreach (Vector2Int cell in GetVisibleEnemyCells(enemies))
                enemyCovered[cell] = true;
        }

        foreach (Vector2Int cell in playerCovered.Keys)
            Paint(cell, enemyCovered.ContainsKey(cell) ? overlapRangeSprite : playerRangeSprite);

        foreach (Vector2Int cell in enemyCovered.Keys)
            if (!playerCovered.ContainsKey(cell)) Paint(cell, enemyRangeSprite);
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

    private void Paint(Vector2Int position, Sprite sprite)
    {
        if (boardManager.TryGetTile(position, out BoardTileVisual tile)) tile.SetOverlay(sprite);
    }
}
