using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 버튼 클릭 시 지정된 이름의 씬을 비동기(Async) 방식으로 로드합니다.
/// 사용 대상: 게임 시작(Start), 갤러리(Gallery) 등 씬 전체를 갈아끼우는 버튼
/// 붙이는 위치: 각 버튼 오브젝트 (Start, Gallery)
/// 사전 준비: File > Build Settings 에 이동할 씬이 반드시 등록되어 있어야 합니다.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("이동할 씬 이름 (Build Settings에 등록된 이름과 정확히 일치해야 함)")]
    [SerializeField] private string sceneName;

    // 버튼의 OnClick()에 이 함수를 연결합니다.
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("[SceneLoader] 씬 이름이 비어있습니다. 인스펙터에서 sceneName을 입력해주세요.");
            return;
        }

        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string targetScene)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(targetScene);

        // 씬이 다 로드될 때까지 매 프레임 대기 (화면이 멈추지 않고 계속 그려짐)
        while (operation != null && !operation.isDone)
        {
            // operation.progress (0~1) 값을 나중에 로딩바에 연결할 수 있습니다.
            yield return null;
        }
    }
}
