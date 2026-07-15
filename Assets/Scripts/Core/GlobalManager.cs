using UnityEngine;

// Top-level container for the managers that must exist exactly once for the
// lifetime of the app. Lives in BootScene, survives every scene load, and
// destroys any duplicate created by re-entering BootScene.
public class GlobalManager : MonoBehaviour
{
    private static GlobalManager instance;

    public static IGameDataProvider DataProvider;

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
