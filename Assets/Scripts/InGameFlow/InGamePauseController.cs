using UnityEngine;

// Drives the ingame pause popup without touching GameManager.cs (설계 원칙 10). Subscribes
// to the pause PopupUI's OnOpened/OnClosed (wired in the Inspector, not here) instead of
// owning Esc detection itself -- CommonUIManager's escTogglePopup already opens/closes the
// popup, this script only reacts to that.
//
// Placement: InGame.unity, alongside GameSessionObserver/AchievementTracker (scene-local,
// same GameManager.Instance dependency).
public class InGamePauseController : MonoBehaviour
{
    public void OnPausePopupOpened()
    {
        Time.timeScale = 0f;
        if (GameManager.Instance != null) GameManager.Instance.enabled = false;
    }

    public void OnPausePopupClosed()
    {
        Time.timeScale = 1f;
        if (GameManager.Instance != null) GameManager.Instance.enabled = true;
    }

    // Safety net: if this object is ever disabled/destroyed while still paused
    // (e.g. scene unload mid-pause), don't leave timeScale stuck at 0 for the next scene.
    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
