using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Wires the title screen's New Game / Continue / Load buttons. Settings and Exit need no script of
// their own - point their OnClick directly at CommonUIManager.OpenSettings() / QuitGame() in the Inspector.
public class StartScreenManager : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private SaveSlotPickerPopup slotPickerPopup;
    [SerializeField] private string mainLobbySceneName = "MainLobby";

    private void Start()
    {
        if (newGameButton != null) newGameButton.onClick.AddListener(OnNewGamePressed);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinuePressed);
        if (loadButton != null) loadButton.onClick.AddListener(OnLoadPressed);

        RefreshContinueButton();
    }

    // Disabled until at least one slot has ever been created/loaded.
    private void RefreshContinueButton()
    {
        if (continueButton == null) return;
        DataManager data = GlobalManager.Instance.DataManager;
        int lastSlot = data.GlobalSettings.lastUsedSlotIndex;
        continueButton.interactable = lastSlot >= 0 && data.HasSaveData(lastSlot);
    }

    private void OnNewGamePressed() => slotPickerPopup?.OpenForNewGame();

    private void OnLoadPressed() => slotPickerPopup?.OpenForLoad();

    private void OnContinuePressed()
    {
        DataManager data = GlobalManager.Instance.DataManager;
        int lastSlot = data.GlobalSettings.lastUsedSlotIndex;
        if (lastSlot < 0) return;

        data.LoadSlot(lastSlot);
        SceneManager.LoadScene(mainLobbySceneName);
    }
}
