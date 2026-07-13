using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject 기반 이벤트 채널.
/// 씬 오브젝트가 아니라 Project 창의 에셋 파일이므로, 서로 다른 씬/스크립트가
/// 직접 참조를 몰라도 이 에셋 하나를 통해 신호를 주고받을 수 있습니다.
///
/// 사용법:
/// 1. Project 창 우클릭 > Create > Events > Game Event 로 에셋 생성
///    (예: OnAchievementUnlocked, OnSettingChanged, OnSceneTransition)
/// 2. 신호를 보내는 쪽: 이 에셋을 참조해 Raise() 호출
/// 3. 신호를 받는 쪽: GameEventListener.cs를 부착하고 이 에셋을 등록
/// </summary>
[CreateAssetMenu(menuName = "Events/Game Event", fileName = "NewGameEvent")]
public class GameEvent : ScriptableObject
{
    private readonly List<GameEventListener> listeners = new List<GameEventListener>();

    public void Raise()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised();
        }
    }

    public void RegisterListener(GameEventListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void UnregisterListener(GameEventListener listener)
    {
        if (listeners.Contains(listener))
        {
            listeners.Remove(listener);
        }
    }
}
