using System.Collections.Generic;
using UnityEngine;

// ScriptableObject event channel that carries the AchievementData of the achievement that
// was just completed (Raise is called from AchievementTracker once progress reaches target).
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
        listeners.Remove(listener);
    }
}
