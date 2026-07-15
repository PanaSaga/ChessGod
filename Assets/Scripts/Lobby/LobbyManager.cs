using UnityEngine;

// MainScene entry point: plays the prologue on first visit only.
public class LobbyManager : MonoBehaviour
{
    [SerializeField] private StoryManager storyManager;

    private void Awake()
    {
        CheckPrologue();
    }

    private void CheckPrologue()
    {
        if (GlobalManager.DataProvider == null)
        {
            Debug.LogWarning("[LobbyManager] DataProvider가 아직 연결되지 않았습니다.");
            return;
        }

        bool hasSeenPrologue = GlobalManager.DataProvider.HasSeenPrologue();
        if (!hasSeenPrologue && storyManager != null)
            storyManager.PlayPrologue();
    }
}
