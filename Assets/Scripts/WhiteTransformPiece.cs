// WhiteTransformPiece.cs
using UnityEngine;

public class WhiteTransformPiece : ChessPiece
{
    [Header("변신 효과 설정")]
    // 현재 이 말의 종류(나이트, 비숍 등)를 그대로 '현재 변신 종류' 데이터로 사용합니다.
    public ChessPieceType CurrentTransformType => PieceType;

    [HideInInspector] public float transformDuration = 20f;

    private void Start()
    {
        // 부모의 pieceData 문서를 '백의 말 양식(WhitePieceSO)'으로 형변환하여 지속 시간을 읽어옵니다.
        WhitePieceSO whiteData = pieceData as WhitePieceSO;
        if (whiteData != null)
        {
            transformDuration = whiteData.duration;
        }
    }

    public void TriggerTransform()
    {
        Debug.Log($"백의 변신 말({PieceType}) 획득! {transformDuration}초간 공격 범위가 [{CurrentTransformType}] 모드로 전환됩니다.");
        // 추후 GameManager와 연동하여 타이머를 켜줄 예정입니다.
        Destroy(gameObject);
    }
}