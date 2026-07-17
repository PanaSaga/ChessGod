using UnityEngine;

public class GlobalManager : MonoBehaviour
{
    public static GlobalManager Instance { get; private set; }

    [Header("Sub managers")]
    [SerializeField] private DataManager dataManager;
    [SerializeField] private SoundManager soundManager;
    [SerializeField] private CommonUIManager commonUIManager;

    public DataManager DataManager => dataManager;
    public SoundManager SoundManager => soundManager;
    public CommonUIManager CommonUIManager => commonUIManager;

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
}
