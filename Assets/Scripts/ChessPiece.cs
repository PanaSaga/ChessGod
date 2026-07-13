using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    [Header("Runtime state")]
    public Vector2Int gridPos;
    public ChessPieceSO pieceData;

    // White pieces use this to expire after the following turn if not collected.
    [HideInInspector] public int spawnedTurn;

    public ChessPieceType PieceType => pieceData != null ? pieceData.pieceType : ChessPieceType.Pawn;
}
