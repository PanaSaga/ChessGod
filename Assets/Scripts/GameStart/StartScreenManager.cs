using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Wires the title screen's New Game / Continue / Load buttons.
public class StartScreenManager : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private string mainLobbySceneName = SceneNames.MainLobby;

    // Looked up through GlobalManager.Instance at call time rather than held as a direct
    // [SerializeField] reference - a direct reference into GlobalManager's own hierarchy from
    // another object in the same (GameStart) scene goes null the moment GlobalManager's
    // DontDestroyOnLoad kicks in, since that happens in this same scene.
    private SaveSlotPickerPopup SlotPickerPopup => GlobalManager.Instance.SaveSlotPickerPopup;

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

    private void OnNewGamePressed() => SlotPickerPopup?.OpenForNewGame();

    private void OnLoadPressed() => SlotPickerPopup?.OpenForLoad();

    private void OnContinuePressed()
    {
        DataManager data = GlobalManager.Instance.DataManager;
        int lastSlot = data.GlobalSettings.lastUsedSlotIndex;
        if (lastSlot < 0) return;

        data.LoadSlot(lastSlot);
        SceneManager.LoadScene(mainLobbySceneName);
    }
}
