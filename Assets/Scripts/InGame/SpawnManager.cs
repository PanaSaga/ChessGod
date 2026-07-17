using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    private const int BoardSize = 8;
    private const int MaxBlackPieces = 16;
    private const int MinSafeZoneTiles = 8;

    [Header("ScriptableObject data")]
    [SerializeField] private BlackPieceSO[] blackPieceData;
    [SerializeField] private WhitePieceSO[] whitePieceData;

    [Header("Black piece spawn drop")]
    [SerializeField, Min(0f)] private float dropHeightY = 2.5f;
    [SerializeField] private float dropOffsetZ = 2f;
    [SerializeField, Min(0.01f)] private float dropDuration = 0.35f;
    [SerializeField, Min(0.01f)] private float landingSquashDuration = 0.18f;
    [SerializeField, Range(0f, 1f)] private float landingSquashSideInfluence = 0.4f;
    [SerializeField] private AnimationCurve dropFallCurve = CreateDefaultDropFallCurve();
    [SerializeField] private AnimationCurve landingSquashCurve = CreateDefaultLandingSquashCurve();

    public readonly List<ChessPiece> activePieces = new();

    public int BlackPieceCount => activePieces.Count(piece => piece is BlackEnemyPiece);
    public int QueenCount => activePieces.Count(piece => piece is BlackEnemyPiece && piece.PieceType == ChessPieceType.Queen);

    public int GetAliveBlackCount(ChessPieceType pieceType) =>
        activePieces.Count(piece => piece is BlackEnemyPiece && piece.PieceType == pieceType);

    public void SpawnInitialBlackPieces(Vector2Int playerPosition, int turn)
    {
        SpawnBlackOfType(ChessPieceType.Queen, playerPosition, turn);
        SpawnBlackOfType(ChessPieceType.Knight, playerPosition, turn);
        SpawnBlackOfType(ChessPieceType.Pawn, playerPosition, turn);
    }

    public int SpawnBlackPieces(int requestedCount, Vector2Int playerPosition, int turn, bool forceQueen)
    {
        int spawned = 0;
        if (forceQueen && QueenCount < 3 && HasSafeZoneRoomToSpawn(playerPosition)
            && SpawnBlackOfType(ChessPieceType.Queen, playerPosition, turn) != null)
            spawned++;

        while (spawned < requestedCount && BlackPieceCount < MaxBlackPieces)
        {
            // Cancel the next spawn attempt if it would drop the board's free-tile safe zone below the minimum.
            if (!HasSafeZoneRoomToSpawn(playerPosition)) break;

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
        // A white piece spawned on turn T stays through turn T+1 and disappears starting turn T+2.
        foreach (ChessPiece piece in activePieces.Where(piece => piece is WhiteBuffPiece || piece is WhiteTransformPiece).ToArray())
        {
            if (piece.spawnedTurn < currentTurn - 1)
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

        Vector3 finalPosition = ChessBoardUtility.GridToWorld(gridPosition);
        bool isBlackPiece = data is BlackPieceSO;
        Vector3 spawnPosition = isBlackPiece
            ? finalPosition + new Vector3(0f, dropHeightY, dropOffsetZ)
            : finalPosition;

        // Keep the prefab's authored orientation (for example, a 2D chess sprite rotated 90 degrees onto the board).
        GameObject instance = Instantiate(data.prefab, spawnPosition, data.prefab.transform.rotation);
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

        ChessBoardUtility.ApplySortingOrder(instance.GetComponentInChildren<SpriteRenderer>(), gridPosition, isPlayer: false);

        activePieces.Add(piece);

        if (isBlackPiece)
            StartCoroutine(AnimateSpawnDrop(instance.transform, finalPosition));

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

    private int CountFreeTiles(Vector2Int playerPosition)
    {
        int count = 0;
        for (int z = 0; z < BoardSize; z++)
            for (int x = 0; x < BoardSize; x++)
            {
                Vector2Int position = new(x, z);
                if (position != playerPosition && !IsOccupied(position)) count++;
            }
        return count;
    }

    // One free tile is consumed by the piece being spawned, so at least MinSafeZoneTiles + 1 must be free beforehand.
    private bool HasSafeZoneRoomToSpawn(Vector2Int playerPosition) => CountFreeTiles(playerPosition) - 1 >= MinSafeZoneTiles;

    public Vector2Int GetRandomFreePosition(Vector2Int playerPosition, ChessPiece ignoredPiece = null)
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
        piece.transform.position = ChessBoardUtility.GridToWorld(position);
        ChessBoardUtility.ApplySortingOrder(piece.GetComponentInChildren<SpriteRenderer>(), position, isPlayer: false);
    }

    // Falls from above the board down to its tile, slow at first and speeding up, then hands off to the landing squash.
    private IEnumerator AnimateSpawnDrop(Transform pieceTransform, Vector3 finalPosition)
    {
        Vector3 startPosition = finalPosition + new Vector3(0f, dropHeightY, dropOffsetZ);
        Vector3 baseScale = pieceTransform.localScale;

        float elapsed = 0f;
        while (elapsed < dropDuration)
        {
            if (pieceTransform == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / dropDuration);
            pieceTransform.position = Vector3.Lerp(startPosition, finalPosition, dropFallCurve.Evaluate(t));
            yield return null;
        }

        if (pieceTransform == null) yield break;
        pieceTransform.position = finalPosition;

        yield return StartCoroutine(AnimateLandingSquash(pieceTransform, finalPosition, baseScale));
    }

    // Squashes the sprite down along its own local Y on impact, then springs back to normal - the "inertia" feel.
    private IEnumerator AnimateLandingSquash(Transform pieceTransform, Vector3 groundPosition, Vector3 baseScale)
    {
        float spriteHalfHeight = PieceSquashUtility.GetSpriteHalfHeight(pieceTransform);

        float elapsed = 0f;
        while (elapsed < landingSquashDuration)
        {
            if (pieceTransform == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / landingSquashDuration);

            float squashY = landingSquashCurve.Evaluate(t);
            float squashXZ = PieceSquashUtility.GetSquashSideScale(squashY, landingSquashSideInfluence);
            Vector3 anchorOffset = PieceSquashUtility.GetGroundAnchorOffset(pieceTransform, spriteHalfHeight, baseScale.y, squashY);

            pieceTransform.position = groundPosition + anchorOffset;
            pieceTransform.localScale = new Vector3(baseScale.x * squashXZ, baseScale.y * squashY, baseScale.z);
            yield return null;
        }

        if (pieceTransform != null)
        {
            pieceTransform.position = groundPosition;
            pieceTransform.localScale = baseScale;
        }
    }

    // Barely moves at first, then accelerates hard into the landing - like gravity pulling it down.
    private static AnimationCurve CreateDefaultDropFallCurve()
    {
        AnimationCurve curve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.4f, 0.15f),
            new Keyframe(0.75f, 0.55f),
            new Keyframe(1f, 1f));
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        return curve;
    }

    // 1 = normal. Flattens hard right on impact, springs slightly tall, then settles back to normal.
    private static AnimationCurve CreateDefaultLandingSquashCurve()
    {
        AnimationCurve curve = new(
            new Keyframe(0f, 1f),
            new Keyframe(0.15f, 0.55f),
            new Keyframe(0.45f, 1.15f),
            new Keyframe(0.75f, 0.95f),
            new Keyframe(1f, 1f));
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        return curve;
    }
}
