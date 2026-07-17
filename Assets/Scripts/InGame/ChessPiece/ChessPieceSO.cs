using UnityEngine;

public abstract class ChessPieceSO : ScriptableObject
{
    [Header("Piece")]
    public ChessPieceType pieceType;
    public GameObject prefab;

    [Header("Attack range")]
    [Tooltip("Use Piece Type uses the Piece Type above. Override it only for a special attack pattern.")]
    public AttackPatternType attackPattern = AttackPatternType.UsePieceType;
    [Range(1, 7)] public int attackRange = 7;
    [Tooltip("Only used by the Pawn pattern. Black pawns normally attack toward -Z.")]
    [Range(-1, 1)] public int pawnForwardZ = -1;

    [Header("Spawn")]
    [Range(0f, 100f)]
    public float spawnProbability; // SpawnManager가 사용하는 스폰 확률 (0% ~ 100%)
}
