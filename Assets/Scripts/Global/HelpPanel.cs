using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HelpPanel : MonoBehaviour
{
    [Serializable]
    private class HelpPage
    {
        public Sprite image;
        [TextArea] public string description;
    }

    [Serializable]
    private class HelpTopic
    {
        public string title;
        public List<HelpPage> pages = new();
    }

    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button openButton;
    [SerializeField] private Button closeButton;

    [Header("Topic list")]
    [SerializeField] private List<HelpTopic> topics = new();
    [SerializeField] private Transform topicListParent;
    [SerializeField] private Button topicButtonPrefab;

    [Header("Page content")]
    [SerializeField] private Image pageImage;
    [SerializeField] private TMP_Text pageDescriptionText;
    [SerializeField] private TMP_Text pageIndicatorText;
    [SerializeField] private Button previousPageButton;
    [SerializeField] private Button nextPageButton;

    private HelpTopic currentTopic;
    private int currentPageIndex;

    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private void Start()
    {
        if (openButton != null) openButton.onClick.AddListener(OpenHelp);
        if (closeButton != null) closeButton.onClick.AddListener(CloseHelp);
        if (previousPageButton != null) previousPageButton.onClick.AddListener(ShowPreviousPage);
        if (nextPageButton != null) nextPageButton.onClick.AddListener(ShowNextPage);

        BuildTopicList();
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    // Spawns one button per topic so adding a new topic in the Inspector list is all that's needed.
    private void BuildTopicList()
    {
        if (topicListParent == null || topicButtonPrefab == null) return;

        foreach (HelpTopic topic in topics)
        {
            Button button = Instantiate(topicButtonPrefab, topicListParent);
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) label.text = topic.title;
            button.onClick.AddListener(() => SelectTopic(topic));
        }
    }

    private void SelectTopic(HelpTopic topic)
    {
        currentTopic = topic;
        currentPageIndex = 0;
        ShowCurrentPage();
    }

    private void ShowPreviousPage()
    {
        if (currentTopic == null || currentPageIndex <= 0) return;
        currentPageIndex--;
        ShowCurrentPage();
    }

    private void ShowNextPage()
    {
        if (currentTopic == null || currentPageIndex >= currentTopic.pages.Count - 1) return;
        currentPageIndex++;
        ShowCurrentPage();
    }

    private void ShowCurrentPage()
    {
        if (currentTopic == null || currentTopic.pages.Count == 0) return;
        HelpPage page = currentTopic.pages[currentPageIndex];

        if (pageImage != null)
        {
            pageImage.sprite = page.image;
            pageImage.enabled = page.image != null;
        }
        if (pageDescriptionText != null) pageDescriptionText.text = page.description;
        if (pageIndicatorText != null) pageIndicatorText.text = $"{currentPageIndex + 1} / {currentTopic.pages.Count}";

        if (previousPageButton != null) previousPageButton.interactable = currentPageIndex > 0;
        if (nextPageButton != null) nextPageButton.interactable = currentPageIndex < currentTopic.pages.Count - 1;
    }

    public void OpenHelp()
    {
        if (panelRoot != null) panelRoot.SetActive(true);
        if (topics.Count > 0) SelectTopic(topics[0]);
    }

    public void CloseHelp()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }
}
