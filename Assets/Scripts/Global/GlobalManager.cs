using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    [Header("Sub managers")]
    [SerializeField] private DataManager dataManager;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private CommonUIManager commonUIManager;
    [SerializeField] private AchievementManager achievementManager;
    [SerializeField] private AchievementUIManager achievementUIManager;

    public DataManager DataManager => dataManager;
    public SoundManager SoundManager => soundManager;
    public CommonUIManager CommonUIManager => commonUIManager;
    public AchievementManager AchievementManager => achievementManager;
    public AchievementUIManager AchievementUIManager => achievementUIManager;

    // Set right before loading the InGame scene to control which mode it starts in.
    // TutorialManager reads and immediately resets this the moment it wakes up.
    public bool LaunchTutorialOnNextIngame { get; set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // Temporary test entry point until the lobby's "튜토리얼 다시보기" button exists.
    // Right-click this component's header in the Inspector (works during Play Mode too) and pick this.
    [ContextMenu("Debug: Launch Tutorial Now")]
    private void DebugLaunchTutorial()
    {
        LaunchTutorialOnNextIngame = true;
        SceneManager.LoadScene("InGame");
    }
}
