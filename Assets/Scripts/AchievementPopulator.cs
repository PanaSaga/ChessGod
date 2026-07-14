using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 패널이 씬 시작 시 자동으로 목록을 채우고,
/// 탭 버튼으로 필터링할 수 있는 스크립트입니다.
///
/// 붙이는 위치: AchievementPanel 오브젝트 (PopupUI 없이 상시 노출되는 패널)
///
/// [연결 방법 - 유니티 에디터]
/// 1. Content Parent 슬롯에 ScrollView > Viewport > Content 드래그
/// 2. Achievement Slot Prefab 슬롯에 업적 슬롯 프리팹 연결
/// 3. 탭 버튼 OnClick()에 ShowAll / ShowCompletedOnly / ShowIncompleteOnly 연결
/// → PopupUI.OnOpened 이벤트 연결은 불필요 (씬 시작 시 Start()가 자동 호출)
///
/// [게임팀 작업물 연결 시 필요한 작업]
/// 없음. GlobalManager.DataProvider 자리에 실제 구현체만 꽂히면 그대로 작동합니다.
/// </summary>
public class AchievementPopulator : MonoBehaviour
{
    [Header("업적 항목이 생성될 부모 오브젝트 (ScrollView > Viewport > Content)")]
    [SerializeField] private Transform contentParent;

    [Header("업적 항목 하나의 프리팹 (AchievementSlotUI 컴포넌트 포함)")]
    [SerializeField] private GameObject achievementSlotPrefab;

    // 씬이 시작되는 순간 자동으로 목록을 채움 (팝업이 아니라 상시 노출이므로)
    private void Start()
    {
        RefreshAchievements();
    }

    // 전체 목록을 새로 채우는 함수 (외부에서 호출 가능, 예: 데이터 갱신 후 강제 새로고침)
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
    // 탭 버튼 3개에 각각 연결하는 필터링 함수
    // (파라미터 없는 함수라 인스펙터 OnClick() 드롭다운에서 정상 선택 가능)
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

    // 실제 필터링 로직 (내부 전용)
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
