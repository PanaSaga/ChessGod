using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public enum TutorialStepAction
    {
        None,
        ForceSpawnBuffPawn,
        ForceSpawnTransformPiece,
        ForceSpawnBlackKing,
        WaitForSafeZone,
        EndTutorial
    }

    [System.Serializable]
    private class TutorialStep
    {
        public string speakerName;
        [TextArea] public string dialogueText;
        public Transform targetTransform;
        public Vector2 positionOffset;
        public TutorialStepAction action;
    }

    [Header("Tutorial steps")]
    [SerializeField] private List<TutorialStep> steps = new();

    [Header("UI references")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform panelRectTransform;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button skipButton;

    [Header("Forced spawn data")]
    [SerializeField] private WhitePieceSO buffPawnData;
    [SerializeField] private WhitePieceSO transformPieceData;
    [SerializeField] private BlackPieceSO blackKingData;

    [Header("Scene transition")]
    [SerializeField] private string nextSceneName = "MainScene";

    private GameManager gameManager;
    private ControlManager controlManager;
    private SpawnManager spawnManager;
    private int currentStepIndex = -1;

    // Safe-zone checkpoint state (Turn 2 space-bar practice step).
    private bool isWaitingForSafeZone;
    private bool isWaitingForTurnSettlement;
    private bool hasSeenSettlementStart;
    private bool isTimerFrozen;
    private float frozenTurnTimer;

    private void Start()
    {
        if (GlobalManager.Instance == null || !GlobalManager.Instance.LaunchTutorialOnNextIngame)
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            enabled = false;
            return;
        }
        GlobalManager.Instance.LaunchTutorialOnNextIngame = false;

        gameManager = FindFirstObjectByType<GameManager>();
        controlManager = FindFirstObjectByType<ControlManager>();
        spawnManager = FindFirstObjectByType<SpawnManager>();

        if (nextButton != null) nextButton.onClick.AddListener(OnNextPressed);
        if (skipButton != null) skipButton.onClick.AddListener(OnSkipPressed);

        if (gameManager != null) gameManager.isTutorialActive = true;

        ShowStep(0);
    }

    private void Update()
    {
        // Keep the turn timer frozen for the whole safe-zone checkpoint, from the moment it
        // starts until the player's real space press actually begins settlement.
        if (isTimerFrozen && gameManager != null && !gameManager.isSettling)
            gameManager.turnTimer = frozenTurnTimer;

        if (isWaitingForSafeZone) { PollSafeZone(); return; }
        if (isWaitingForTurnSettlement) { PollTurnSettlement(); return; }

        // Space only advances a dialogue step. While isTutorialPaused is false (the safe-zone
        // checkpoint), the real space bar goes to GameManager's own turn-end handling instead.
        if (gameManager != null && gameManager.isTutorialPaused
            && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            OnNextPressed();
    }

    private void ShowStep(int index)
    {
        if (index < 0 || index >= steps.Count)
        {
            EndTutorial();
            return;
        }

        currentStepIndex = index;
        TutorialStep step = steps[index];

        if (panelRoot != null) panelRoot.SetActive(true);
        if (nameText != null) nameText.text = step.speakerName;
        if (dialogueText != null) dialogueText.text = step.dialogueText;
        MovePanelToTarget(step.targetTransform, step.positionOffset);

        if (gameManager != null) gameManager.isTutorialPaused = true;

        if (step.action is TutorialStepAction.ForceSpawnBuffPawn
            or TutorialStepAction.ForceSpawnTransformPiece
            or TutorialStepAction.ForceSpawnBlackKing
            or TutorialStepAction.EndTutorial)
            ExecuteStepAction(step.action);
    }

    // The panel's Canvas is Screen Space - Overlay, so a RectTransform's .position is already
    // a screen-pixel coordinate; a world-space piece needs one extra conversion to reach that
    // same space.
    private void MovePanelToTarget(Transform target, Vector2 offset)
    {
        if (panelRectTransform == null || target == null) return;

        RectTransform targetRect = target as RectTransform;
        Vector3 screenPoint = targetRect != null
            ? targetRect.position
            : Camera.main.WorldToScreenPoint(target.position);

        panelRectTransform.position = screenPoint + (Vector3)offset;
    }

    private void OnNextPressed()
    {
        if (currentStepIndex < 0 || currentStepIndex >= steps.Count) return;
        TutorialStep step = steps[currentStepIndex];

        if (panelRoot != null) panelRoot.SetActive(false);
        if (gameManager != null) gameManager.isTutorialPaused = false;

        if (step.action == TutorialStepAction.WaitForSafeZone)
        {
            BeginSafeZoneCheckpoint();
            return;
        }

        ShowStep(currentStepIndex + 1);
    }

    private void OnSkipPressed()
    {
        EndTutorial();
    }

    private void ExecuteStepAction(TutorialStepAction action)
    {
        switch (action)
        {
            case TutorialStepAction.ForceSpawnBuffPawn:
                ForceSpawnPiece(buffPawnData);
                break;
            case TutorialStepAction.ForceSpawnTransformPiece:
                ForceSpawnPiece(transformPieceData);
                break;
            case TutorialStepAction.ForceSpawnBlackKing:
                ForceSpawnPiece(blackKingData);
                break;
            case TutorialStepAction.EndTutorial:
                EndTutorial();
                break;
        }
    }

    private void ForceSpawnPiece(ChessPieceSO data)
    {
        if (spawnManager == null || gameManager == null || controlManager == null || data == null) return;

        Vector2Int position = spawnManager.GetRandomFreePosition(controlManager.GetPlayerGridPosition());
        if (position.x < 0) return;

        spawnManager.SpawnPiece(data, position, gameManager.currentTurn);
    }

    private void BeginSafeZoneCheckpoint()
    {
        if (gameManager == null) return;

        frozenTurnTimer = gameManager.turnTimer;
        isTimerFrozen = true;
        gameManager.isTutorialTurnEndBlocked = true;
        isWaitingForSafeZone = true;
    }

    private void PollSafeZone()
    {
        if (gameManager == null || controlManager == null || spawnManager == null) return;

        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();
        if (IsPositionUnderAttack(playerPosition)) return;

        // The player just reached a safe tile: let a real space press end the turn now,
        // but keep the timer frozen until that press actually starts settlement.
        gameManager.isTutorialTurnEndBlocked = false;
        isWaitingForSafeZone = false;
        isWaitingForTurnSettlement = true;
        hasSeenSettlementStart = false;
    }

    private bool IsPositionUnderAttack(Vector2Int position)
    {
        return spawnManager.activePieces
            .OfType<BlackEnemyPiece>()
            .Any(enemy => ChessAttackResolver.GetAttackCells(enemy.pieceData, enemy.gridPos).Contains(position));
    }

    private void PollTurnSettlement()
    {
        if (gameManager == null) return;

        if (!hasSeenSettlementStart)
        {
            if (gameManager.isSettling) hasSeenSettlementStart = true;
            return;
        }

        if (gameManager.isSettling) return;

        // Settlement finished: the turn actually ended, so the checkpoint is complete.
        isWaitingForTurnSettlement = false;
        isTimerFrozen = false;
        ShowStep(currentStepIndex + 1);
    }

    private void EndTutorial()
    {
        if (gameManager != null)
        {
            gameManager.isTutorialActive = false;
            gameManager.isTutorialPaused = false;
            gameManager.isTutorialTurnEndBlocked = false;
        }
        SceneManager.LoadScene(nextSceneName);
    }
}
