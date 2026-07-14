using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// "업적이 달성됐다"는 신호와 함께 어떤 업적인지(AchievementData)까지 실어 나르는
/// 전용 이벤트 채널입니다. 일반 GameEvent와 달리 데이터를 1개 갖고 다닙니다.
///
/// 사용법:
/// 1. Project 창에서 Create > Events > Achievement Unlocked Event 로 에셋 생성
///    (예: OnAchievementUnlocked.asset — 기존 파라미터 없는 버전을 이걸로 교체)
/// 2. 신호를 보내는 쪽(게임팀 AchievementDataManager): Raise(달성된 AchievementData) 호출
/// 3. 신호를 받는 쪽(AchievementToastManager): AchievementEventListener.cs를 부착하고
///    이 에셋을 등록, Response에 AchievementToastManager.Enqueue(AchievementData) 연결
///    → 파라미터가 정확히 1개(AchievementData)인 함수라 인스펙터 드롭다운에 정상적으로 나타납니다.
/// </summary>
[CreateAssetMenu(menuName = "Events/Achievement Unlocked Event", fileName = "OnAchievementUnlocked")]
public class AchievementUnlockedEvent : ScriptableObject
{
    private readonly List<AchievementEventListener> listeners = new List<AchievementEventListener>();

    public void Raise(AchievementData data)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised(data);
        }
    }

    public void RegisterListener(AchievementEventListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void UnregisterListener(AchievementEventListener listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }
}
