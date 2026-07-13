using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// BootScene이 실행되면, GlobalManager(및 하위 DataProvider 등)의 Awake()가
/// 전부 끝난 뒤 MainScene을 Additive(겹쳐 로드) 방식으로 불러옵니다.
///
/// 붙이는 위치: BootScene의 빈 오브젝트 ("BootLoader")
///
/// [테스트 방법] 앞으로는 반드시 BootScene을 Hierarchy에 띄운 상태로 Play를 눌러야
/// GlobalManager.DataProvider가 정상적으로 연결됩니다. MainScene만 단독으로 열고
/// Play를 누르면 이 스크립트 자체가 실행되지 않으므로 여전히 경고가 뜹니다.
/// </summary>
public class BootLoader : MonoBehaviour
{
    [Header("가장 먼저 겹쳐 로드할 씬 이름")]
    [SerializeField] private string firstSceneName = "MainScene";

    // Start()는 씬의 모든 오브젝트의 Awake()가 끝난 뒤 호출되므로,
    // GlobalManager/DataProviderSlot의 Awake()가 먼저 실행된 뒤 안전하게 다음 씬을 로드합니다.
    private void Start()
    {
        SceneManager.LoadScene(firstSceneName, LoadSceneMode.Additive);
    }
}
