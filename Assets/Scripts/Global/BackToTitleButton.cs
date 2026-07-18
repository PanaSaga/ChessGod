using UnityEngine;
using UnityEngine.UI;

// Put this on any scene's own "타이틀로 돌아가기" button EXCEPT the one inside the settings panel
// itself (that one can link CommonUIManager.ConfirmBackToTitle() directly in the Inspector, since
// it lives in the same scene GlobalManager does - every other scene needs this wrapper instead).
public class BackToTitleButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(RequestBackToTitle);
    }

    private void RequestBackToTitle()
    {
        if (GlobalManager.Instance == null)
        {
            Debug.LogError("BackToTitleButton: no GlobalManager.Instance - make sure the game was started from the GameStart scene.");
            return;
        }

        GlobalManager.Instance.CommonUIManager.ConfirmBackToTitle();
    }
}
