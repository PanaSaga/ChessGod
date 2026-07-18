using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// One row in AchievementListPanel's list.
public class AchievementListEntry : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text progressText;
    [SerializeField] private GameObject newBadge;
    [SerializeField] private Button claimButton;

    public void Setup(AchievementSO achievement, string displayNameOverride, string descriptionOverride, Sprite iconOverride,
        bool achieved, int progress, bool showClaim, Action onClaim)
    {
        if (icon != null) icon.sprite = iconOverride != null ? iconOverride : achievement.icon;
        if (nameText != null) nameText.text = displayNameOverride ?? achievement.displayName;
        if (descriptionText != null) descriptionText.text = descriptionOverride ?? achievement.description;
        if (progressText != null) progressText.text = achieved ? string.Empty : $"{progress}/{achievement.targetCount}";
        if (newBadge != null) newBadge.SetActive(showClaim);

        if (claimButton == null) return;
        claimButton.gameObject.SetActive(showClaim);
        claimButton.onClick.RemoveAllListeners();
        if (showClaim) claimButton.onClick.AddListener(() => onClaim?.Invoke());
    }
}
