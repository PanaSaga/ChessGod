using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// Real IGameDataProvider implementation for this project (there is no
// separate "game team" stub/real split anymore: UI and ingame share one
// codebase, this is just the one connector). Quest definitions
// (title/condition/target) are the confirmed content from the design doc;
// current progress/completion is merged in from DataManager at read time.
[RequireComponent(typeof(DataManager))]
public class GameDataProvider : MonoBehaviour, IGameDataProvider
{
    private static readonly List<QuestData> QuestDefinitions = new List<QuestData>
    {
        new QuestData { id = "ach_checkmate", title = "체크메이트", condition = "흑의 킹 제거 누적 10회", targetProgress = 10 },
        new QuestData { id = "ach_fork", title = "포크", condition = "한 번의 공격으로 체스말 2개 이상 잡기 누적 10회", targetProgress = 10 },
        new QuestData { id = "ach_upset", title = "하극상", condition = "나이트로 퀸 처치 누적 10회", targetProgress = 10 },
        new QuestData { id = "ach_brilliant", title = "탁월수", condition = "킹이 아닌 상태로 적의 퀸과 동귀어진 누적 5회", targetProgress = 5 },
        new QuestData { id = "ach_stalemate", title = "스테일 메이트", condition = "보드 위에 흑의 체스말 10개 이상일 때 생존 누적 3회", targetProgress = 3 },
        new QuestData { id = "ach_genocide", title = "제노사이드", condition = "2스테이지 이후 보드 위의 체스말을 전부 제거 누적 3회", targetProgress = 3 },
        new QuestData { id = "ach_rating_master", title = "레이팅 마스터", condition = "점수 2000점 이상 달성", targetProgress = 2000 },
    };

    private DataManager dataManager;

    private void Awake()
    {
        dataManager = GetComponent<DataManager>();
        GlobalManager.DataProvider = this;
    }

    public List<QuestData> GetAllQuests()
    {
        List<DataManager.QuestSaveEntry> saved = dataManager.GetQuestProgress();

        return QuestDefinitions.Select(def =>
        {
            DataManager.QuestSaveEntry entry = saved.Find(e => e.id == def.id);
            return new QuestData
            {
                id = def.id,
                title = def.title,
                condition = def.condition,
                targetProgress = def.targetProgress,
                currentProgress = entry?.progress ?? 0,
                isCompleted = entry?.isCompleted ?? false
            };
        }).ToList();
    }

    public void SaveQuestProgress(string questId, int progress, bool isCompleted)
    {
        dataManager.SaveQuestProgress(questId, progress, isCompleted);
    }

    // TODO(갤러리 콘텐츠 확정 후): 실제 GalleryItemData 목록으로 교체. 지금은 갤러리 화면을
    // 만들지 않는 단계라 빈 목록을 반환.
    public List<GalleryItemData> GetUnlockedGalleryItems()
    {
        return new List<GalleryItemData>();
    }

    // TODO(스토리 콘텐츠 확정 후): storyId별 실제 대사 목록으로 교체.
    public List<StoryLineData> GetStoryLines(string storyId)
    {
        return new List<StoryLineData>();
    }

    public SettingsData GetSettingsData()
    {
        return new SettingsData
        {
            bgmVolume = dataManager.GetBgmVolume(),
            sfxVolume = dataManager.GetSfxVolume(),
            bgmMuted = dataManager.GetBgmMuted(),
            sfxMuted = dataManager.GetSfxMuted()
        };
    }

    public void SaveSettingsData(SettingsData data)
    {
        dataManager.SetBgmVolume(data.bgmVolume);
        dataManager.SetSfxVolume(data.sfxVolume);
        dataManager.SetBgmMuted(data.bgmMuted);
        dataManager.SetSfxMuted(data.sfxMuted);
    }

    public bool HasSeenPrologue() => dataManager.HasSeenPrologue();
    public void SetPrologueSeen() => dataManager.SetPrologueSeen();

    public void ResetProgress() => dataManager.ResetAllData();
}
