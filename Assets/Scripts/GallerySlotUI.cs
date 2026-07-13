using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 갤러리 슬롯 프리팹 자체에 부착합니다.
/// GalleryPopulator가 이 함수를 호출해 데이터를 화면에 표시합니다.
/// </summary>
public class GallerySlotUI : MonoBehaviour
{
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private GameObject lockedOverlay; // 미해금 표시용 (선택사항)

    public void Setup(GalleryItemData data)
    {
        // TODO(UI팀): data.imagePath를 실제 스프라이트로 불러오는 로직
        // 예: thumbnailImage.sprite = Resources.Load<Sprite>(data.imagePath);

        if (lockedOverlay != null)
        {
            lockedOverlay.SetActive(!data.isUnlocked);
        }
    }
}
