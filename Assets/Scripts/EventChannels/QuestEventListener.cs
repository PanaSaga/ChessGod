using UnityEngine;
using UnityEngine.Events;

// Bridge component that subscribes to a QuestUnlockedEvent asset and forwards the
// QuestData payload to a UnityEvent<QuestData> (single-parameter function,
// so it shows up correctly in the OnClick()-style inspector dropdown).
public class QuestEventListener : MonoBehaviour
{
    [System.Serializable]
    public class QuestUnityEvent : UnityEvent<QuestData> { }

    [SerializeField] private QuestUnlockedEvent questEvent;
    [SerializeField] private QuestUnityEvent response;

    private void OnEnable()
    {
        if (questEvent != null) questEvent.RegisterListener(this);
    }

    private void OnDisable()
    {
        if (questEvent != null) questEvent.UnregisterListener(this);
    }

    public void OnEventRaised(QuestData data)
    {
        response?.Invoke(data);
    }
}
