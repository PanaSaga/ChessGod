using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// 설정 데이터(PlayerPrefs)와 업적/해금 데이터(JSON 파일)를 저장/불러오기 하는 매니저.
/// 붙이는 위치: GlobalManager 하위의 "DataManager" 오브젝트
///
/// 저장 방식 구분 (검색한 최신 자료 기준):
/// - 단순 설정값(볼륨 등) → PlayerPrefs
/// - 구조화된 데이터(업적 진행도, 해금 목록) → JSON 파일 (Application.persistentDataPath)
/// - 향후 콘텐츠 추가 시 기존 세이브가 깨지지 않도록 saveVersion 필드 포함
/// </summary>
public class DataManager : MonoBehaviour
{
    private const int CURRENT_SAVE_VERSION = 1;
    private string SaveFilePath => Path.Combine(Application.persistentDataPath, "save_data.json");

    // 업적/토스트 매니저가 동시에 데이터를 건드리지 않도록 하는 잠금 플래그 (개정안 6-2 반영)
    public bool IsUpdating { get; private set; }

    [Serializable]
    public class SaveData
    {
        public int saveVersion = CURRENT_SAVE_VERSION;
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
        if (File.Exists(SaveFilePath))
        {
            string json = File.ReadAllText(SaveFilePath);
            currentSave = JsonUtility.FromJson<SaveData>(json);

            // 버전이 다르면 여기서 마이그레이션 로직을 추가 (지금은 최소 구현)
            if (currentSave.saveVersion != CURRENT_SAVE_VERSION)
            {
                Debug.LogWarning($"[DataManager] 세이브 버전 불일치 (파일: {currentSave.saveVersion}, 현재: {CURRENT_SAVE_VERSION}). 마이그레이션 로직 필요.");
            }
        }
        else
        {
            currentSave = new SaveData();
            Save();
        }

        Debug.Log($"[DataManager] 세이브 파일 경로: {SaveFilePath}");
    }

    public void Save()
    {
        IsUpdating = true;
        string json = JsonUtility.ToJson(currentSave, true);
        File.WriteAllText(SaveFilePath, json);
        IsUpdating = false;
    }

    // ---- 업적 진행도 ----
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

    // ---- 해금 데이터 ----
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

    // ---- 설정 데이터 (PlayerPrefs) ----
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

    public bool HasSeenPrologue() => PlayerPrefs.GetInt("seen_prologue", 0) == 1;
    public void SetPrologueSeen()
    {
        PlayerPrefs.SetInt("seen_prologue", 1);
        PlayerPrefs.Save();
    }
}
