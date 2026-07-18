using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour
{
    public const int SlotCount = 3;

    private SaveSlotData currentSlot;
    private int currentSlotIndex = -1;

    public GlobalSettingsData GlobalSettings { get; private set; }

    public SaveSlotData CurrentSlot => currentSlot;
    public int CurrentSlotIndex => currentSlotIndex;

    private void Awake()
    {
        LoadGlobalSettings();
    }

    // ----- Save slots -----

    public bool HasSaveData(int slotIndex) => File.Exists(GetSlotFilePath(slotIndex));

    // Reads a slot from disk without making it the active slot - for preview UI (start screen slot list).
    public SaveSlotData PeekSlot(int slotIndex)
    {
        string path = GetSlotFilePath(slotIndex);
        return File.Exists(path) ? JsonUtility.FromJson<SaveSlotData>(File.ReadAllText(path)) : null;
    }

    // Wipes the given slot to a fresh profile and makes it the active slot. Used by "새로 시작하기".
    public void CreateNewSlot(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        currentSlot = new SaveSlotData();
        SaveCurrentSlot();
        RememberLastUsedSlot(slotIndex);
    }

    // Reads the given slot from disk and makes it the active slot. Used by "이어하기"/"불러오기".
    public bool LoadSlot(int slotIndex)
    {
        string path = GetSlotFilePath(slotIndex);
        if (!File.Exists(path)) return false;

        currentSlot = JsonUtility.FromJson<SaveSlotData>(File.ReadAllText(path));
        currentSlotIndex = slotIndex;
        RememberLastUsedSlot(slotIndex);
        return true;
    }

    private void RememberLastUsedSlot(int slotIndex)
    {
        GlobalSettings.lastUsedSlotIndex = slotIndex;
        SaveGlobalSettings();
    }

    // Writes the active slot's current state to disk. Called manually (pause popup) and
    // automatically (right after a new achievement unlocks).
    public void SaveCurrentSlot()
    {
        if (currentSlotIndex < 0 || currentSlot == null) return;
        File.WriteAllText(GetSlotFilePath(currentSlotIndex), JsonUtility.ToJson(currentSlot));
        WebGLStorageSync.Sync();
    }

    private string GetSlotFilePath(int slotIndex) => Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");

    // ----- Achievement / story / gallery unlock helpers (ID-based, content-agnostic) -----

    public bool IsAchievementUnlocked(string achievementId) => currentSlot != null && currentSlot.unlockedAchievementIds.Contains(achievementId);

    public void UnlockAchievement(string achievementId)
    {
        if (currentSlot == null || IsAchievementUnlocked(achievementId)) return;
        currentSlot.unlockedAchievementIds.Add(achievementId);
        SaveCurrentSlot();
    }

    public int GetAchievementProgress(string achievementId)
    {
        if (currentSlot == null) return 0;
        AchievementProgressEntry entry = currentSlot.achievementProgress.Find(e => e.achievementId == achievementId);
        return entry?.count ?? 0;
    }

    public void SetAchievementProgress(string achievementId, int count)
    {
        if (currentSlot == null) return;
        AchievementProgressEntry entry = currentSlot.achievementProgress.Find(e => e.achievementId == achievementId);
        if (entry == null)
            currentSlot.achievementProgress.Add(new AchievementProgressEntry { achievementId = achievementId, count = count });
        else
            entry.count = count;
        SaveCurrentSlot();
    }

    public bool IsAchievementAcknowledged(string achievementId) => currentSlot != null && currentSlot.acknowledgedAchievementIds.Contains(achievementId);

    public void AcknowledgeAchievement(string achievementId)
    {
        if (currentSlot == null || IsAchievementAcknowledged(achievementId)) return;
        currentSlot.acknowledgedAchievementIds.Add(achievementId);
        SaveCurrentSlot();
    }

    public bool IsStoryUnlocked(string storyId) => currentSlot != null && currentSlot.unlockedStoryIds.Contains(storyId);

    public void UnlockStory(string storyId)
    {
        if (currentSlot == null || IsStoryUnlocked(storyId)) return;
        currentSlot.unlockedStoryIds.Add(storyId);
        SaveCurrentSlot();
    }

    public bool IsStoryViewed(string storyId) => currentSlot != null && currentSlot.viewedStoryIds.Contains(storyId);

    public void MarkStoryViewed(string storyId)
    {
        if (currentSlot == null || IsStoryViewed(storyId)) return;
        currentSlot.viewedStoryIds.Add(storyId);
        SaveCurrentSlot();
    }

    public bool IsGalleryItemUnlocked(string galleryId) => currentSlot != null && currentSlot.unlockedGalleryIds.Contains(galleryId);

    public void UnlockGalleryItem(string galleryId)
    {
        if (currentSlot == null || IsGalleryItemUnlocked(galleryId)) return;
        currentSlot.unlockedGalleryIds.Add(galleryId);
        SaveCurrentSlot();
    }

    // ----- Global settings (volume) -----

    private void LoadGlobalSettings()
    {
        string path = GetGlobalSettingsFilePath();
        GlobalSettings = File.Exists(path)
            ? JsonUtility.FromJson<GlobalSettingsData>(File.ReadAllText(path))
            : new GlobalSettingsData();
    }

    public void SaveGlobalSettings()
    {
        File.WriteAllText(GetGlobalSettingsFilePath(), JsonUtility.ToJson(GlobalSettings));
        WebGLStorageSync.Sync();
    }

    private string GetGlobalSettingsFilePath() => Path.Combine(Application.persistentDataPath, "settings.json");
}
