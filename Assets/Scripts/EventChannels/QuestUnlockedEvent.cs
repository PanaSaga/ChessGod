using System.Collections.Generic;
using UnityEngine;

// ScriptableObject event channel that carries the QuestData of the quest that
// was just completed (Raise is called from QuestTracker once progress reaches target).
[CreateAssetMenu(menuName = "Events/Quest Unlocked Event", fileName = "OnQuestUnlocked")]
public class QuestUnlockedEvent : ScriptableObject
{
    private readonly List<QuestEventListener> listeners = new List<QuestEventListener>();

    public void Raise(QuestData data)
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventRaised(data);
        }
    }

    public void RegisterListener(QuestEventListener listener)
    {
        if (!listeners.Contains(listener))
        {
            listeners.Add(listener);
        }
    }

    public void UnregisterListener(QuestEventListener listener)
    {
        listeners.Remove(listener);
    }
}
