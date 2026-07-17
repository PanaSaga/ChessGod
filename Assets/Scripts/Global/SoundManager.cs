using System.Collections;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [Header("Audio sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("BGM fade")]
    [SerializeField, Min(0f)] private float bgmFadeDuration = 0.6f;

    private DataManager dataManager;
    private Coroutine bgmFadeRoutine;

    private void Start()
    {
        dataManager = GlobalManager.Instance != null ? GlobalManager.Instance.DataManager : null;
        ApplySavedVolume();
    }

    private void ApplySavedVolume()
    {
        GlobalSettingsData settings = dataManager != null ? dataManager.GlobalSettings : null;
        if (settings == null) return;

        bgmSource.volume = settings.isBgmMuted ? 0f : settings.bgmVolume;
        sfxSource.volume = settings.isSfxMuted ? 0f : settings.sfxVolume;
    }

    public void SetBgmVolume(float volume)
    {
        if (dataManager == null || dataManager.GlobalSettings == null) return;
        ApplyVolume(bgmSource, ref dataManager.GlobalSettings.bgmVolume, dataManager.GlobalSettings.isBgmMuted, volume);
    }

    public void SetBgmMuted(bool isMuted)
    {
        if (dataManager == null || dataManager.GlobalSettings == null) return;
        ApplyMute(bgmSource, ref dataManager.GlobalSettings.isBgmMuted, dataManager.GlobalSettings.bgmVolume, isMuted);
    }

    public void SetSfxVolume(float volume)
    {
        if (dataManager == null || dataManager.GlobalSettings == null) return;
        ApplyVolume(sfxSource, ref dataManager.GlobalSettings.sfxVolume, dataManager.GlobalSettings.isSfxMuted, volume);
    }

    public void SetSfxMuted(bool isMuted)
    {
        if (dataManager == null || dataManager.GlobalSettings == null) return;
        ApplyMute(sfxSource, ref dataManager.GlobalSettings.isSfxMuted, dataManager.GlobalSettings.sfxVolume, isMuted);
    }

    // Shared by all four Set*Volume/Set*Muted calls above - only the source and which field they touch differ.
    private void ApplyVolume(AudioSource source, ref float storedVolume, bool isMuted, float newVolume)
    {
        storedVolume = newVolume;
        if (!isMuted) source.volume = newVolume;
        dataManager.SaveGlobalSettings();
    }

    private void ApplyMute(AudioSource source, ref bool storedMuted, float volume, bool isMuted)
    {
        storedMuted = isMuted;
        source.volume = isMuted ? 0f : volume;
        dataManager.SaveGlobalSettings();
    }

    public void PlaySfx(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        sfxSource.PlayOneShot(clip);
    }

    // Fades the current track out, swaps the clip, then fades the new one in. Call this whenever a scene wants its own BGM.
    public void PlayBgm(AudioClip clip)
    {
        if (bgmFadeRoutine != null) StopCoroutine(bgmFadeRoutine);
        bgmFadeRoutine = StartCoroutine(FadeToNewBgm(clip));
    }

    private IEnumerator FadeToNewBgm(AudioClip clip)
    {
        bool isMuted = dataManager != null && dataManager.GlobalSettings != null && dataManager.GlobalSettings.isBgmMuted;
        float targetVolume = isMuted || dataManager == null || dataManager.GlobalSettings == null ? 0f : dataManager.GlobalSettings.bgmVolume;

        yield return StartCoroutine(FadeVolume(bgmSource, bgmSource.volume, 0f, bgmFadeDuration));

        bgmSource.clip = clip;
        if (clip != null) bgmSource.Play();

        yield return StartCoroutine(FadeVolume(bgmSource, 0f, targetVolume, bgmFadeDuration));
    }

    private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
        source.volume = to;
    }
}
