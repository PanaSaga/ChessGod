using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// Asynchronously loads a target scene (Single mode -- replaces every currently loaded
// scene except whatever is DontDestroyOnLoad, e.g. GlobalManager). Raises an optional
// GameEvent right before the load starts so SoundManager can fade the BGM out.
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private GameEvent onSceneTransitionEvent;

    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneLoader] 씬 이름이 비어있습니다. 인스펙터에서 sceneName을 입력해주세요.");
            return;
        }

        onSceneTransitionEvent?.Raise();
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string targetScene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);
        while (operation != null && !operation.isDone)
            yield return null;
    }
}
