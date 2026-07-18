using UnityEngine;
using UnityEngine.UI;

// Put this on Gallery's "업적" button - opens the same achievement list panel that's
// permanently open in the Lobby/InGame, just toggled here instead.
public class AchievementListButton : MonoBehaviour
{
    [SerializeField] private Button button;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(OpenList);
    }

    private void OpenList()
    {
        if (GlobalManager.Instance == null)
        {
            Debug.LogError("AchievementListButton: no GlobalManager.Instance - make sure the game was started from the GameStart scene.");
            return;
        }

        GlobalManager.Instance.AchievementUIManager.OpenListPanel();
    }
}
