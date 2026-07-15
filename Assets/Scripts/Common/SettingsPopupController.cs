using UnityEngine;
using UnityEngine.UI;

// Bridges the settings/pause popup's SFX/BGM slider+toggle to SoundManager. SoundManager
// lives under GlobalManager (BootScene, DontDestroyOnLoad) so it can't be wired via
// Inspector drag-and-drop from MainScene/InGame -- it's looked up at runtime instead.
// Shared by both the lobby settings popup and the ingame pause popup (설계 원칙 9: both
// popups include the same SFX/BGM controls).
public class SettingsPopupController : MonoBehaviour
{
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle bgmMuteToggle;
    [SerializeField] private Toggle sfxMuteToggle;

    private SoundManager soundManager;

    // 팝업의 PopupUI.OnOpened에 연결 -- 열릴 때마다 현재 볼륨/뮤트 상태를 반영
    public void RefreshFromSoundManager()
    {
        soundManager = FindFirstObjectByType<SoundManager>();
        if (soundManager == null)
        {
            Debug.LogWarning("[SettingsPopupController] SoundManager를 찾을 수 없습니다.");
            return;
        }

        // SetValueWithoutNotify로 채워야 OnValueChanged가 다시 SetBgmVolume 등을 불러
        // 값을 그대로 되돌려쓰는 무의미한 왕복이 안 생김
        if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(soundManager.BgmVolume);
        if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(soundManager.SfxVolume);
        if (bgmMuteToggle != null) bgmMuteToggle.SetIsOnWithoutNotify(soundManager.BgmMuted);
        if (sfxMuteToggle != null) sfxMuteToggle.SetIsOnWithoutNotify(soundManager.SfxMuted);
    }

    // 슬라이더/토글의 OnValueChanged()에 각각 연결
    public void OnBgmSliderChanged(float value) => soundManager?.SetBgmVolume(value);
    public void OnSfxSliderChanged(float value) => soundManager?.SetSfxVolume(value);
    public void OnBgmMuteToggled(bool muted) => soundManager?.SetBgmMuted(muted);
    public void OnSfxMuteToggled(bool muted) => soundManager?.SetSfxMuted(muted);
}
