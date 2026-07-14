using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private const int BoardSize = 8;
    private const int MaxBlackPieces = 16;

    [Header("ScriptableObject data")]
    [SerializeField] private BlackPieceSO[] blackPieceData;
    [SerializeField] private WhitePieceSO[] whitePieceData;

    public readonly List<ChessPiece> activePieces = new();

    public int BlackPieceCount => activePieces.Count(piece => piece is BlackEnemyPiece);
    public int QueenCount => activePieces.Count(piece => piece is BlackEnemyPiece && piece.PieceType == ChessPieceType.Queen);

    public void SpawnInitialBlackPieces(Vector2Int playerPosition, int turn)
    {
        SpawnBlackOfType(ChessPieceType.Queen, playerPosition, turn);
        SpawnBlackOfType(ChessPieceType.Knight, playerPosition, turn);
        SpawnBlackOfType(ChessPieceType.Pawn, playerPosition, turn);
    }

    public int SpawnBlackPieces(int requestedCount, Vector2Int playerPosition, int turn, bool forceQueen)
    {
        int spawned = 0;
        if (forceQueen && QueenCount < 3 && SpawnBlackOfType(ChessPieceType.Queen, playerPosition, turn) != null)
            spawned++;

        while (spawned < requestedCount && BlackPieceCount < MaxBlackPieces)
        {
            BlackPieceSO data = SelectWeightedBlackData();
            if (data == null || SpawnPiece(data, GetRandomFreePosition(playerPosition), turn) == null) break;
            spawned++;
        }
        return spawned;
    }

    public bool TrySpawnBuffPiece(Vector2Int playerPosition, int turn, float chancePercent)
    {
        if (HasWhitePiece<WhiteBuffPiece>() || Random.Range(0f, 100f) >= chancePercent) return false;
        WhitePieceSO pawnData = whitePieceData.FirstOrDefault(data => data != null && data.pieceType == ChessPieceType.Pawn);
        return pawnData != null && SpawnPiece(pawnData, GetRandomFreePosition(playerPosition), turn) != null;
    }

    public bool TrySpawnTransformPiece(Vector2Int playerPosition, int turn, bool forceKnight)
    {
        if (HasWhitePiece<WhiteTransformPiece>()) return false;

        List<WhitePieceSO> options = whitePieceData
            .Where(data => data != null && data.pieceType != ChessPieceType.Pawn)
            .ToList();
        if (forceKnight)
            options = options.Where(data => data.pieceType == ChessPieceType.Knight).ToList();

        WhitePieceSO selected = SelectWeightedWhiteData(options);
        return selected != null && SpawnPiece(selected, GetRandomFreePosition(playerPosition), turn) != null;
    }

    public void RepositionBlackPieces(Vector2Int playerPosition)
    {
        foreach (BlackEnemyPiece piece in activePieces.OfType<BlackEnemyPiece>().ToArray())
        {
            Vector2Int position = GetRandomFreePosition(playerPosition, piece);
            if (position.x < 0) return;
            SetPiecePosition(piece, position);
        }
    }

    public void RemoveExpiredWhitePieces(int currentTurn)
    {
        foreach (ChessPiece piece in activePieces.Where(piece => piece is WhiteBuffPiece || piece is WhiteTransformPiece).ToArray())
        {
            if (piece.spawnedTurn < currentTurn)
                RemovePiece(piece);
        }
    }

    public ChessPiece SpawnPiece(ChessPieceSO data, Vector2Int gridPosition, int turn)
    {
        if (data == null || data.prefab == null || gridPosition.x < 0)
        {
            Debug.LogError("Spawn requires a valid SO, prefab, and free board position.");
            return null;
        }

        // Keep the prefab's authored orientation (for example, a 2D chess sprite rotated 90¡Æ onto the board).
        GameObject instance = Instantiate(data.prefab, GridToWorld(gridPosition), data.prefab.transform.rotation);
        ChessPiece piece = instance.GetComponent<ChessPiece>();
        if (piece == null)
        {
            Debug.LogError($"'{data.prefab.name}' requires a ChessPiece component.");
            Destroy(instance);
            return null;
        }

        piece.pieceData = data;
        piece.gridPos = gridPosition;
        piece.spawnedTurn = turn;
        if (piece is BlackEnemyPiece enemy && data is BlackPieceSO blackData)
            enemy.Initialize(blackData);

        activePieces.Add(piece);
        return piece;
    }

    public ChessPiece GetPieceAt(Vector2Int position)
    {
        activePieces.RemoveAll(piece => piece == null);
        return activePieces.Find(piece => piece.gridPos == position);
    }

    public bool IsOccupied(Vector2Int position, ChessPiece ignoredPiece = null)
    {
        return activePieces.Any(piece => piece != null && piece != ignoredPiece && piece.gridPos == position);
    }

    public void RemovePiece(ChessPiece piece)
    {
        if (piece == null) return;
        activePieces.Remove(piece);
        Destroy(piece.gameObject);
    }

    private BlackEnemyPiece SpawnBlackOfType(ChessPieceType type, Vector2Int playerPosition, int turn)
    {
        if (BlackPieceCount >= MaxBlackPieces || (type == ChessPieceType.Queen && QueenCount >= 3)) return null;
        BlackPieceSO data = blackPieceData.FirstOrDefault(item => item != null && item.pieceType == type);
        return SpawnPiece(data, GetRandomFreePosition(playerPosition), turn) as BlackEnemyPiece;
    }

    private BlackPieceSO SelectWeightedBlackData()
    {
        return SelectWeighted(blackPieceData, data => data.spawnProbability);
    }

    private WhitePieceSO SelectWeightedWhiteData(List<WhitePieceSO> candidates)
    {
        return SelectWeighted(candidates, data => data.spawnProbability);
    }

    private T SelectWeighted<T>(IEnumerable<T> candidates, System.Func<T, float> getWeight) where T : ChessPieceSO
    {
        List<T> valid = candidates.Where(data => data != null && getWeight(data) > 0f).ToList();
        float total = valid.Sum(getWeight);
        if (total <= 0f) return null;

        float roll = Random.Range(0f, total);
        foreach (T data in valid)
        {
            roll -= getWeight(data);
            if (roll <= 0f) return data;
        }
        return valid[valid.Count - 1];
    }

    private bool HasWhitePiece<T>() where T : ChessPiece => activePieces.Any(piece => piece is T);

    private Vector2Int GetRandomFreePosition(Vector2Int playerPosition, ChessPiece ignoredPiece = null)
    {
        List<Vector2Int> free = new();
        for (int z = 0; z < BoardSize; z++)
            for (int x = 0; x < BoardSize; x++)
            {
                Vector2Int position = new(x, z);
                if (position != playerPosition && !IsOccupied(position, ignoredPiece)) free.Add(position);
            }
        return free.Count == 0 ? new Vector2Int(-1, -1) : free[Random.Range(0, free.Count)];
    }

    private void SetPiecePosition(ChessPiece piece, Vector2Int position)
    {
        piece.gridPos = position;
        piece.transform.position = GridToWorld(position);
    }

    private static Vector3 GridToWorld(Vector2Int position) => new(position.x - 3.5f, 0.101f, position.y - 3.5f);
}
