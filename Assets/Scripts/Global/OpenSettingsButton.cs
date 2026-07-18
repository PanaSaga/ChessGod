using UnityEngine;
using UnityEngine.UI;

// Put this on any scene's own settings/gear-icon button EXCEPT GameStart's (GameStart can link
// CommonUIManager.OpenSettings() directly in the Inspector since it's the same scene GlobalManager
// lives in - every other scene needs this wrapper because that cross-scene reference isn't possible).
public class OpenSettingsButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OpenSettings);
    }

    private void OpenSettings()
    {
        if (GlobalManager.Instance == null)
        {
            Debug.LogError("OpenSettingsButton: no GlobalManager.Instance - make sure the game was started from the GameStart scene.");
            return;
        }

        GlobalManager.Instance.CommonUIManager.OpenSettings();
    }
}
