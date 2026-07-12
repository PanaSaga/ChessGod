using UnityEngine;

/// <summary>
/// 게임 전체에서 하나만 존재해야 하는 매니저들의 부모 오브젝트에 부착합니다.
/// DontDestroyOnLoad로 씬이 전환되어도 파괴되지 않으며,
/// 씬을 재진입해 중복 생성되는 경우 새로 생긴 쪽을 스스로 파괴해 1개만 유지합니다.
/// 붙이는 위치: 최초 실행 씬(MainScene)의 최상위 빈 오브젝트 (예: "GameManagers")
///             그 자식으로 UIManager 등을 배치하면 함께 유지됩니다.
/// </summary>
public class GameManagers : MonoBehaviour
{
    private static GameManagers instance;

    private void Awake()
    {
        if (instance != null)
        {
            // 이미 매니저가 존재하면 이번에 새로 생긴 쪽을 파괴 (중복 방지)
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
