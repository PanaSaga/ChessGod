using UnityEngine;
using UnityEngine.Events;

// Bridge component that subscribes to an AchievementUnlockedEvent asset and forwards the
// AchievementData payload to a UnityEvent<AchievementData> (single-parameter function,
// so it shows up correctly in the OnClick()-style inspector dropdown).
public class AchievementEventListener : MonoBehaviour
{
    [System.Serializable]
    public class AchievementUnityEvent : UnityEvent<AchievementData> { }

    [SerializeField] private AchievementUnlockedEvent achievementEvent;
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
