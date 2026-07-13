using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// GameEvent(ScriptableObject 이벤트 채널)를 구독하는 브릿지 컴포넌트.
/// 씬의 아무 오브젝트에나 부착하고, 인스펙터에서 구독할 GameEvent 에셋과
/// 신호가 왔을 때 실행할 함수(Response)를 연결합니다.
///
/// 예: SoundManager 오브젝트에 부착 → Game Event에 OnSceneTransition.asset 연결
///     → Response에 SoundManager.FadeOutBGM() 연결
/// </summary>
public class GameEventListener : MonoBehaviour
{
    [Header("구독할 이벤트 채널")]
    [SerializeField] private GameEvent gameEvent;

    [Header("이벤트 발생 시 실행할 함수")]
    [SerializeField] private UnityEvent response;

    private void OnEnable()
    {
        if (gameEvent != null) gameEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (gameEvent != null) gameEvent.UnregisterListener(this);
    }

    public void OnEventRaised()
    {
        response?.Invoke();
    }
}
