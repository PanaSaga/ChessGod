using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Put this on the lobby's "튜토리얼 다시보기" button instead of a plain SceneLoadButton -
// it flips the tutorial flag on before loading the InGame scene.
public class TutorialLaunchButton : MonoBehaviour
{
    [SerializeField] private string ingameSceneName = "InGame";
    [SerializeField] private Button button;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(LaunchTutorial);
    }

    private void LaunchTutorial()
    {
        if (GlobalManager.Instance == null)
        {
            Debug.LogError("TutorialLaunchButton: no GlobalManager.Instance - make sure the game was started from the GameStart scene.");
            return;
        }

        GlobalManager.Instance.LaunchTutorialOnNextIngame = true;
        SceneManager.LoadScene(ingameSceneName);
    }
}
