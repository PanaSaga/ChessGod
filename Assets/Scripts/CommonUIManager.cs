using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

/// <summary>
/// 씬 공통 UI(팝업)를 총괄하는 매니저. (구 UIManager.cs — 매니저_구조.md의
/// "공통 UI 매니저" 명칭에 맞춰 확장: 버튼-팝업 매핑 + Esc 키로 설정 팝업 토글)
///
/// 붙이는 위치: GlobalManager 하위의 "CommonUIManager" 오브젝트
/// </summary>
public class CommonUIManager : MonoBehaviour
{
    [Serializable]
    public class ButtonPopupPair
    {
        public Button button;
        public PopupUI popup;
    }

    [Header("버튼-팝업 매핑 (Setting, Help, Achievement 등록)")]
    [SerializeField] private List<ButtonPopupPair> popupPairs = new List<ButtonPopupPair>();

    [Header("Esc 키로 여닫을 설정 팝업 (없으면 비워둬도 무방)")]
    [SerializeField] private PopupUI settingPopup;

    public static CommonUIManager Instance { get; private set; }

    private PopupUI currentPopup;

    private void Awake()
    {
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

    private void Update()
    {
        if (settingPopup == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPopup == settingPopup)
            {
                CloseCurrentPopup();
            }
            else
            {
                OpenPopup(settingPopup);
            }
        }
    }

    private void RegisterAll()
    {
        foreach (var pair in popupPairs)
        {
            if (pair.button == null || pair.popup == null)
            {
                Debug.LogWarning("[CommonUIManager] 등록되지 않은 버튼 또는 팝업이 있습니다.");
                continue;
            }

            PopupUI targetPopup = pair.popup;
            pair.button.onClick.AddListener(() => OpenPopup(targetPopup));
        }
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
