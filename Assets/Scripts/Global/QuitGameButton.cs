using UnityEngine;
using UnityEngine.UI;

// Put this on any scene's own exit/quit button, including GameStart's - a direct Inspector link to
// CommonUIManager.QuitGame() only works reliably from other scenes. In GameStart itself, a direct
// reference into GlobalManager's hierarchy goes null the instant GlobalManager's DontDestroyOnLoad
// runs (since that happens in this same scene), so every scene looks it up through
// GlobalManager.Instance at click time instead, for consistency.
public class QuitGameButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(Quit);
    }

    private void Quit()
    {
        if (GlobalManager.Instance == null)
        {
            Debug.LogError("QuitGameButton: no GlobalManager.Instance - make sure the game was started from the GameStart scene.");
            return;
        }

        GlobalManager.Instance.CommonUIManager.QuitGame();
    }
}
