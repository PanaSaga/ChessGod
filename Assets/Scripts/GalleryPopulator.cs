using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 갤러리 팝업이 "열릴 때마다" IGameDataProvider로부터 최신 데이터를 받아 화면을 갱신합니다.
/// PopupUI는 건드리지 않고, PopupUI가 보내는 OnOpened 이벤트만 받아서 동작합니다.
///
/// 붙이는 위치: GalleryPopup(또는 PhotoDetailPopup) 오브젝트 (PopupUI와 같은 오브젝트)
///
/// [연결 방법 - 유니티 에디터]
/// 1. PopupUI 컴포넌트의 On Opened() 리스트에 + 클릭
/// 2. 오브젝트 슬롯에 자기 자신 드래그
/// 3. 함수 선택 → GalleryPopulator > RefreshGallery()
///
/// [게임팀 작업물 연결 시 필요한 작업]
/// 없음. GlobalManager.DataProvider 자리에 실제 구현체만 꽂히면 이 스크립트는 그대로 작동합니다.
/// 단, GallerySlotUI.Setup(GalleryItemData) 함수는 UI팀이 슬롯 프리팹에 맞게 미리 작성해둬야 합니다.
/// </summary>
public class GalleryPopulator : MonoBehaviour
{
    [Header("갤러리 슬롯이 생성될 부모 오브젝트 (그리드 레이아웃 등)")]
    [SerializeField] private Transform contentParent;

    [Header("슬롯 하나의 프리팹 (GallerySlotUI 컴포넌트 포함)")]
    [SerializeField] private GameObject gallerySlotPrefab;

    // PopupUI.OnOpened 이벤트에 연결하는 함수
    public void RefreshGallery()
    {
        if (GlobalManager.DataProvider == null)
        {
            Debug.LogWarning("[GalleryPopulator] DataProvider가 아직 연결되지 않았습니다. " +
                              "GlobalManager 하위에 IGameDataProvider 구현체(Stub 또는 실제 매니저)가 있는지 확인하세요.");
            return;
        }

        // 기존에 그려져 있던 슬롯들을 전부 지운다
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 계약(IGameDataProvider)을 통해 데이터를 받아온다 - 실제 구현이 Stub이든 진짜 매니저든 코드는 동일
        List<GalleryItemData> items = GlobalManager.DataProvider.GetUnlockedGalleryItems();

        foreach (var item in items)
        {
            GameObject slot = Instantiate(gallerySlotPrefab, contentParent);

            // 슬롯 프리팹에 GallerySlotUI 스크립트가 있다면 데이터를 넘겨줍니다.
            // (GallerySlotUI는 UI팀이 슬롯 디자인에 맞춰 별도로 작성)
            var slotUI = slot.GetComponent<GallerySlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(item);
            }
        }
    }
}
