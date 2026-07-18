using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// A brief, buttonless notification (like an SNS alert) - shows then auto-hides on its own timer.
// AchievementUIManager owns the queue; this component only knows how to display one at a time.
public class AchievementToast : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField, Min(0.1f)] private float displayDuration = 3f;

    private void Start()
    {
        if (root != null) root.SetActive(false);
    }

    public void Show(AchievementSO achievement, Action onFinished)
    {
        if (icon != null) icon.sprite = achievement.icon;
        if (nameText != null) nameText.text = achievement.displayName;
        if (descriptionText != null) descriptionText.text = achievement.description;
        if (root != null) root.SetActive(true);
        StartCoroutine(HideAfterDelay(onFinished));
    }

    private IEnumerator HideAfterDelay(Action onFinished)
    {
        yield return new WaitForSeconds(displayDuration);
        if (root != null) root.SetActive(false);
        onFinished?.Invoke();
    }
}
