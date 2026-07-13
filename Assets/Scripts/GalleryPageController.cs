using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 갤러리 페이지를 5개씩 묶어 전환하는 스크립트.
/// Page_0 ~ Page_3 오브젝트를 인스펙터에 순서대로 등록해두면
/// Prev/Next 버튼으로 하나씩 켜고 끄며 전환합니다.
///
/// 붙이는 위치: PageArea 오브젝트 (GalleryScene)
/// </summary>
public class GalleryPageController : MonoBehaviour
{
    [Header("페이지 순서대로 등록 (Page_0, Page_1, Page_2, Page_3)")]
    [SerializeField] private GameObject[] pages;

    [Header("페이지 전환 버튼")]
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;

    private int currentPage = 0;

    private void Awake()
    {
        prevButton.onClick.AddListener(GoPrevPage);
        nextButton.onClick.AddListener(GoNextPage);
    }

    private void Start()
    {
        ShowPage(0);
    }

    private void ShowPage(int index)
    {
        for (int i = 0; i < pages.Length; i++)
        {
            pages[i].SetActive(i == index);
        }

        currentPage = index;

        // 양 끝에서는 버튼을 비활성화 (선택사항, 없어도 작동함)
        prevButton.interactable = currentPage > 0;
        nextButton.interactable = currentPage < pages.Length - 1;
    }

    private void GoPrevPage()
    {
        if (currentPage > 0) ShowPage(currentPage - 1);
    }

    private void GoNextPage()
    {
        if (currentPage < pages.Length - 1) ShowPage(currentPage + 1);
    }
}
