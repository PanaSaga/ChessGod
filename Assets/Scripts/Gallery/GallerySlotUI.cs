using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One gallery thumbnail. Locked: title shows "??? ", click ignored. Unlocked: "01. 제목",
// click plays the cutscene via StoryManager.
[RequireComponent(typeof(Button))]
public class GallerySlotUI : MonoBehaviour
{
    [SerializeField] private Image thumbnailImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private GameObject lockedOverlay;

    [SerializeField] private Color lockedTintColor = new Color(0.25f, 0.25f, 0.25f, 1f);
    private readonly Color unlockedTintColor = Color.white;

    private StoryManager storyManager;
    private GalleryItemData currentData;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnSlotClicked);
    }

    public void Init(StoryManager manager)
    {
        storyManager = manager;
    }

    public void Setup(GalleryItemData data)
    {
        currentData = data;

        if (thumbnailImage != null && !string.IsNullOrEmpty(data.imagePath))
        {
            Sprite thumb = Resources.Load<Sprite>(data.imagePath);
            if (thumb != null) thumbnailImage.sprite = thumb;
        }

        if (thumbnailImage != null)
            thumbnailImage.color = data.isUnlocked ? unlockedTintColor : lockedTintColor;

        if (lockedOverlay != null)
            lockedOverlay.SetActive(!data.isUnlocked);

        if (titleText != null)
        {
            string numberLabel = (data.sortOrder + 1).ToString("00");
            titleText.text = data.isUnlocked ? $"{numberLabel}. {data.title}" : $"{numberLabel}. ???";
        }
    }

    private void OnSlotClicked()
    {
        if (currentData == null || !currentData.isUnlocked) return;

        if (storyManager == null)
        {
            Debug.LogWarning("[GallerySlotUI] StoryManager가 연결되지 않았습니다.");
            return;
        }

        storyManager.PlayStoryById(currentData.storyId);
    }
}
