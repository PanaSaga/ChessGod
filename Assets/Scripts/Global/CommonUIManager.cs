using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CommonUIManager : MonoBehaviour
{
    [Header("Settings panel")]
    [SerializeField] private GameObject settingsPanelRoot;
    [SerializeField] private Button closeSettingsButton;

    [Header("Volume controls")]
    [SerializeField] private Slider bgmVolumeSlider;
    [SerializeField] private Toggle bgmMuteToggle;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Toggle sfxMuteToggle;

    [Header("Back to title (hidden during an active ingame session - use Give Up for that instead)")]
    [SerializeField] private GameObject backToTitleButtonRoot;
    [SerializeField] private ConfirmDialog backToTitleConfirmDialog;
    [SerializeField] private string gameStartSceneName = SceneNames.GameStart;

    [Header("Help")]
    [SerializeField] private HelpPanel helpPanel;

    [Header("Give up (ingame only)")]
    [SerializeField] private Button giveUpButton;
    [SerializeField] private ConfirmDialog confirmDialog;
    [SerializeField] private string mainLobbySceneName = SceneNames.MainLobby;

    private bool isSettingsOpen;

    private void Start()
    {
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (giveUpButton != null) giveUpButton.onClick.AddListener(OnGiveUpPressed);

        if (bgmVolumeSlider != null) bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
        if (bgmMuteToggle != null) bgmMuteToggle.onValueChanged.AddListener(OnBgmMuteChanged);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
        if (sfxMuteToggle != null) sfxMuteToggle.onValueChanged.AddListener(OnSfxMuteChanged);

        if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);
    }

    private void Update()
    {
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        // Closes on top of the stack first: whichever confirm dialog is up, then help, then settings underneath.
        if (confirmDialog != null && confirmDialog.IsOpen)
        {
            confirmDialog.Cancel();
            return;
        }

        if (backToTitleConfirmDialog != null && backToTitleConfirmDialog.IsOpen)
        {
            backToTitleConfirmDialog.Cancel();
            return;
        }

        if (helpPanel != null && helpPanel.IsOpen)
        {
            helpPanel.CloseHelp();
            return;
        }

        if (isSettingsOpen) CloseSettings();
        else OpenSettings();
    }

    // Any scene's own gear-icon button can call this directly (see the Unity setup notes for how).
    public void OpenSettings()
    {
        if (isSettingsOpen) return;
        isSettingsOpen = true;
        if (settingsPanelRoot != null) settingsPanelRoot.SetActive(true);
        RefreshVolumeControls();
        // Only a live ingame session has anything to give up on - and only outside one does
        // "back to title" make sense as a plain exit (ingame uses the confirm-gated Give Up instead).
        if (giveUpButton != null) giveUpButton.gameObject.SetActive(GameManager.Instance != null);
        if (backToTitleButtonRoot != null) backToTitleButtonRoot.SetActive(GameManager.Instance == null);
        if (GameManager.Instance != null) GameManager.Instance.isPaused = true;
    }

    public void CloseSettings()
    {
        if (!isSettingsOpen) return;
        isSettingsOpen = false;
        if (settingsPanelRoot != null) settingsPanelRoot.SetActive(false);
        if (GameManager.Instance != null) GameManager.Instance.isPaused = false;
    }

    private void RefreshVolumeControls()
    {
        GlobalSettingsData settings = GlobalManager.Instance != null ? GlobalManager.Instance.DataManager.GlobalSettings : null;
        if (settings == null) return;

        if (bgmVolumeSlider != null) bgmVolumeSlider.SetValueWithoutNotify(settings.bgmVolume);
        if (bgmMuteToggle != null) bgmMuteToggle.SetIsOnWithoutNotify(settings.isBgmMuted);
        if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(settings.sfxVolume);
        if (sfxMuteToggle != null) sfxMuteToggle.SetIsOnWithoutNotify(settings.isSfxMuted);
    }

    private void OnBgmVolumeChanged(float value) => GlobalManager.Instance.SoundManager.SetBgmVolume(value);
    private void OnBgmMuteChanged(bool isMuted) => GlobalManager.Instance.SoundManager.SetBgmMuted(isMuted);
    private void OnSfxVolumeChanged(float value) => GlobalManager.Instance.SoundManager.SetSfxVolume(value);
    private void OnSfxMuteChanged(bool isMuted) => GlobalManager.Instance.SoundManager.SetSfxMuted(isMuted);

    // Any scene's own exit button (start screen, settings panel) can call this directly.
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void OnGiveUpPressed()
    {
        confirmDialog?.Show(ConfirmGiveUp);
    }

    // Yes: close the (persistent) settings UI so it doesn't stay open on top of the lobby, then leave.
    private void ConfirmGiveUp()
    {
        CloseSettings();
        SceneManager.LoadScene(mainLobbySceneName);
    }

    // Any scene's own "타이틀로 돌아가기" button can call this directly (it's not gated by
    // isSettingsOpen, so it also works from a button that isn't inside the settings panel, like the lobby's).
    public void ConfirmBackToTitle()
    {
        backToTitleConfirmDialog?.Show(GoToTitle);
    }

    private void GoToTitle()
    {
        CloseSettings();
        SceneManager.LoadScene(gameStartSceneName);
    }
}
