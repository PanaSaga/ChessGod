using UnityEngine;

/// <summary>
/// 업적(Achievement) 팝업이 "열릴 때마다" 최신 진행 상황으로 내용을 갱신하는 스크립트입니다.
/// PopupUI는 절대 건드리지 않고, PopupUI가 보내는 OnOpened 이벤트만 받아서 동작합니다.
///
/// 붙이는 위치: AchievementPopup 오브젝트 (PopupUI와 같은 오브젝트에 함께 부착)
///
/// [연결 방법 - 유니티 에디터]
/// 1. AchievementPopup의 PopupUI 컴포넌트에서 On Opened() 리스트에 + 클릭
/// 2. 오브젝트 슬롯에 AchievementPopup(자기 자신) 드래그
/// 3. 함수 선택 → AchievementPopulator > RefreshAchievements()
///
/// [나중에 게임 데이터를 연결할 때]
/// RefreshAchievements() 함수 안의 TODO 부분만 채우면 됩니다.
/// 예: 업적 달성 여부, 진행도(%), 잠금 아이콘 표시 등.
/// </summary>
public class AchievementPopulator : MonoBehaviour
{
    [Header("업적 항목이 생성될 부모 오브젝트 (리스트 레이아웃 등)")]
    [SerializeField] private Transform contentParent;

    [Header("업적 항목 하나의 프리팹")]
    [SerializeField] private GameObject achievementSlotPrefab;

    // PopupUI.OnOpened 이벤트에 연결하는 함수
    public void RefreshAchievements()
    {
        // ---------------------------------------------------------
        // TODO: 여기에 실제 업적 데이터를 불러와서 화면에 그리는 로직을 넣습니다.
        //
        // 예시 흐름 (실제 데이터 구조가 정해지면 이 부분만 교체):
        //
        // 1) 기존에 그려져 있던 항목들을 전부 지운다
        //    foreach (Transform child in contentParent) Destroy(child.gameObject);
        //
        // 2) 세이브/게임매니저에서 "전체 업적 목록 + 달성 여부/진행도"를 가져온다
        //    List<AchievementData> achievements = GameData.Instance.GetAllAchievements();
        //
        // 3) 목록을 순회하며 슬롯 프리팹을 생성하고 정보를 채운다
        //    foreach (var data in achievements)
        //    {
        //        GameObject slot = Instantiate(achievementSlotPrefab, contentParent);
        //        slot.GetComponent<AchievementSlotUI>().Setup(data);
        //    }
        // ---------------------------------------------------------

        Debug.Log("[AchievementPopulator] 업적 갱신 자리 - 아직 데이터 연결 전입니다.");
    }
}
