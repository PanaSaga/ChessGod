using TMPro;
using UnityEngine;

public class GameOverPopup : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text stageText;

    [Header("Score text format")]
    [Tooltip("{0} = 최종 점수")]
    [SerializeField, TextArea] private string scoreTextFormat = "점수: {0}";

    [Header("Stage text format")]
    [Tooltip("{0} = 도달 스테이지, {1} = 도달 턴")]
    [SerializeField, TextArea] private string stageTextFormat = "{0}층 이동하는동안 방 {1}개를 조사했다!";

    private void Start()
    {
        if (panelRoot != null) panelRoot.SetActive(false);
    }

    public void Show(int finalScore, int finalStage, int finalTurn)
    {
        if (scoreText != null) scoreText.text = string.Format(scoreTextFormat, finalScore);
        if (stageText != null) stageText.text = string.Format(stageTextFormat, finalStage, finalTurn);
        if (panelRoot != null) panelRoot.SetActive(true);
    }
}
