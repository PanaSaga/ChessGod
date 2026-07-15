using System.Collections.Generic;

// UI-side persistence/data contract. UI scripts only ever call through this interface.
public interface IGameDataProvider
{
    // Reads
    List<GalleryItemData> GetUnlockedGalleryItems();
    List<QuestData> GetAllQuests();
    SettingsData GetSettingsData();
    bool HasSeenPrologue();
    List<StoryLineData> GetStoryLines(string storyId);

    // Writes
    void SaveQuestProgress(string questId, int progress, bool isCompleted);
    void SaveSettingsData(SettingsData data);
    void SetPrologueSeen();
    void ResetProgress();
}

[System.Serializable]
public class GalleryItemData
{
    public string id;
    public string category;
    public int sortOrder;
    public string title;
    public string imagePath;
    public string description;
    public bool isUnlocked;
    public string storyId;
}

[System.Serializable]
public class QuestData
{
    public string id;
    public string title;
    public string condition;
    public int currentProgress;
    public int targetProgress;
    public bool isCompleted;
}

[System.Serializable]
public class StoryLineData
{
    public string characterName;
    public string dialogueText;
    public string backgroundPath;
    public string standingPath;
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
