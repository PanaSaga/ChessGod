// BlackEnemyPiece.cs
using UnityEngine;

public class BlackEnemyPiece : ChessPiece
{
    [Header("흑의 말 실시간 데이터")]
    public int currentHp;
    public int scoreValue;

    private void Start()
    {
        // 부모의 pieceData 문서를 '흑의 말 양식(BlackPieceSO)'으로 형변환하여 데이터를 안전하게 읽어옵니다.
        BlackPieceSO blackData = pieceData as BlackPieceSO;

        if (blackData != null)
        {
            currentHp = blackData.defaultHp;
            scoreValue = blackData.scoreValue;
        }
        else
        {
            Debug.LogError($"{gameObject.name} 프리팹의 Piece Data 슬롯에 'BlackPieceSO' 양식이 아닌 다른 것이 들어있습니다!");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"{PieceType}이(가) {damage}의 피해를 입음. 남은 HP: {currentHp}");

        if (currentHp <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{PieceType} 처치 완료! {scoreValue}점 획득");
        Destroy(gameObject);
    }
}