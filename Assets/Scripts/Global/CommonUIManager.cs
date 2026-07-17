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

    [Header("Save")]
    [SerializeField] private Button saveButton;

    [Header("Help")]
    [SerializeField] private HelpPanel helpPanel;

    [Header("Give up (ingame only)")]
    [SerializeField] private Button giveUpButton;
    [SerializeField] private ConfirmDialog confirmDialog;
    [SerializeField] private string mainLobbySceneName = "MainLobby";

    private bool isSettingsOpen;

    private void Start()
    {
        if (closeSettingsButton != null) closeSettingsButton.onClick.AddListener(CloseSettings);
        if (saveButton != null) saveButton.onClick.AddListener(OnSavePressed);
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

        // Closes on top of the stack first: the give-up confirm, then help, then settings underneath.
        if (confirmDialog != null && confirmDialog.IsOpen)
        {
            confirmDialog.Cancel();
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
        // Only a live ingame session has anything to give up on.
        if (giveUpButton != null) giveUpButton.gameObject.SetActive(GameManager.Instance != null);
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

    private void OnSavePressed() => GlobalManager.Instance.DataManager.SaveCurrentSlot();

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
}
