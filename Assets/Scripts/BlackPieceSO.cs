using UnityEngine;

[CreateAssetMenu(fileName = "NewBlackPiece", menuName = "ChessGame/BlackPieceData")]
public class BlackPieceSO : ChessPieceSO
{
    [Header("흑의 말 전용 데이터")]
    public int defaultHp = 1;
    public int scoreValue = 10;

    [Header("스폰 설정")]
    [Range(0f, 100f)]
    public float spawnProbability; // SpawnManager가 참조할 스폰 확률 (0% ~ 100%)
}