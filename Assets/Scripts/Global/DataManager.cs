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

    // Wipes the given slot to a fresh profile and makes it the active slot. Used by "새로 시작하기".
    public void CreateNewSlot(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        currentSlot = new SaveSlotData();
        SaveCurrentSlot();
    }

    // Reads the given slot from disk and makes it the active slot. Used by "불러오기".
    public bool LoadSlot(int slotIndex)
    {
        string path = GetSlotFilePath(slotIndex);
        if (!File.Exists(path)) return false;

        currentSlot = JsonUtility.FromJson<SaveSlotData>(File.ReadAllText(path));
        currentSlotIndex = slotIndex;
        return true;
    }

    // Writes the active slot's current state to disk. Called manually (pause popup) and
    // automatically (right after a new achievement unlocks).
    public void SaveCurrentSlot()
    {
        if (currentSlotIndex < 0 || currentSlot == null) return;
        File.WriteAllText(GetSlotFilePath(currentSlotIndex), JsonUtility.ToJson(currentSlot));
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
    }

    private string GetGlobalSettingsFilePath() => Path.Combine(Application.persistentDataPath, "settings.json");
}
