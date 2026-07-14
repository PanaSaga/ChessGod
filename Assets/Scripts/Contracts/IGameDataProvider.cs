using System.Collections.Generic;

/// <summary>
/// [협업 계약 파일 - 절대 혼자 수정 금지, 반드시 UI팀/게임팀 합의 후 변경]
///
/// UI팀은 이 인터페이스에 정의된 함수만 호출하도록 스크립트를 작성합니다.
/// 게임팀은 이 인터페이스를 실제로 구현한 매니저 클래스를 만듭니다.
///
/// [개정 사항] 기존에는 "읽기" 함수만 있었으나, 업적 진행도 판단은 게임팀
/// (AchievementDataManager)이 담당하고 결과를 저장하는 "쓰기" 함수를 추가했습니다.
/// UI팀은 판단 로직을 갖지 않고, 결과만 읽거나 저장 요청만 전달합니다.
/// </summary>
public interface IGameDataProvider
{
    // ---- 읽기 ----
    List<GalleryItemData> GetUnlockedGalleryItems();
    List<AchievementData> GetAllAchievements();
    SettingsData GetSettingsData();
    bool HasSeenPrologue();

    // ---- 쓰기 ----
    void SaveAchievementProgress(string achievementId, int progress, bool isCompleted);
    void SaveSettingsData(SettingsData data);
    void SetPrologueSeen();
}

[System.Serializable]
public class GalleryItemData
{
    public string id;
    public string imagePath;
    public string description;
    public bool isUnlocked;
}

[System.Serializable]
public class AchievementData
{
    public string id;
    public string title;
    public string condition;
    public int currentProgress;
    public int targetProgress;
    public bool isCompleted;
}

[System.Serializable]
public class SettingsData
{
    public int saveVersion = 1;
    public float bgmVolume = 1f;
    public float sfxVolume = 1f;
    public bool bgmMuted = false;
    public bool sfxMuted = false;
}
