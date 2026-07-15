using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

// Scene-local popup coordinator: maps buttons to popups, and toggles a designated popup
// with Esc. Not persistent -- MainScene points settingPopup at the lobby settings popup,
// InGame.unity points it at the pause popup. Each scene gets its own instance (destroyed
// and recreated on every scene load), so the singleton guard here only ever needs to
// protect against duplicates within the same scene.
public class CommonUIManager : MonoBehaviour
{
    [Serializable]
    public class ButtonPopupPair
    {
        public Button button;
        public PopupUI popup;
    }

    [SerializeField] private List<ButtonPopupPair> popupPairs = new List<ButtonPopupPair>();

    [Header("Esc 키로 여닫을 팝업 (로비=설정, 인게임=일시정지)")]
    [SerializeField] private PopupUI escTogglePopup;

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
        if (escTogglePopup == null) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentPopup == escTogglePopup)
                CloseCurrentPopup();
            else
                OpenPopup(escTogglePopup);
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
            currentPopup.Close();

        currentPopup = popup;
        popup.Open();
    }

    public void CloseCurrentPopup()
    {
        if (currentPopup == null) return;
        currentPopup.Close();
        currentPopup = null;
    }
}
