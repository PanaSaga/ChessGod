using System;

// Device-level settings, separate from the 3 save slots (changing slots should not reset volume).
[Serializable]
public class GlobalSettingsData
{
    public float bgmVolume = 1f;
    public bool isBgmMuted;
    public float sfxVolume = 1f;
    public bool isSfxMuted;

    // -1 means no slot has ever been created/loaded yet (used by the start screen's "이어하기" button).
    public int lastUsedSlotIndex = -1;

    public int resolutionWidth = 1920;
    public int resolutionHeight = 1080;
}
