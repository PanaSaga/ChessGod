using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 업적 리스트 한 줄 프리팹에 부착합니다.
/// AchievementPopulator가 이 함수를 호출해 데이터를 화면에 표시합니다.
/// </summary>
public class AchievementSlotUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text conditionText;
    [SerializeField] private TMP_Text progressText;

    public void Setup(AchievementData data)
    {
        if (titleText != null) titleText.text = data.title;
        if (conditionText != null) conditionText.text = data.condition;
        if (progressText != null) progressText.text = $"{data.currentProgress}/{data.targetProgress}";
    }
}
