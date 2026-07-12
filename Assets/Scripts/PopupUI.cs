using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 팝업 창 하나의 활성화/비활성화 상태를 제어합니다.
/// 사용 대상: 설정(Setting), 도움말(Help), 업적(Quest/Achievement) 등 화면 위에 뜨는 창
/// 붙이는 위치: 각 팝업 오브젝트 (SettingPopup, HelpPopup, AchievementPopup)
///
/// [데이터 연동 설계 포인트]
/// 이 스크립트는 "보여주기/숨기기"만 담당하고, 실제 내용(갤러리 이미지, 업적 목록 등)을
/// 채우는 로직은 절대 여기에 넣지 않습니다. 대신 OnOpened 이벤트를 발행(broadcast)만 하고,
/// 데이터를 채워야 하는 팝업(Gallery, Achievement)은 별도의 "Populator" 스크립트를 만들어
/// 이 이벤트를 구독(subscribe)하는 방식으로 연결합니다.
/// → 이렇게 하면 나중에 게임 데이터를 연결할 때 PopupUI.cs를 단 한 줄도 수정할 필요가 없습니다.
/// </summary>
public class PopupUI : MonoBehaviour
{
    [Header("페이드 효과용 (선택사항, 없으면 비워둬도 정상 작동)")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("팝업이 열릴 때/닫힐 때 발생하는 이벤트 (데이터 갱신 스크립트가 여기에 구독)")]
    public UnityEvent OnOpened;
    public UnityEvent OnClosed;

    private void Awake()
    {
        // 게임 시작 시 팝업은 항상 꺼진 상태로 시작
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        // 열릴 때마다 신호만 보냄. 실제 갱신 로직은 이 신호를 듣는 쪽(Populator)이 처리.
        OnOpened?.Invoke();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        OnClosed?.Invoke();
    }
}
