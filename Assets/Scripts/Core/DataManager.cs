using System;
using System.Collections.Generic;
using UnityEngine;

// Persists settings (PlayerPrefs scalars) and structured progress data
// (achievements, gallery unlocks) as a single JSON blob stored through
// PlayerPrefs rather than File I/O.
//
// WebGL note: Application.persistentDataPath is backed by an in-memory
// IndexedDB filesystem (IDBFS) that Unity does not always flush to disk
// automatically, so File.WriteAllText can silently lose data after a page
// reload. PlayerPrefs is backed directly by IndexedDB on WebGL and Unity
// keeps it in sync, so the structured save data is serialized to JSON and
// stored under a single PlayerPrefs key instead of a real file.
public class DataManager : MonoBehaviour
{
    private const int CurrentSaveVersion = 1;
    private const string SaveDataKey = "save_data_json";

    public bool IsUpdating { get; private set; }

    [Serializable]
    public class SaveData
    {
        public int saveVersion = CurrentSaveVersion;
        public List<string> unlockedGalleryIds = new List<string>();
        public List<AchievementSaveEntry> achievementProgress = new List<AchievementSaveEntry>();
    }

    [Serializable]
    public class AchievementSaveEntry
    {
        public string id;
        public int progress;
        public bool isCompleted;
    }

    private SaveData currentSave;

    private void Awake()
    {
        LoadOrCreate();
    }

    private void LoadOrCreate()
    {
        if (PlayerPrefs.HasKey(SaveDataKey))
        {
            string json = PlayerPrefs.GetString(SaveDataKey);
            currentSave = JsonUtility.FromJson<SaveData>(json);

            if (currentSave.saveVersion != CurrentSaveVersion)
            {
                Debug.LogWarning($"[DataManager] 세이브 버전 불일치 (저장값: {currentSave.saveVersion}, 현재: {CurrentSaveVersion}). 마이그레이션 로직 필요.");
            }
        }
        else
        {
            currentSave = new SaveData();
            Save();
        }
    }

    public void Save()
    {
        IsUpdating = true;
        string json = JsonUtility.ToJson(currentSave);
        PlayerPrefs.SetString(SaveDataKey, json);
        PlayerPrefs.Save();
        IsUpdating = false;
    }

    // ---- Achievement progress ----
    public void SaveAchievementProgress(string achievementId, int progress, bool isCompleted)
    {
        var entry = currentSave.achievementProgress.Find(e => e.id == achievementId);
        if (entry == null)
        {
            entry = new AchievementSaveEntry { id = achievementId };
            currentSave.achievementProgress.Add(entry);
        }
        entry.progress = progress;
        entry.isCompleted = isCompleted;
        Save();
    }

    public List<AchievementSaveEntry> GetAchievementProgress()
    {
        return currentSave.achievementProgress;
    }

    // ---- Gallery unlocks ----
    public void UnlockGalleryItem(string galleryId)
    {
        if (!currentSave.unlockedGalleryIds.Contains(galleryId))
        {
            currentSave.unlockedGalleryIds.Add(galleryId);
            Save();
        }
    }

    public bool IsGalleryItemUnlocked(string galleryId)
    {
        return currentSave.unlockedGalleryIds.Contains(galleryId);
    }

    // ---- Settings (PlayerPrefs scalars) ----
    public float GetBgmVolume() => PlayerPrefs.GetFloat("bgm_volume", 1f);
    public void SetBgmVolume(float value)
    {
        PlayerPrefs.SetFloat("bgm_volume", value);
        PlayerPrefs.Save();
    }

    public float GetSfxVolume() => PlayerPrefs.GetFloat("sfx_volume", 1f);
    public void SetSfxVolume(float value)
    {
        PlayerPrefs.SetFloat("sfx_volume", value);
        PlayerPrefs.Save();
    }

    public bool GetBgmMuted() => PlayerPrefs.GetInt("bgm_muted", 0) == 1;
    public void SetBgmMuted(bool muted)
    {
        PlayerPrefs.SetInt("bgm_muted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool GetSfxMuted() => PlayerPrefs.GetInt("sfx_muted", 0) == 1;
    public void SetSfxMuted(bool muted)
    {
        PlayerPrefs.SetInt("sfx_muted", muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public bool HasSeenPrologue() => PlayerPrefs.GetInt("seen_prologue", 0) == 1;
    public void SetPrologueSeen()
    {
        PlayerPrefs.SetInt("seen_prologue", 1);
        PlayerPrefs.Save();
    }

    // ---- Reset ("처음부터") ----
    // Volume/mute settings are left untouched; only progress data resets.
    public void ResetAllData()
    {
        currentSave = new SaveData();
        Save();

        PlayerPrefs.SetInt("seen_prologue", 0);
        PlayerPrefs.Save();
    }
}
