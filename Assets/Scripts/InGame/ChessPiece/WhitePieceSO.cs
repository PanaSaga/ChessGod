using UnityEngine;

[CreateAssetMenu(fileName = "NewWhitePiece", menuName = "ChessGame/WhitePieceData")]
public class WhitePieceSO : ChessPieceSO
{
    [Header("���� �� ���� ������")]
    public float duration = 20f;

    [Header("Player transform visual")]
    public Sprite transformSprite;
}