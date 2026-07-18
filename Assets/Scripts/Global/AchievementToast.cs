using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A brief, buttonless notification (like an SNS alert) - slides up, holds, slides back down.
// AchievementUIManager owns the queue; this component only knows how to display one at a time.
public class AchievementToast : MonoBehaviour
{
    [SerializeField] private SlideUpPopup slideAnimation;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField, Min(0.1f)] private float displayDuration = 3f;

    public void Show(AchievementSO achievement, Action onFinished)
    {
        if (icon != null) icon.sprite = achievement.icon;
        if (nameText != null) nameText.text = achievement.displayName;
        if (descriptionText != null) descriptionText.text = achievement.description;

        if (slideAnimation == null) { onFinished?.Invoke(); return; }
        slideAnimation.SlideIn(() => StartCoroutine(HoldThenSlideOut(onFinished)));
    }

    private IEnumerator HoldThenSlideOut(Action onFinished)
    {
        yield return new WaitForSeconds(displayDuration);
        slideAnimation.SlideOut(() => onFinished?.Invoke());
    }
}
