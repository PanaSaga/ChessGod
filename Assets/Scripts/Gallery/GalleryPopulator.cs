using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

// Fully dynamic gallery pagination -- no fixed Page_0~3 objects, itemsPerPage alone
// decides layout. Category tabs (스토리/오마케 등) call ShowCategory(string) directly from
// OnClick() with a literal string argument.
public class GalleryPopulator : MonoBehaviour
{
    [SerializeField] private Transform contentParent;
    [SerializeField] private GameObject gallerySlotPrefab;
    [SerializeField] private StoryManager storyManager;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private int itemsPerPage = 8;
    [SerializeField] private string defaultCategory = "story";

    private string currentCategory;
    private int currentPage;
    private List<GalleryItemData> allItems = new List<GalleryItemData>();

    private void Awake()
    {
        if (prevButton != null) prevButton.onClick.AddListener(PrevPage);
        if (nextButton != null) nextButton.onClick.AddListener(NextPage);
    }

    private void Start()
    {
        currentCategory = defaultCategory;
        RefreshGallery();
    }

    public void RefreshGallery()
    {
        if (GlobalManager.DataProvider == null)
        {
            Debug.LogWarning("[GalleryPopulator] DataProvider가 아직 연결되지 않았습니다.");
            return;
        }

        allItems = GlobalManager.DataProvider.GetUnlockedGalleryItems();
        currentPage = 0;
        RenderCurrentPage();
    }

    // 탭 버튼 OnClick()에 "story"/"omake" 문자열로 직접 연결
    public void ShowCategory(string category)
    {
        currentCategory = category;
        currentPage = 0;
        RenderCurrentPage();
    }

    private void NextPage()
    {
        currentPage++;
        RenderCurrentPage();
    }

    private void PrevPage()
    {
        currentPage--;
        RenderCurrentPage();
    }

    private void RenderCurrentPage()
    {
        List<GalleryItemData> categoryItems = allItems
            .Where(item => item.category == currentCategory)
            .OrderBy(item => item.sortOrder)
            .ToList();

        int totalPages = Mathf.Max(1, Mathf.CeilToInt((float)categoryItems.Count / itemsPerPage));
        currentPage = Mathf.Clamp(currentPage, 0, totalPages - 1);

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        int start = currentPage * itemsPerPage;
        int end = Mathf.Min(start + itemsPerPage, categoryItems.Count);

        for (int i = start; i < end; i++)
        {
            GameObject slot = Instantiate(gallerySlotPrefab, contentParent);
            var slotUI = slot.GetComponent<GallerySlotUI>();
            if (slotUI != null)
            {
                slotUI.Init(storyManager);
                slotUI.Setup(categoryItems[i]);
            }
        }

        if (prevButton != null) prevButton.interactable = currentPage > 0;
        if (nextButton != null) nextButton.interactable = currentPage < totalPages - 1;
    }
}
