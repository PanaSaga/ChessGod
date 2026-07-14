// WhiteBuffPiece.cs
using UnityEngine;

public class WhiteBuffPiece : ChessPiece
{
    [Header("버프 효과 설정")]
    public int attackBuffPower = 2; // 버프 시 올라갈 공격력 수치

    [HideInInspector] public float buffDuration = 20f;

    private void Start()
    {
        // 부모의 pieceData 문서를 '백의 말 양식(WhitePieceSO)'으로 형변환하여 지속 시간을 읽어옵니다.
        WhitePieceSO whiteData = pieceData as WhitePieceSO;
        if (whiteData != null)
        {
            buffDuration = whiteData.duration;
        }
    }

    public void TriggerBuff()
    {
        Debug.Log($"백의 버프 말({PieceType}) 획득! {buffDuration}초간 공격력이 {attackBuffPower}(으)로 증가합니다.");
        // 추후 GameManager와 연동하여 타이머를 켜줄 예정입니다.
        Destroy(gameObject);
    }
}