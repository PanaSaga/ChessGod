using UnityEngine;
using UnityEngine.UI;
using TMPro;

// One row of the always-visible achievement panel. Icon is unique per achievement and
// resolved by AchievementPopulator (id -> Sprite lookup), not carried in AchievementData.
public class AchievementSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text conditionText;
    [SerializeField] private TMP_Text progressText;

    [Header("달성/미달성 배지")]
    [SerializeField] private TMP_Text badgeText;
    [SerializeField] private Image badgeBackground;
    [SerializeField] private Color completedColor = new Color(0.85f, 0.65f, 0.2f);
    [SerializeField] private Color incompleteColor = new Color(0.6f, 0.6f, 0.6f);

    public void Setup(AchievementData data, Sprite icon)
    {
        if (iconImage != null) iconImage.sprite = icon;
        if (titleText != null) titleText.text = data.title;
        if (conditionText != null) conditionText.text = data.condition;
        if (progressText != null) progressText.text = $"{data.currentProgress}/{data.targetProgress}";

        if (badgeText != null) badgeText.text = data.isCompleted ? "달성" : "미달성";
        if (badgeBackground != null) badgeBackground.color = data.isCompleted ? completedColor : incompleteColor;
    }
}
