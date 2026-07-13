using UnityEngine;

/// <summary>
/// 로비(MainScene) 진입 시 초기화를 담당합니다.
/// 데이터 매니저를 확인해 프롤로그 컷씬을 본 적 없다면 StoryManager를 호출해 재생합니다.
///
/// 붙이는 위치: MainScene의 Panel 하위 빈 오브젝트 ("LobbyManager")
/// </summary>
public class LobbyManager : MonoBehaviour
{
    [Header("프롤로그 재생을 담당할 StoryManager (씬에 없다면 비워둠)")]
    [SerializeField] private StoryManager storyManager;

    private void Awake()
    {
        CheckPrologue();
    }

    private void CheckPrologue()
    {
        if (GlobalManager.DataProvider == null)
        {
            Debug.LogWarning("[LobbyManager] DataProvider가 아직 연결되지 않았습니다.");
            return;
        }

        bool hasSeenPrologue = GlobalManager.DataProvider.HasSeenPrologue();

        if (!hasSeenPrologue && storyManager != null)
        {
            storyManager.PlayPrologue();
        }
    }
}
