using UnityEngine;
using UnityEngine.SceneManagement;

// Bridge for the lobby settings popup's "처음부터" button. ConfirmDialogManager.Show()
// takes an Action callback that Unity's OnClick() inspector can't bind directly to, so
// this wraps it in a parameterless method. Not used by the ingame pause popup (설계 원칙 9
// -- 처음부터 is lobby-only).
public class ResetProgressHandler : MonoBehaviour
{
    [SerializeField] private string sceneNameAfterReset = "MainScene";

    public void OnResetButtonClicked()
    {
        if (ConfirmDialogManager.Instance == null)
        {
            Debug.LogWarning("[ResetProgressHandler] ConfirmDialogManager가 씬에 없습니다.");
            return;
        }

        ConfirmDialogManager.Instance.Show(
            "정말 처음부터 다시 시작하시겠습니까?\n지금까지의 진행 데이터가 모두 초기화됩니다.",
            DoReset
        );
    }

    private void DoReset()
    {
        GlobalManager.DataProvider?.ResetProgress();
        SceneManager.LoadScene(sceneNameAfterReset);
    }
}
