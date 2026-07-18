using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AchievementUIManager : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string mainLobbySceneName = SceneNames.MainLobby;
    [SerializeField] private string inGameSceneName = SceneNames.InGame;

    [Header("UI")]
    [SerializeField] private AchievementToast toast;
    [SerializeField] private AchievementListPanel listPanel;

    [Header("List panel position per scene")]
    [Tooltip("Anchored Position to place the list panel at while in the Lobby scene.")]
    [SerializeField] private Vector2 lobbyListPosition = new(-600f, -250f);
    [Tooltip("Anchored Position to place the list panel at while in the InGame scene.")]
    [SerializeField] private Vector2 inGameListPosition = new(600f, -250f);

    [Header("List panel scale per scene")]
    [SerializeField] private float lobbyListScale = 1f;
    [SerializeField] private float inGameListScale = 0.8f;

    private readonly Queue<AchievementSO> toastQueue = new();
    private bool isShowingToast;
    private bool isInGameScene;
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
        achievementManager.OnAchievementProgressChanged += OnAchievementProgressChanged;
        if (listPanel != null) listPanel.Initialize(achievementManager);

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySceneMode(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        if (achievementManager != null)
        {
            achievementManager.OnAchievementUnlocked -= OnAchievementUnlocked;
            achievementManager.OnAchievementProgressChanged -= OnAchievementProgressChanged;
        }
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // The tutorial runs inside the InGame scene itself (no separate scene load), so hiding the
    // panel for it can't be driven by ApplySceneMode - it has to be polled every frame instead.
    private void Update()
    {
        if (!isInGameScene || listPanel == null) return;
        bool tutorialActive = GameManager.Instance != null && GameManager.Instance.isTutorialActive;
        listPanel.SetVisible(!tutorialActive);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => ApplySceneMode(scene.name);

    private void ApplySceneMode(string sceneName)
    {
        isInGameScene = sceneName == inGameSceneName;
        if (listPanel == null) return;

        if (isInGameScene)
        {
            listPanel.ShowAlwaysOpen(AchievementListPanel.Tab.Unachieved);
            listPanel.SetAnchoredPosition(inGameListPosition);
            listPanel.SetScale(inGameListScale);
        }
        else if (sceneName == mainLobbySceneName)
        {
            listPanel.ShowAlwaysOpen(AchievementListPanel.Tab.All);
            listPanel.SetAnchoredPosition(lobbyListPosition);
            listPanel.SetScale(lobbyListScale);
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

    private void OnAchievementProgressChanged(AchievementSO achievement)
    {
        if (listPanel == null) return;
        listPanel.UpdateProgress(achievement.achievementId, achievementManager.GetProgress(achievement.achievementId), achievement.targetCount);
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
