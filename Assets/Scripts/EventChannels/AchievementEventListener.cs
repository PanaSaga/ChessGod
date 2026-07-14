using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// AchievementUnlockedEvent(데이터를 실어 나르는 이벤트 채널)를 구독하는 브릿지 컴포넌트.
///
/// 붙이는 위치: AchievementToastManager와 같은 오브젝트
/// 인스펙터 설정:
///   - Achievement Event: OnAchievementUnlocked.asset 드래그
///   - Response: 오브젝트 슬롯에 자기 자신(AchievementToastManager가 붙은 오브젝트) 드래그
///               → 함수 AchievementToastManager > Enqueue(AchievementData) 선택
///               (파라미터 1개짜리 함수라 정상적으로 드롭다운에 표시됨)
/// </summary>
public class AchievementEventListener : MonoBehaviour
{
    [System.Serializable]
    public class AchievementUnityEvent : UnityEvent<AchievementData> { }

    [Header("구독할 이벤트 채널")]
    [SerializeField] private AchievementUnlockedEvent achievementEvent;

    [Header("이벤트 발생 시 실행할 함수 (파라미터 1개: AchievementData)")]
    [SerializeField] private AchievementUnityEvent response;

    private void OnEnable()
    {
        if (achievementEvent != null) achievementEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (achievementEvent != null) achievementEvent.UnregisterListener(this);
    }

    public void OnEventRaised(AchievementData data)
    {
        response?.Invoke(data);
    }
}
