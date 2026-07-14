using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

/// <summary>
/// 버튼 클릭 시 지정된 이름의 씬을 비동기(Async) 방식으로 로드합니다.
/// 씬 전환 시작 직전 OnSceneTransition 이벤트를 Raise하여,
/// SoundManager 등이 BGM 페이드 아웃 등을 자동으로 처리할 수 있게 합니다.
///
/// 붙이는 위치: Start, Gallery, BackButton 등 씬 전환 버튼
/// 사전 준비: Build Settings에 이동할 씬이 등록되어 있어야 합니다.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [Header("이동할 씬 이름")]
    [SerializeField] private string sceneName;

    [Header("씬 전환 시작 시 발생시킬 이벤트 (선택사항, 없어도 작동)")]
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
        {
            yield return null;
        }
    }
}
