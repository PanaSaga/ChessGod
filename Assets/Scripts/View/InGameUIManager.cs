using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Reads GameManager state every frame and writes it into the HUD. Owns no game data itself.
public class InGameUIManager : MonoBehaviour
{
    [Serializable]
    private struct DefeatedCountSlot
    {
        public ChessPieceType pieceType;
        public TMP_Text countText;
    }

    [Header("Stage / Turn / Score")]
    [SerializeField] private TMP_Text stageText;
    [SerializeField] private TMP_Text turnText;
    [SerializeField] private TMP_Text scoreText;

    [Header("Turn timer")]
    [SerializeField] private TMP_Text timerText;
    [Tooltip("Board_Timer_Bar Image. Image Type must be set to Filled (Horizontal) in the Inspector.")]
    [SerializeField] private Image timerFillImage;

    [Header("Defeated piece counts")]
    [SerializeField] private List<DefeatedCountSlot> defeatedCountSlots = new();

    private void Update()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager == null) return;

        if (stageText != null) stageText.text = gameManager.currentStage.ToString("00");
        if (turnText != null) turnText.text = gameManager.currentTurn.ToString("00");
        if (scoreText != null) scoreText.text = gameManager.playerScore.ToString("00");

        if (timerText != null) timerText.text = Mathf.Max(0f, gameManager.turnTimer).ToString("F1");
        if (timerFillImage != null)
            timerFillImage.fillAmount = gameManager.maxTurnTime > 0f
                ? Mathf.Clamp01(gameManager.turnTimer / gameManager.maxTurnTime)
                : 0f;

        foreach (DefeatedCountSlot slot in defeatedCountSlots)
        {
            if (slot.countText == null) continue;
            slot.countText.text = gameManager.GetDefeatedCount(slot.pieceType).ToString("00");
        }
    }
}
