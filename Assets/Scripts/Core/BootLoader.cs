using UnityEngine;
using UnityEngine.SceneManagement;

// Loads MainScene additively once every object under GlobalManager has
// finished Awake(). Must be placed in BootScene, and BootScene must be the
// scene that is open/loaded first when entering Play mode.
public class BootLoader : MonoBehaviour
{
    [SerializeField] private string firstSceneName = "MainScene";

    private void Start()
    {
        SceneManager.LoadScene(firstSceneName, LoadSceneMode.Additive);
    }
}
