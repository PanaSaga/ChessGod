using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Shared by both "새 게임 시작" and "불러오기" - only the filtering (Load hides empty slots) and
// what happens on pick (New Game may need an overwrite confirmation first) differ between modes.
public class SaveSlotPickerPopup : MonoBehaviour
{
    private enum Mode
    {
        NewGame,
        Load
    }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button closeButton;

    [Header("Slots (index 0-2)")]
    [SerializeField] private SaveSlotButton[] slotButtons;

    [Header("Overwrite confirmation (New Game only)")]
    [SerializeField] private ConfirmDialog confirmDialog;

    [Header("Scenes")]
    [SerializeField] private string ingameSceneName = SceneNames.InGame;
    [SerializeField] private string mainLobbySceneName = SceneNames.MainLobby;

    private Mode mode;

    private void Start()
    {
        if (closeButton != null) closeButton.onClick.AddListener(Close);
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void OpenForNewGame()
    {
        mode = Mode.NewGame;
        Refresh();
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void OpenForLoad()
    {
        mode = Mode.Load;
        Refresh();
        if (panelRoot != null) panelRoot.SetActive(true);
    }

    public void Close()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    private void Refresh()
    {
        if (slotButtons == null) return;
        DataManager data = GlobalManager.Instance.DataManager;

        for (int i = 0; i < slotButtons.Length; i++)
        {
            SaveSlotButton slotButton = slotButtons[i];
            if (slotButton == null) continue;

            int slotIndex = i;
            bool hasData = data.HasSaveData(slotIndex);

            // 불러오기: 데이터 있는 슬롯만 표시.
            if (mode == Mode.Load && !hasData)
            {
                slotButton.gameObject.SetActive(false);
                continue;
            }
            slotButton.gameObject.SetActive(true);

            string label = hasData
                ? BuildSummary(data.PeekSlot(slotIndex), slotIndex)
                : $"Slot {slotIndex + 1} - 비어있음";
            slotButton.Setup(label, () => OnSlotSelected(slotIndex, hasData));
        }
    }

    private static string BuildSummary(SaveSlotData slot, int slotIndex)
    {
        int achievedCount = slot != null ? slot.unlockedAchievementIds.Count : 0;
        return $"Slot {slotIndex + 1} - 업적 {achievedCount}개 달성";
    }

    private void OnSlotSelected(int slotIndex, bool hasData)
    {
        if (mode == Mode.Load)
        {
            GlobalManager.Instance.DataManager.LoadSlot(slotIndex);
            Close();
            SceneManager.LoadScene(mainLobbySceneName);
            return;
        }

        // New Game: an already-used slot needs an explicit "this will be wiped" confirmation.
        if (hasData) confirmDialog?.Show(() => StartNewGame(slotIndex));
        else StartNewGame(slotIndex);
    }

    private void StartNewGame(int slotIndex)
    {
        GlobalManager.Instance.DataManager.CreateNewSlot(slotIndex);
        GlobalManager.Instance.LaunchTutorialOnNextIngame = true;
        Close();
        SceneManager.LoadScene(ingameSceneName);
    }
}
