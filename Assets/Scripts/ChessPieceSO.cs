// ChessPieceSO.cs
using UnityEngine;

public abstract class ChessPieceSO : ScriptableObject
{
    [Header("공통 기물 데이터")]
    public ChessPieceType pieceType; // 기물 종류 (폰, 나이트 등)
    public GameObject prefab;        // 화면에 생성할 프리팹 에셋
}