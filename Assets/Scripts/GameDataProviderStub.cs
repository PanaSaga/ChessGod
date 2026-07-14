using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// [UI팀 전용 - 테스트용 가짜 데이터]
/// 게임팀의 실제 데이터 매니저(AchievementDataManager 등)가 준비되기 전,
/// UI팀이 화면 동작을 미리 테스트할 수 있도록 IGameDataProvider를 임시로 구현합니다.
///
/// 붙이는 위치: GlobalManager 하위 "DataProviderSlot" 오브젝트
/// 게임팀 작업물이 준비되면 이 컴포넌트를 실제 구현체로 교체하기만 하면 됩니다.
/// </summary>
public class GameDataProviderStub : MonoBehaviour, IGameDataProvider
{
    private bool prologueSeen = false;

    private void Awake()
    {
        GlobalManager.DataProvider = this;
    }

    public List<GalleryItemData> GetUnlockedGalleryItems()
    {
        var list = new List<GalleryItemData>();
        for (int i = 0; i < 20; i++)
        {
            list.Add(new GalleryItemData
            {
                id = $"gallery_{i:00}",
                imagePath = "",
                description = $"테스트용 사진 설명 {i + 1}",
                isUnlocked = i % 2 == 0
            });
        }
        return list;
    }

    public List<AchievementData> GetAllAchievements()
    {
        var list = new List<AchievementData>();
        for (int i = 0; i < 8; i++)
        {
            list.Add(new AchievementData
            {
                id = $"ach_{i:00}",
                title = $"테스트 업적 {i + 1}",
                condition = "테스트용 달성 조건",
                currentProgress = i,
                targetProgress = 10,
                isCompleted = i >= 5
            });
        }
        return list;
    }

    public SettingsData GetSettingsData()
    {
        return new SettingsData();
    }

    public bool HasSeenPrologue() => prologueSeen;

    public void SaveAchievementProgress(string achievementId, int progress, bool isCompleted)
    {
        Debug.Log($"[GameDataProviderStub] 업적 저장 요청(더미): {achievementId} {progress} {isCompleted}");
    }

    public void SaveSettingsData(SettingsData data)
    {
        Debug.Log("[GameDataProviderStub] 설정 저장 요청(더미)");
    }

    public void SetPrologueSeen()
    {
        prologueSeen = true;
        Debug.Log("[GameDataProviderStub] 프롤로그 시청 처리(더미)");
    }
}
