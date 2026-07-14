using System.Collections;
using UnityEngine;

/// <summary>
/// BGM 페이드 인/아웃, 효과음 재생, 볼륨/온오프를 담당하는 매니저.
/// 붙이는 위치: GlobalManager 하위의 "SoundManager" 오브젝트 (AudioSource 2개 필요)
///
/// 씬 전환 시 자동 페이드: 이 스크립트를 직접 호출하지 않고,
/// 같은 오브젝트에 GameEventListener를 추가로 부착해 OnSceneTransition 이벤트를
/// 구독하고 Response에 FadeOutBGM()을 연결하는 방식을 권장합니다.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SoundManager : MonoBehaviour
{
    [Header("BGM 재생용 AudioSource")]
    [SerializeField] private AudioSource bgmSource;

    [Header("효과음 재생용 AudioSource")]
    [SerializeField] private AudioSource sfxSource;

    [Header("페이드 소요 시간(초)")]
    [SerializeField] private float fadeDuration = 1f;

    private Coroutine fadeCoroutine;

    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    // OnSceneTransition 이벤트의 Response로 연결
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

    public void SetBgmVolume(float value) => bgmSource.volume = value;
    public void SetSfxVolume(float value) => sfxSource.volume = value;

    public void SetBgmMuted(bool muted) => bgmSource.mute = muted;
    public void SetSfxMuted(bool muted) => sfxSource.mute = muted;
}
