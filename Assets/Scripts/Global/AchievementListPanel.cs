using UnityEngine;
using UnityEngine.UI;

public class AchievementListPanel : MonoBehaviour
{
    public enum Tab
    {
        All,
        Achieved,
        Unachieved
    }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [Tooltip("Only used when the panel is opened/closed via a button (e.g. Gallery). Hidden while always-open.")]
    [SerializeField] private Button closeButton;

    [Header("Tabs")]
    [SerializeField] private Button allTabButton;
    [SerializeField] private Button achievedTabButton;
    [SerializeField] private Button unachievedTabButton;

    [Header("List")]
    [SerializeField] private Transform listParent;
    [SerializeField] private AchievementListEntry entryPrefab;

    [Header("Secret achievement placeholder")]
    [SerializeField] private Sprite secretIcon;
    [SerializeField] private string secretDisplayName = "???";
    [SerializeField] private string secretDescription = "???";

    private AchievementManager achievementManager;
    private Tab currentTab = Tab.All;
    private bool unachievedOnly;

    private void Start()
    {
        if (allTabButton != null) allTabButton.onClick.AddListener(() => SelectTab(Tab.All));
        if (achievedTabButton != null) achievedTabButton.onClick.AddListener(() => SelectTab(Tab.Achieved));
        if (unachievedTabButton != null) unachievedTabButton.onClick.AddListener(() => SelectTab(Tab.Unachieved));
        if (closeButton != null) closeButton.onClick.AddListener(Close);
    }

    // Called once by AchievementUIManager once GlobalManager's sub-managers exist.
    public void Initialize(AchievementManager manager)
    {
        achievementManager = manager;
    }

    // Lobby/InGame: the panel is always visible, with no close button.
    public void ShowAlwaysOpen(bool unachievedOnly)
    {
        this.unachievedOnly = unachievedOnly;
        if (panelRoot != null) panelRoot.SetActive(true);
        if (closeButton != null) closeButton.gameObject.SetActive(false);
        if (allTabButton != null) allTabButton.gameObject.SetActive(!unachievedOnly);
        if (achievedTabButton != null) achievedTabButton.gameObject.SetActive(!unachievedOnly);
        SelectTab(unachievedOnly ? Tab.Unachieved : Tab.All);
    }

    // Other scenes (e.g. Gallery): stays hidden until a scene button calls Open().
    public void ShowToggleable()
    {
        unachievedOnly = false;
        if (panelRoot != null) panelRoot.SetActive(false);
        if (closeButton != null) closeButton.gameObject.SetActive(true);
        if (allTabButton != null) allTabButton.gameObject.SetActive(true);
        if (achievedTabButton != null) achievedTabButton.gameObject.SetActive(true);
    }

    public void Open()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        SelectTab(Tab.All);
    }

    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // Called by AchievementUIManager whenever an achievement unlocks, so an already-open panel updates live.
    public void Refresh() => SelectTab(currentTab);

    private void SelectTab(Tab tab)
    {
        currentTab = unachievedOnly ? Tab.Unachieved : tab;
        BuildList();
    }

    private void BuildList()
    {
        if (listParent == null || entryPrefab == null || achievementManager == null) return;

        foreach (Transform child in listParent) Destroy(child.gameObject);

        foreach (AchievementSO achievement in achievementManager.Achievements)
        {
            bool achieved = achievementManager.IsUnlocked(achievement.achievementId);
            if (currentTab == Tab.Achieved && !achieved) continue;
            if (currentTab == Tab.Unachieved && achieved) continue;

            bool hideDetails = achievement.isSecret && !achieved;
            bool showClaim = achieved && !achievementManager.IsAcknowledged(achievement.achievementId);

            AchievementListEntry entry = Instantiate(entryPrefab, listParent);
            entry.Setup(achievement,
                hideDetails ? secretDisplayName : null,
                hideDetails ? secretDescription : null,
                hideDetails ? secretIcon : null,
                achieved,
                achievementManager.GetProgress(achievement.achievementId),
                showClaim,
                () =>
                {
                    achievementManager.AcknowledgeAchievement(achievement.achievementId);
                    Refresh();
                });
        }
    }
}
