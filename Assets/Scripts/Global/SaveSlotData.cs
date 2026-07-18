using System;
using System.Collections.Generic;

[Serializable]
public class AchievementProgressEntry
{
    public string achievementId;
    public int count;
}

// One save slot's worth of progress. Identified only by string IDs so this class never needs to
// change when the actual achievement/story/gallery lists are designed later.
[Serializable]
public class SaveSlotData
{
    public bool prologueSeen;
    public bool tutorialCompleted;

    public List<string> unlockedAchievementIds = new();
    public List<AchievementProgressEntry> achievementProgress = new();
    public List<string> acknowledgedAchievementIds = new();
    public List<string> unlockedStoryIds = new();
    public List<string> viewedStoryIds = new();
    public List<string> unlockedGalleryIds = new();
}
