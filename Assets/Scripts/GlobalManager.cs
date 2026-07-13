using UnityEngine;

/// <summary>
/// 게임 전체에서 하나만 존재해야 하는 매니저들의 최상위 컨테이너입니다.
/// (구 GameManagers.cs — 매니저_구조.md의 "글로벌 매니저" 명칭에 맞춰 이름 변경)
///
/// DontDestroyOnLoad로 씬 전환에도 파괴되지 않으며, 중복 생성 시 새로 생긴 쪽을 파괴합니다.
/// 하위에 CommonUIManager, DataManager, SoundManager, DataProviderSlot을 자식으로 둡니다.
///
/// 붙이는 위치: BootScene의 최상위 빈 오브젝트 ("GlobalManager")
/// </summary>
public class GlobalManager : MonoBehaviour
{
    private static GlobalManager instance;

    // IGameDataProvider 구현체(Stub 또는 실제 매니저)에 대한 전역 접근 지점
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
