using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoadButton : MonoBehaviour
{
    [SerializeField] private string targetSceneName;
    [SerializeField] private Button button;

    private void Start()
    {
        if (button == null) button = GetComponent<Button>();
        if (button != null) button.onClick.AddListener(LoadTargetScene);
    }

    private void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError($"SceneLoadButton on '{gameObject.name}' has no Target Scene Name set.");
            return;
        }
        SceneManager.LoadScene(targetSceneName);
    }
}
