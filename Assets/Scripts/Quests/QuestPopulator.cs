using System.Collections.Generic;
using UnityEngine;

// Fills the lobby's always-visible quest panel and handles the 진행도/달성/미달성 tabs.
// Not a PopupUI-driven panel, so it refreshes itself on Start() rather than on OnOpened.
public class QuestPopulator : MonoBehaviour
{
    [System.Serializable]
    private struct QuestIconEntry
    {
        public string questId;
        public Sprite icon;
    }

    [Header("업적 항목이 생성될 부모 오브젝트 (ScrollView > Viewport > Content)")]
    [SerializeField] private Transform contentParent;

    [Header("업적 항목 하나의 프리팹 (QuestSlotUI 컴포넌트 포함)")]
    [SerializeField] private GameObject questSlotPrefab;

    [Header("업적 id별 고유 아이콘")]
    [SerializeField] private List<QuestIconEntry> icons = new();

    private void Start()
    {
        RefreshQuests();
    }

    // QuestEventListener의 Response(파라미터 1개)로 연결하면 달성 즉시 목록이 갱신됨
    public void OnQuestUnlocked(QuestData data)
    {
        RefreshQuests();
    }

    public void RefreshQuests() => RefreshFiltered(false, false);

    // Tab_Progress(전체) 버튼의 OnClick()에 연결
    public void ShowAll() => RefreshFiltered(false, false);

    // Tab_Completed(달성) 버튼의 OnClick()에 연결
    public void ShowCompletedOnly() => RefreshFiltered(true, false);

    // Tab_Incomplete(미달성) 버튼의 OnClick()에 연결
    public void ShowIncompleteOnly() => RefreshFiltered(false, true);

    private void RefreshFiltered(bool onlyCompleted, bool onlyIncomplete)
    {
        if (GlobalManager.DataProvider == null)
        {
            Debug.LogWarning("[QuestPopulator] DataProvider가 아직 연결되지 않았습니다.");
            return;
        }

        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        List<QuestData> quests = GlobalManager.DataProvider.GetAllQuests();

        foreach (var data in quests)
        {
            if (onlyCompleted && !data.isCompleted) continue;
            if (onlyIncomplete && data.isCompleted) continue;

            GameObject slot = Instantiate(questSlotPrefab, contentParent);
            var slotUI = slot.GetComponent<QuestSlotUI>();
            if (slotUI != null)
            {
                slotUI.Setup(data, GetIcon(data.id));
            }
        }
    }

    private Sprite GetIcon(string questId)
    {
        foreach (var entry in icons)
            if (entry.questId == questId) return entry.icon;
        return null;
    }
}
