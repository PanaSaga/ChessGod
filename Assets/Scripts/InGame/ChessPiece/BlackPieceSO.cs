using UnityEngine;

[CreateAssetMenu(fileName = "NewBlackPiece", menuName = "ChessGame/BlackPieceData")]
public class BlackPieceSO : ChessPieceSO
{
    [Header("���� �� ���� ������")]
    public int defaultHp = 1;
    public int scoreValue = 10;
}