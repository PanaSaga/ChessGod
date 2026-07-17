using System;

// Device-level settings, separate from the 3 save slots (changing slots should not reset volume).
[Serializable]
public class GlobalSettingsData
{
    public float bgmVolume = 1f;
    public bool isBgmMuted;
    public float sfxVolume = 1f;
    public bool isSfxMuted;
}
