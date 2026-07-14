using UnityEngine;

[CreateAssetMenu(fileName = "NewWhitePiece", menuName = "ChessGame/WhitePieceData")]
public class WhitePieceSO : ChessPieceSO
{
    [Header("백의 말 전용 데이터")]
    public float duration = 20f;

    [Header("스폰 설정")]
    [Range(0f, 100f)]
    public float spawnProbability; // SpawnManager가 참조할 스폰 확률 (0% ~ 100%)
}