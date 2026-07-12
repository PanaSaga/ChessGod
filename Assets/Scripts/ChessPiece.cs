// ChessPiece.cs
using UnityEngine;

public class ChessPiece : MonoBehaviour
{
    [Header("공통 좌표 및 데이터")]
    public Vector2Int gridPos;       // 체스판 위의 좌표 값
    public ChessPieceSO pieceData;   // 스크립트 테이블 참조 문서

    // 기물 종류를 쉽게 가져오기 위한 프로퍼티
    public ChessPieceType PieceType => pieceData != null ? pieceData.pieceType : ChessPieceType.Pawn;
}