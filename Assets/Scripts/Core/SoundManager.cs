using System.Collections;
using UnityEngine;

// BGM fade in/out, SFX playback, volume/mute. Lives under GlobalManager (persistent, see
// CLAUDE.md 설계 원칙 11) so it survives every scene transition instead of restarting.
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeCoroutine;
    private DataManager dataManager;

    private void Awake()
    {
        dataManager = FindFirstObjectByType<DataManager>();
        ApplySavedSettings();
    }

    private void ApplySavedSettings()
    {
        if (dataManager == null) return;
        SetBgmVolume(dataManager.GetBgmVolume());
        SetSfxVolume(dataManager.GetSfxVolume());
        SetBgmMuted(dataManager.GetBgmMuted());
        SetSfxMuted(dataManager.GetSfxMuted());
    }

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    // OnSceneTransition GameEvent의 Response로 연결
    public void FadeOutBGM()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolume(bgmSource, bgmSource.volume, 0f));
    }

    public void FadeInBGM()
    {
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeVolume(bgmSource, bgmSource.volume, 1f));
    }

    private IEnumerator FadeVolume(AudioSource source, float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }
        source.volume = to;
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }

    public void SetBgmVolume(float value)
    {
        bgmSource.volume = value;
        dataManager?.SetBgmVolume(value);
    }

    public void SetSfxVolume(float value)
    {
        sfxSource.volume = value;
        dataManager?.SetSfxVolume(value);
    }

    public void SetBgmMuted(bool muted)
    {
        bgmSource.mute = muted;
        dataManager?.SetBgmMuted(muted);
    }

    public void SetSfxMuted(bool muted)
    {
        sfxSource.mute = muted;
        dataManager?.SetSfxMuted(muted);
    }

    // 설정 팝업 UI가 현재 상태를 슬라이더/토글에 반영할 때 사용
    public float BgmVolume => bgmSource.volume;
    public float SfxVolume => sfxSource.volume;
    public bool BgmMuted => bgmSource.mute;
    public bool SfxMuted => sfxSource.mute;
}
