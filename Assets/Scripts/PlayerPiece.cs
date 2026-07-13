using UnityEngine;

public class PlayerPiece : ChessPiece
{
    [Header("플레이어 상태 데이터")]
    public int hp = 3;
    public int atk = 1;
    public ChessPieceType currentRangeType = ChessPieceType.King;

    [Header("버프 및 변신 타이머")]
    public float buffTimer = 0f;
    public bool isBuffActive = false;
    public float transformTimer = 0f;
    public bool isTransformActive = false;

    void Update()
    {
        // 버프 타이머 처리 (매 프레임마다 시간 감소)
        if (isBuffActive)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                isBuffActive = false;
                atk = 1; // 기획서에 따라 기본 공격력 1로 복구
                Debug.Log("버프 종료: 공격력이 1로 돌아왔습니다.");
            }
        }

        // 변신 타이머 처리 (매 프레임마다 시간 감소)
        if (isTransformActive)
        {
            transformTimer -= Time.deltaTime;
            if (transformTimer <= 0f)
            {
                isTransformActive = false;
                currentRangeType = ChessPieceType.King; // 기획서에 따라 기본 형태 킹으로 복구
                Debug.Log("변신 종료: 공격 범위가 킹으로 돌아왔습니다.");
            }
        }
    }

    // 버프 획득 시 호출될 함수 (기획서 룰 10 반영: 남은 시간 무관하게 시간 갱신)
    public void ApplyBuff(float duration, int buffAtk)
    {
        isBuffActive = true;
        buffTimer = duration;
        atk = buffAtk;
    }

    // 변신 획득 시 호출될 함수 (기획서 룰 11 반영: 남은 시간 무관하게 신규 형태 및 시간 갱신)
    public void ApplyTransform(float duration, ChessPieceType newType)
    {
        isTransformActive = true;
        transformTimer = duration;
        currentRangeType = newType;
    }

    // 피격 시 호출될 함수
    public void TakeDamage()
    {
        hp--;
        Debug.Log($"플레이어 피격! 남은 HP: {hp}");
        if (hp <= 0)
        {
            Debug.Log("게임 오버! 플레이어 HP가 0이 되었습니다.");
            // 2단계에서 GameManager의 게임 오버 로직과 연결될 예정입니다.
        }
    }
}