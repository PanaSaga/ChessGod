using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Reads GameManager state every frame and writes it into the HUD. Owns no game data itself.
public class InGameUIManager : MonoBehaviour
{
    [Serializable]
    public struct DefeatedCountSlot
    {
        public ChessPieceType pieceType;
        public TMP_Text countText;
    }

    [Serializable]
    public struct AliveCountSlot
    {
        public ChessPieceType pieceType;
        [Tooltip("Up to 5 pip icon GameObjects, shown left to right in order.")]
        public List<GameObject> pipIcons;
        [Tooltip("Shows \"+N\" when the alive count exceeds the number of pip icons.")]
        public TMP_Text overflowText;
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

    [Header("Alive piece counts")]
    [SerializeField] private List<AliveCountSlot> aliveCountSlots = new();

    [Header("Buff status (White Pawn)")]
    [SerializeField] private Image buffIconImage;
    [SerializeField] private Sprite buffOnIcon;
    [SerializeField] private Sprite buffOffIcon;
    [SerializeField] private TMP_Text buffDurationText;

    [Header("Transform status")]
    [SerializeField] private Image transformIconImage;
    [Tooltip("Shown when no transform is active (King).")]
    [SerializeField] private Sprite transformIdleIcon;
    [SerializeField] private List<PieceTypeIconEntry> transformIcons = new();
    [SerializeField] private TMP_Text transformDurationText;

    [Header("Player avatar")]
    [SerializeField] private Image avatarImage;
    [Tooltip("Normal: no attack, no hit, or before the first turn resolves.")]
    [FormerlySerializedAs("avatarIdle")]
    [SerializeField] private Sprite avatarNormal;
    [Tooltip("Attack: attacked an enemy and was not hit.")]
    [FormerlySerializedAs("avatarHappy")]
    [SerializeField] private Sprite avatarAttack;
    [Tooltip("Mutual Attack: attacked an enemy and was also hit.")]
    [FormerlySerializedAs("avatarAngry")]
    [SerializeField] private Sprite avatarMutualAttack;
    [Tooltip("Hit: did not attack an enemy but was hit.")]
    [FormerlySerializedAs("avatarSurprised")]
    [SerializeField] private Sprite avatarHit;
    [Tooltip("Defeat: game over.")]
    [FormerlySerializedAs("avatarSad")]
    [SerializeField] private Sprite avatarDefeat;
    [Tooltip("How long Attack/Mutual Attack/Hit stays before returning to Normal.")]
    [SerializeField, Min(0f)] private float avatarResultHoldSeconds = 1f;

    private PlayerPiece playerPiece;
    private SpawnManager spawnManager;

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

        if (spawnManager == null) spawnManager = FindFirstObjectByType<SpawnManager>();
        if (spawnManager != null)
            foreach (AliveCountSlot slot in aliveCountSlots)
                RefreshAliveCountSlot(slot, spawnManager.GetAliveBlackCount(slot.pieceType));

        if (playerPiece == null) playerPiece = FindFirstObjectByType<PlayerPiece>();
        if (playerPiece == null) return;

        if (buffIconImage != null) buffIconImage.sprite = playerPiece.isBuffActive ? buffOnIcon : buffOffIcon;
        if (buffDurationText != null)
            buffDurationText.text = playerPiece.isBuffActive ? Mathf.Max(0f, playerPiece.buffTimer).ToString("F1") : "-";

        if (transformIconImage != null)
            transformIconImage.sprite = playerPiece.isTransformActive
                ? GetTransformIcon(playerPiece.transformedAttackData.pieceType)
                : transformIdleIcon;
        if (transformDurationText != null)
            transformDurationText.text = playerPiece.isTransformActive ? Mathf.Max(0f, playerPiece.transformTimer).ToString("F1") : "-";

        if (avatarImage != null) avatarImage.sprite = GetAvatarIcon(gameManager);
    }

    private static void RefreshAliveCountSlot(AliveCountSlot slot, int aliveCount)
    {
        if (slot.pipIcons == null) return;

        int visiblePips = Mathf.Min(aliveCount, slot.pipIcons.Count);
        for (int i = 0; i < slot.pipIcons.Count; i++)
            if (slot.pipIcons[i] != null) slot.pipIcons[i].SetActive(i < visiblePips);

        if (slot.overflowText == null) return;
        int overflow = aliveCount - slot.pipIcons.Count;
        slot.overflowText.gameObject.SetActive(overflow > 0);
        if (overflow > 0) slot.overflowText.text = $"+{overflow}";
    }

    private Sprite GetTransformIcon(ChessPieceType pieceType) =>
        PieceTypeIconLookup.Find(transformIcons, pieceType) ?? transformIdleIcon;

    private Sprite GetAvatarIcon(GameManager gameManager)
    {
        if (gameManager.isGameOver || gameManager.LastTurnFatal) return avatarDefeat;
        if (Time.time - gameManager.LastSettlementTime >= avatarResultHoldSeconds) return avatarNormal;

        bool attacked = gameManager.LastTurnAttackHitEnemy;
        bool wasHit = gameManager.LastTurnPlayerWasHit;
        if (attacked && !wasHit) return avatarAttack;
        if (attacked && wasHit) return avatarMutualAttack;
        if (!attacked && wasHit) return avatarHit;
        return avatarNormal;
    }
}
