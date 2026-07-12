using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 게임 전체의 팝업 열기/닫기를 총괄하는 매니저입니다.
/// - 인스펙터의 리스트에 "버튼-팝업" 짝을 등록해두면 자동으로 클릭 이벤트를 연결합니다.
/// - 팝업이 동시에 2개 이상 열리지 않도록 통제합니다.
/// - 씬이 바뀌어도 GameManagers를 통해 계속 살아있으므로, 게임 화면에서도 그대로 재사용됩니다.
/// 붙이는 위치: GameManagers 오브젝트 (씬에 딱 하나만 존재)
/// 주의: popupPairs에 등록한 버튼은 유니티 에디터의 OnClick() 리스트를 비워둘 것
///       (코드로 이벤트를 연결하므로 중복 연결 시 팝업이 두 번 열리려다 충돌할 수 있음)
/// </summary>
public class UIManager : MonoBehaviour
{
    [Serializable]
    public class ButtonPopupPair
    {
        public Button button;
        public PopupUI popup;
    }

    [Header("버튼-팝업 매핑 (Setting, Help, Quest 등록)")]
    [SerializeField] private List<ButtonPopupPair> popupPairs = new List<ButtonPopupPair>();

    public static UIManager Instance { get; private set; }

    private PopupUI currentPopup;

    private void Awake()
    {
        // 씬에 하나만 존재하도록 보장 (간단한 싱글톤 패턴)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        RegisterAll();
    }

    private void RegisterAll()
    {
        foreach (var pair in popupPairs)
        {
            if (pair.button == null || pair.popup == null)
            {
                Debug.LogWarning("[UIManager] 등록되지 않은 버튼 또는 팝업이 있습니다.");
                continue;
            }

            // 클로저 문제 방지를 위해 지역 변수에 복사
            PopupUI targetPopup = pair.popup;
            pair.button.onClick.AddListener(() => OpenPopup(targetPopup));
        }
    }

    // 씬마다 새로 생기는 버튼을 코드에서 직접 등록하고 싶을 때 사용 (선택사항)
    public void RegisterButton(Button button, PopupUI popup)
    {
        if (button == null || popup == null) return;

        button.onClick.AddListener(() => OpenPopup(popup));
    }

    public void OpenPopup(PopupUI popup)
    {
        if (currentPopup != null)
        {
            currentPopup.Close();
        }

        currentPopup = popup;
        popup.Open();
    }

    public void CloseCurrentPopup()
    {
        if (currentPopup != null)
        {
            currentPopup.Close();
            currentPopup = null;
        }
    }
}
