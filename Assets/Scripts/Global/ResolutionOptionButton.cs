using UnityEngine;
using UnityEngine.UI;

// Put this on each resolution choice button in the settings panel (e.g. one for 1280x720, one
// for 1920x1080, one for 2560x1440) - set Width/Height per button in the Inspector.
public class ResolutionOptionButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private int width = 1920;
    [SerializeField] private int height = 1080;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(Apply);
    }

    private void Apply()
    {
        if (GlobalManager.Instance == null)
        {
            Debug.LogError("ResolutionOptionButton: no GlobalManager.Instance - make sure the game was started from the GameStart scene.");
            return;
        }

        GlobalManager.Instance.CommonUIManager.SetResolution(width, height);
    }
}
