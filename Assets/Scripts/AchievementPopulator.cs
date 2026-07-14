using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 팝업이 "열릴 때마다" IGameDataProvider로부터 최신 진행 상황을 받아 화면을 갱신합니다.
/// PopupUI는 건드리지 않고, PopupUI가 보내는 OnOpened 이벤트만 받아서 동작합니다.
///
/// 붙이는 위치: AchievementPopup 오브젝트 (PopupUI와 같은 오브젝트)
///
/// [연결 방법 - 유니티 에디터]
/// 1. PopupUI 컴포넌트의 On Opened() 리스트에 + 클릭
/// 2. 오브젝트 슬롯에 자기 자신 드래그
/// 3. 함수 선택 → AchievementPopulator > RefreshAchievements()
///
/// [게임팀 작업물 연결 시 필요한 작업]
/// 없음. GlobalManager.DataProvider 자리에 실제 구현체만 꽂히면 이 스크립트는 그대로 작동합니다.
/// 단, AchievementSlotUI.Setup(AchievementData) 함수는 UI팀이 슬롯 프리팹에 맞게 미리 작성해둬야 합니다.
/// </summary>
public class AchievementPopulator : MonoBehaviour
{
    [Header("업적 항목이 생성될 부모 오브젝트 (리스트 레이아웃 등)")]
    [SerializeField] private Transform contentParent;

    [Header("업적 항목 하나의 프리팹 (AchievementSlotUI 컴포넌트 포함)")]
    [SerializeField] private GameObject achievementSlotPrefab;

    // PopupUI.OnOpened 이벤트에 연결하는 함수
    public void RefreshAchievements()
    {
        if (GlobalManager.DataProvider == null)
        {
            Debug.LogWarning("[AchievementPopulator] DataProvider가 아직 연결되지 않았습니다. " +
                              "GlobalManager 하위에 IGameDataProvider 구현체(Stub 또는 실제 매니저)가 있는지 확인하세요.");
            return;
        }

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<AchievementData> achievements = GlobalManager.DataProvider.GetAllAchievements();

        foreach (var data in achievements)
        {
            GameObject slot = Instantiate(achievementSlotPrefab, contentParent);

            var slotUI = slot.GetComponent<AchievementSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(data);
            }
        }
    }

    // ---------------------------------------------------------------
    // [주의] 유니티 Button.OnClick() 인스펙터는 파라미터가 1개 이하인
    // 함수만 드롭다운에 표시합니다. 파라미터 2개짜리 함수는 인스펙터에서
    // 선택할 수 없으므로, 탭 버튼 3개에 각각 연결할 수 있도록
    // 파라미터 없는 함수 3개로 나눠서 제공합니다.
    // ---------------------------------------------------------------

    // Tab_Progress(전체) 버튼의 OnClick()에 연결
    public void ShowAll()
    {
        RefreshFiltered(false, false);
    }

    // Tab_Completed(달성) 버튼의 OnClick()에 연결
    public void ShowCompletedOnly()
    {
        RefreshFiltered(true, false);
    }

    // Tab_Incomplete(미달성) 버튼의 OnClick()에 연결
    public void ShowIncompleteOnly()
    {
        RefreshFiltered(false, true);
    }

    // 실제 필터링 로직 (내부용, 인스펙터에서는 직접 연결하지 않음)
    private void RefreshFiltered(bool onlyCompleted, bool onlyIncomplete)
    {
        if (GlobalManager.DataProvider == null) return;

        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        List<AchievementData> achievements = GlobalManager.DataProvider.GetAllAchievements();

        foreach (var data in achievements)
        {
            if (onlyCompleted && !data.isCompleted) continue;
            if (onlyIncomplete && data.isCompleted) continue;

            GameObject slot = Instantiate(achievementSlotPrefab, contentParent);
            var slotUI = slot.GetComponent<AchievementSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(data);
            }
        }
    }
}
