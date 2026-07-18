using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementUIManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainLobbySceneName = "MainLobby";
    [SerializeField] private string inGameSceneName = "InGame";

    [Header("UI")]
    [SerializeField] private AchievementToast toast;
    [SerializeField] private AchievementListPanel listPanel;

    [Header("List panel position per scene")]
    [Tooltip("Anchored Position to place the list panel at while in the Lobby scene.")]
    [SerializeField] private Vector2 lobbyListPosition = new(-600f, -250f);
    [Tooltip("Anchored Position to place the list panel at while in the InGame scene.")]
    [SerializeField] private Vector2 inGameListPosition = new(600f, -250f);

    private readonly Queue<AchievementSO> toastQueue = new();
    private bool isShowingToast;
    private AchievementManager achievementManager;

    private void Start()
    {
        if (GlobalManager.Instance == null)
        {
            Debug.LogError("AchievementUIManager requires GlobalManager.Instance - make sure the game was started from the GameStart scene.");
            enabled = false;
            return;
        }

        achievementManager = GlobalManager.Instance.AchievementManager;
        achievementManager.OnAchievementUnlocked += OnAchievementUnlocked;
        if (listPanel != null) listPanel.Initialize(achievementManager);

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySceneMode(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (achievementManager != null) achievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplySceneMode(scene.name);

    private void ApplySceneMode(string sceneName)
    {
        if (listPanel == null) return;

        if (sceneName == inGameSceneName)
        {
            listPanel.ShowAlwaysOpen(AchievementListPanel.Tab.Unachieved);
            listPanel.SetAnchoredPosition(inGameListPosition);
        }
        else if (sceneName == mainLobbySceneName)
        {
            listPanel.ShowAlwaysOpen(AchievementListPanel.Tab.All);
            listPanel.SetAnchoredPosition(lobbyListPosition);
        }
        else
        {
            listPanel.ShowToggleable();
        }
    }

    private void OnAchievementUnlocked(AchievementSO achievement)
    {
        toastQueue.Enqueue(achievement);
        if (listPanel != null) listPanel.Refresh();
        if (!isShowingToast) ShowNextToast();
    }

    private void ShowNextToast()
    {
        if (toastQueue.Count == 0)
        {
            isShowingToast = false;
            return;
        }

        isShowingToast = true;
        AchievementSO next = toastQueue.Dequeue();
        if (toast != null) toast.Show(next, ShowNextToast);
        else ShowNextToast();
    }

    // Called by a scene-local button (e.g. Gallery's "업적" button).
    public void OpenListPanel()
    {
        if (listPanel != null) listPanel.Open();
    }
}
