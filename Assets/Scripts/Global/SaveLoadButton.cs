using UnityEngine;
using UnityEngine.UI;

// Put this on any scene's own "저장/불러오기" button - opens the shared slot picker popup
// (the same one used by the start screen's "불러오기") in Load mode.
public class SaveLoadButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OpenPicker);
    }

    private void OpenPicker()
    {
        if (GlobalManager.Instance == null)
        {
            Debug.LogError("SaveLoadButton: no GlobalManager.Instance - make sure the game was started from the GameStart scene.");
            return;
        }

        GlobalManager.Instance.SaveSlotPickerPopup.OpenForLoad();
    }
}
