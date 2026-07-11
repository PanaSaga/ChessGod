using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // 인스펙터 창에서 이동할 씬의 이름을 직접 입력합니다
    // 예: "GameScene", "GalleryScene" 등
    [SerializeField] private string sceneName;

    // 버튼의 OnClick() 이벤트에 이 함수를 연결합니다
    public void LoadTargetScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("씬 이름이 비어있습니다. 인스펙터에서 sceneName을 입력해주세요.");
            return;
        }

        SceneManager.LoadScene(sceneName);
    }
}