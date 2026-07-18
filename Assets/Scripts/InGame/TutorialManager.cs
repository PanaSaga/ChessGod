using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    // What happens when the player presses Next/space on a step, instead of immediately
    // revealing the next step's panel.
    public enum TutorialStepAction
    {
        None,
        // Settlement is fully done (currentTurn has NOT incremented yet), but nothing shows until
        // this happens - attach to the step right before the "turn just ended" dialogue.
        WaitForTurnEnd,
        // Attach to the LAST "turn just ended" dialogue step: pressing Next here is what actually
        // advances currentTurn (spawns/timer reset/etc.) before moving on to the next step.
        AdvanceTurn,
        WaitForBuffPickup,
        WaitForTransformPickup,
        WaitForSafeZoneArrival,
        EndTutorial
    }

    // Which side/corner of the target the textbox sits on. X/Z follow world axes (X = left/right,
    // Z = the axis that reads as up/down on this top-down camera - same convention used everywhere else).
    public enum TutorialAnchor
    {
        Top,
        Bottom,
        Left,
        Right,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    // One piece to place the instant this step is shown - either a brand new spawn, or moving
    // an already-active piece of the same data to a new tile. Lets a single step place several
    // pieces at once (e.g. repositioning survivors and spawning a new one together).
    [System.Serializable]
    public class ForcedPiece
    {
        public ChessPieceSO pieceData;
        public Vector2Int position;
        [Tooltip("Checked: move an already-active piece using this same data asset. Unchecked: spawn a new one.")]
        public bool repositionExisting;
    }

    [System.Serializable]
    public class TutorialStep
    {
        [Tooltip("Your own label for finding this step in the list - never shown in-game.")]
        public string editorLabel;

        public string speakerName;
        [TextArea] public string dialogueText;
        [Tooltip("For a fixed scene object: the player, or a UI element like the timer text.")]
        public Transform targetTransform;
        [Tooltip("For a black/white piece instead: pieces only exist at runtime, so this looks up whichever active piece is currently using this data when the step shows. Leave empty and use Target Transform above for anything else. If both are set, this takes priority.")]
        public ChessPieceSO targetPieceData;

        [Header("World-space anchor (used when Target Transform is a piece/board object)")]
        public TutorialAnchor anchor = TutorialAnchor.BottomLeft;
        [Tooltip("Board-unit distance from the target along the anchor direction. One board tile = 1.")]
        public float anchorDistance = 0.3f;

        [Header("Screen-space offset (used when Target Transform is a UI element instead)")]
        public Vector2 screenOffset;

        [Header("Pieces to force into place the instant this step shows")]
        public List<ForcedPiece> forcedPieces = new();

        [Header("What happens on Next/space")]
        public TutorialStepAction action;
        [Tooltip("Only used when Action is Wait For Safe Zone Arrival - the exact tile the player must reach.")]
        public Vector2Int safeZonePosition;
        [Tooltip("Check this on the step that teaches the space bar. Until this step has shown, space can never end a turn - so the player can't stumble into ending a turn before being told how.")]
        public bool teachesSpacebar;
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

    [Header("Scene transition")]
    [SerializeField] private string nextSceneName = SceneNames.MainLobby;

    [Header("Safe zone overlay")]
    [Tooltip("Board_ChessW_Safe: shown on the exact tile the player needs to reach for a safe-zone checkpoint.")]
    [SerializeField] private Sprite safeZoneSprite;

    private GameManager gameManager;
    private ControlManager controlManager;
    private SpawnManager spawnManager;
    private PlayerPiece playerPiece;
    private BoardManager boardManager;
    private int currentStepIndex = -1;

    // Wait-condition polling state.
    private TutorialStepAction pendingWait = TutorialStepAction.None;
    private bool isWaitingForTurnSettlement;
    private bool hasSeenSettlementStart;
    private Vector2Int currentSafeZoneTarget;
    private bool hasTaughtSpacebar;

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
        playerPiece = FindFirstObjectByType<PlayerPiece>();
        boardManager = FindFirstObjectByType<BoardManager>();

        if (nextButton != null) nextButton.onClick.AddListener(OnNextPressed);
        if (skipButton != null) skipButton.onClick.AddListener(OnSkipPressed);

        if (gameManager != null) gameManager.isTutorialActive = true;

        ShowStep(0);
    }

    private void Update()
    {
        // Per-turn rule overrides: turns 2-3 run untimed, turn 2 keeps the buff duration from
        // ticking (the player is meant to still have it active when reaching the knight), and
        // turn 3 keeps the transform duration from ticking (same reasoning for the transform).
        if (gameManager != null)
        {
            gameManager.isTutorialTimerFrozen = gameManager.currentTurn >= 2;
            gameManager.isTutorialBuffTimerFrozen = gameManager.currentTurn == 2;
            gameManager.isTutorialTransformTimerFrozen = gameManager.currentTurn == 3;

            // Space can't end a turn at all until the player has been taught it, or while a
            // pickup is still pending (picking up the buff/transform is a prerequisite for the
            // rest of the turn's script, so the turn must not end before it happens - even in a
            // later turn where space was already taught). A safe-zone checkpoint manages the
            // flag itself every frame instead (see PollSafeZone), since it depends on position.
            if (!hasTaughtSpacebar
                || pendingWait == TutorialStepAction.WaitForBuffPickup
                || pendingWait == TutorialStepAction.WaitForTransformPickup)
            {
                gameManager.isTutorialTurnEndBlocked = true;
            }
            else if (pendingWait != TutorialStepAction.WaitForSafeZoneArrival)
            {
                gameManager.isTutorialTurnEndBlocked = false;
            }
        }

        if (pendingWait == TutorialStepAction.WaitForSafeZoneArrival) { PollSafeZone(); return; }
        if (isWaitingForTurnSettlement) { PollTurnSettlement(); return; }
        if (pendingWait == TutorialStepAction.WaitForBuffPickup) { PollBuffPickup(); return; }
        if (pendingWait == TutorialStepAction.WaitForTransformPickup) { PollTransformPickup(); return; }

        // Space only advances a dialogue step. While isTutorialPaused is false (a wait-condition
        // is active instead), the real space bar goes to GameManager's own turn-end handling.
        if (gameManager != null && gameManager.isTutorialPaused
            && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            OnNextPressed();
    }

    private void ShowStep(int index)
    {
        if (index < 0 || index >= steps.Count)
        {
            StartCoroutine(EndTutorial());
            return;
        }

        currentStepIndex = index;
        TutorialStep step = steps[index];

        if (panelRoot != null) panelRoot.SetActive(true);
        if (nameText != null) nameText.text = step.speakerName;
        if (dialogueText != null) dialogueText.text = step.dialogueText;
        MovePanelToTarget(step);

        if (step.teachesSpacebar) hasTaughtSpacebar = true;

        foreach (ForcedPiece forced in step.forcedPieces)
            ApplyForcedPiece(forced);

        // These end on their own once their condition is met - no Next press needed (or possible),
        // so the button is hidden and the player can act immediately instead of being paused.
        bool isAutoWaitStep = step.action == TutorialStepAction.WaitForTurnEnd
            || step.action == TutorialStepAction.WaitForBuffPickup
            || step.action == TutorialStepAction.WaitForTransformPickup
            || step.action == TutorialStepAction.WaitForSafeZoneArrival;

        if (nextButton != null) nextButton.gameObject.SetActive(!isAutoWaitStep);
        if (gameManager != null) gameManager.isTutorialPaused = !isAutoWaitStep;

        switch (step.action)
        {
            case TutorialStepAction.WaitForTurnEnd:
                BeginWaitForTurnEnd();
                break;
            case TutorialStepAction.WaitForBuffPickup:
            case TutorialStepAction.WaitForTransformPickup:
                pendingWait = step.action;
                if (gameManager != null) gameManager.isTutorialTurnEndBlocked = true;
                break;
            case TutorialStepAction.WaitForSafeZoneArrival:
                BeginSafeZoneCheckpoint(step.safeZonePosition);
                break;
        }
    }

    // The panel's Canvas is Screen Space - Overlay, so a RectTransform's .position is already
    // a screen-pixel coordinate; a world-space piece needs one extra conversion to reach that
    // same space. UI targets use a flat screen offset; world targets are anchored in board units
    // (X/Z) before converting to screen space, so the offset stays consistent with the game's own
    // grid regardless of camera zoom.
    private void MovePanelToTarget(TutorialStep step)
    {
        Transform target = ResolveTarget(step);
        if (panelRectTransform == null || target == null) return;

        RectTransform targetRect = target as RectTransform;
        if (targetRect != null)
        {
            panelRectTransform.position = targetRect.position + (Vector3)step.screenOffset;
            return;
        }

        Vector3 anchoredWorldPosition = target.position + GetAnchorWorldOffset(step.anchor) * step.anchorDistance;
        panelRectTransform.position = Camera.main.WorldToScreenPoint(anchoredWorldPosition);
    }

    // Pieces are spawned at runtime, so a specific piece can't be dragged into Target Transform
    // ahead of time - Target Piece Data finds whichever active piece is using that data instead.
    private Transform ResolveTarget(TutorialStep step)
    {
        if (step.targetPieceData != null && spawnManager != null)
        {
            ChessPiece piece = spawnManager.activePieces.FirstOrDefault(p => p.pieceData == step.targetPieceData);
            if (piece != null) return piece.transform;
        }
        return step.targetTransform;
    }

    private static Vector3 GetAnchorWorldOffset(TutorialAnchor anchor) => anchor switch
    {
        TutorialAnchor.Top => new Vector3(0f, 0f, 1f),
        TutorialAnchor.Bottom => new Vector3(0f, 0f, -1f),
        TutorialAnchor.Left => new Vector3(-1f, 0f, 0f),
        TutorialAnchor.Right => new Vector3(1f, 0f, 0f),
        TutorialAnchor.TopLeft => new Vector3(-1f, 0f, 1f),
        TutorialAnchor.TopRight => new Vector3(1f, 0f, 1f),
        TutorialAnchor.BottomLeft => new Vector3(-1f, 0f, -1f),
        TutorialAnchor.BottomRight => new Vector3(1f, 0f, -1f),
        _ => Vector3.zero
    };

    private void OnNextPressed()
    {
        if (currentStepIndex < 0 || currentStepIndex >= steps.Count) return;
        // Auto-wait steps hide this button entirely and start their wait from ShowStep instead -
        // this guard only remains as a safety net against a stray leftover space press.
        if (isWaitingForTurnSettlement || pendingWait != TutorialStepAction.None) return;

        TutorialStep step = steps[currentStepIndex];

        if (panelRoot != null) panelRoot.SetActive(false);
        if (gameManager != null) gameManager.isTutorialPaused = false;

        switch (step.action)
        {
            case TutorialStepAction.AdvanceTurn:
                gameManager?.AdvanceTurn();
                break;
            case TutorialStepAction.EndTutorial:
                StartCoroutine(EndTutorial());
                return;
        }

        ShowStep(currentStepIndex + 1);
    }

    private void OnSkipPressed()
    {
        StartCoroutine(EndTutorial());
    }

    // Reposition Existing = true means "move this piece IF it's still alive" - if the player
    // already killed it, this entry does nothing at all. It never spawns a replacement, so a
    // captured piece never comes back from a reposition command.
    private void ApplyForcedPiece(ForcedPiece forced)
    {
        if (spawnManager == null || gameManager == null || forced.pieceData == null) return;

        if (forced.repositionExisting)
        {
            ChessPiece existing = spawnManager.activePieces.FirstOrDefault(piece => piece.pieceData == forced.pieceData);
            if (existing != null)
            {
                existing.gridPos = forced.position;
                existing.transform.position = ChessBoardUtility.GridToWorld(forced.position);
                ChessBoardUtility.ApplySortingOrder(existing.GetComponentInChildren<SpriteRenderer>(), forced.position, isPlayer: false);
            }
            return;
        }

        spawnManager.SpawnPiece(forced.pieceData, forced.position, gameManager.currentTurn);
    }

    private void BeginWaitForTurnEnd()
    {
        isWaitingForTurnSettlement = true;
        hasSeenSettlementStart = false;
    }

    private void BeginSafeZoneCheckpoint(Vector2Int targetPosition)
    {
        if (gameManager == null) return;

        currentSafeZoneTarget = targetPosition;
        gameManager.isTutorialTurnEndBlocked = true;
        pendingWait = TutorialStepAction.WaitForSafeZoneArrival;

        if (boardManager != null && boardManager.TryGetTile(targetPosition, out BoardTileVisual tile))
            tile.SetSafeZoneOverlay(safeZoneSprite);
    }

    // Re-checked every single frame (not just once on arrival) - space only works while the
    // player is standing exactly on the tile right now, and re-blocks the instant they step off.
    private void PollSafeZone()
    {
        if (gameManager == null || controlManager == null) return;

        bool onSafeZone = controlManager.GetPlayerGridPosition() == currentSafeZoneTarget;
        gameManager.isTutorialTurnEndBlocked = !onSafeZone;

        if (!gameManager.isSettling) return;

        // Space was pressed while standing on the tile - the turn is genuinely ending now.
        pendingWait = TutorialStepAction.None;
        isWaitingForTurnSettlement = true;
        hasSeenSettlementStart = true;
        ClearSafeZoneOverlay();
    }

    private void ClearSafeZoneOverlay()
    {
        if (boardManager != null && boardManager.TryGetTile(currentSafeZoneTarget, out BoardTileVisual tile))
            tile.ClearSafeZoneOverlay();
    }

    private void PollBuffPickup()
    {
        if (playerPiece == null || !playerPiece.isBuffActive) return;
        pendingWait = TutorialStepAction.None;
        ShowStep(currentStepIndex + 1);
    }

    private void PollTransformPickup()
    {
        if (playerPiece == null || !playerPiece.isTransformActive) return;
        pendingWait = TutorialStepAction.None;
        ShowStep(currentStepIndex + 1);
    }

    private void PollTurnSettlement()
    {
        if (gameManager == null) return;

        if (!hasSeenSettlementStart)
        {
            if (gameManager.isSettling)
            {
                hasSeenSettlementStart = true;
                ClearSafeZoneOverlay();
            }
            return;
        }

        if (gameManager.isSettling) return;

        // Settlement finished: the turn actually ended, so the checkpoint is complete.
        isWaitingForTurnSettlement = false;
        ShowStep(currentStepIndex + 1);
    }

    // Clears any remaining black pieces with the normal death-fling animation, waits for it to
    // finish, then leaves for the lobby - so the tutorial closes on the same "victory" beat a
    // real turn-clear would.
    private IEnumerator EndTutorial()
    {
        if (pendingWait == TutorialStepAction.WaitForSafeZoneArrival) ClearSafeZoneOverlay();

        if (gameManager != null)
        {
            gameManager.isTutorialPaused = false;
            gameManager.isTutorialTurnEndBlocked = false;
            gameManager.isTutorialTimerFrozen = false;
            gameManager.isTutorialBuffTimerFrozen = false;
            gameManager.isTutorialTransformTimerFrozen = false;

            yield return StartCoroutine(gameManager.PlayFinaleDeathSequence());

            gameManager.isTutorialActive = false;
        }

        SceneManager.LoadScene(nextSceneName);
    }
}
