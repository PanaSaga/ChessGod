using System;
using System.Collections;
using UnityEngine;

// Slides a RectTransform up from below its resting position, and back down on request - reused by
// AchievementToast and LobbyCharacterController's dialogue popup for the same notification motion.
public class SlideUpPopup : MonoBehaviour
{
    [SerializeField] private RectTransform target;
    [Tooltip("How far below the resting position this starts/ends, in UI units.")]
    [SerializeField, Min(0f)] private float hiddenYOffset = 200f;
    [SerializeField, Min(0.01f)] private float slideDuration = 0.3f;
    [SerializeField] private AnimationCurve slideCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float restingY;
    private bool restingYCaptured;
    private Coroutine activeRoutine;

    // Awake can't be relied on here - this starts inactive, and Awake doesn't run on an inactive
    // GameObject until it's first activated - so the resting Y is captured lazily on first use instead.
    private void CaptureRestingY()
    {
        if (target == null) target = GetComponent<RectTransform>();
        if (restingYCaptured || target == null) return;
        restingY = target.anchoredPosition.y;
        restingYCaptured = true;
    }

    public void SlideIn(Action onComplete = null)
    {
        CaptureRestingY();
        if (target == null) { onComplete?.Invoke(); return; }

        gameObject.SetActive(true);
        StartSlide(restingY - hiddenYOffset, restingY, onComplete);
    }

    public void SlideOut(Action onComplete = null)
    {
        CaptureRestingY();
        if (target == null) { onComplete?.Invoke(); return; }

        StartSlide(target.anchoredPosition.y, restingY - hiddenYOffset, () =>
        {
            gameObject.SetActive(false);
            onComplete?.Invoke();
        });
    }

    private void StartSlide(float fromY, float toY, Action onComplete)
    {
        if (activeRoutine != null) StopCoroutine(activeRoutine);
        activeRoutine = StartCoroutine(SlideRoutine(fromY, toY, onComplete));
    }

    private IEnumerator SlideRoutine(float fromY, float toY, Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            elapsed += Time.deltaTime;
            float t = slideCurve.Evaluate(Mathf.Clamp01(elapsed / slideDuration));
            target.anchoredPosition = new Vector2(target.anchoredPosition.x, Mathf.Lerp(fromY, toY, t));
            yield return null;
        }
        target.anchoredPosition = new Vector2(target.anchoredPosition.x, toY);
        activeRoutine = null;
        onComplete?.Invoke();
    }
}
