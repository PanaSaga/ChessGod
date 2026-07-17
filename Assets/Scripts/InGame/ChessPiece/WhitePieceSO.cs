using UnityEngine;

[CreateAssetMenu(fileName = "NewWhitePiece", menuName = "ChessGame/WhitePieceData")]
public class WhitePieceSO : ChessPieceSO
{
    [Header("���� �� ���� ������")]
    public float duration = 20f;

    [Header("Player transform visual")]
    public Sprite transformSprite;

    [Header("���� ����")]
    [Range(0f, 100f)]
    public float spawnProbability; // SpawnManager�� ������ ���� Ȯ�� (0% ~ 100%)
}