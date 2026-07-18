using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private const int TurnClearScore = 30;

    public static GameManager Instance { get; private set; }

    [Header("ScriptableObject data")]
    [SerializeField] private PlayerPieceSO playerData;

    [Header("Game state")]
    public int currentStage = 1;
    public int currentTurn = 1;
    public int playerScore;
    public int defeatedBlackPieceCount;
    public float turnTimer = 10f;
    public float maxTurnTime = 10f;
    public bool isSettling;
    public bool isGameOver;
    // Set by CommonUIManager while the settings popup is open.
    public bool isPaused;
    // Set by TutorialManager while an explanation panel is on screen.
    public bool isTutorialPaused;
    // Set by TutorialManager for the whole tutorial session; suppresses automatic white-piece spawns.
    public bool isTutorialActive;
    // Set by TutorialManager while waiting for the player to reach a safe tile before the turn may end.
    public bool isTutorialTurnEndBlocked;
    // Set by TutorialManager for whichever turns should feel untimed (the turn timer stops decreasing).
    public bool isTutorialTimerFrozen;
    // Set by TutorialManager for whichever turns the player's buff duration should not tick down.
    public bool isTutorialBuffTimerFrozen;
    // Set by TutorialManager for whichever turns the player's transform duration should not tick down.
    public bool isTutorialTransformTimerFrozen;
    // Separate from isSettling: this unlocks partway through settlement (once black pieces start
    // their reposition jump) so the player can already move while that animation finishes.
    public bool isMovementLocked;

    // Result of the most recently resolved turn, used by the avatar UI.
    // Predicted the instant the turn ends (see PreviewTurnResult) so the avatar reacts immediately, before the settlement animation.
    public bool LastTurnAttackHitEnemy { get; private set; }
    public bool LastTurnPlayerWasHit { get; private set; }
    public bool LastTurnFatal { get; private set; }
    public float LastSettlementTime { get; private set; } = float.NegativeInfinity;

    [Header("Settlement animation")]
    [Tooltip("Time one piece's jump takes, in seconds.")]
    [SerializeField, Min(0.05f)] private float jumpDuration = 0.4f;
    [Tooltip("World-unit height added along Y during a jump (perspective 'pop').")]
    [SerializeField, Min(0f)] private float jumpHeightY = 0.32f;
    [Tooltip("World-unit height added along Z during a jump (the axis that actually reads as 'up' on a top-down camera).")]
    [SerializeField, Min(0f)] private float jumpHeightZ = 0.45f;
    [Tooltip("Height of the jump arc over time: 0 at start/end, 1 at the peak. Edit the curve directly to change the weight/inertia feel.")]
    [SerializeField] private AnimationCurve jumpCurve = CreateDefaultJumpCurve();
    [Tooltip("Vertical squash multiplier over time: 1 = normal size, below 1 = squashed flat, above 1 = stretched tall. The default dips before liftoff and squashes again on landing.")]
    [SerializeField] private AnimationCurve squashCurve = CreateDefaultSquashCurve();
    [Tooltip("How much width compensates for the vertical squash (0 = none, 1 = fully volume-preserving).")]
    [SerializeField, Range(0f, 1f)] private float squashSideInfluence = 0.6f;
    [Tooltip("Total time spread across which every piece's jump start is staggered, top-left to bottom-right.")]
    [SerializeField, Min(0f)] private float jumpStaggerWindow = 0.2f;
    [Tooltip("Seconds between each ring of a piece's attack-range reveal, once that piece lands. A queen's range takes longer to fully reveal than a pawn's.")]
    [SerializeField, Min(0.01f)] private float revealRingInterval = 0.05f;
    [Tooltip("How long each individual ring of tiles stays lit, counted from the moment that specific ring appeared. Every ring - from every piece - fades out on its own schedule.")]
    [SerializeField, Min(0f)] private float revealLingerDuration = 0.2f;

    [Header("Hit reaction (survived)")]
    [Tooltip("Grid-unit distance pushed away from the attacker before snapping back.")]
    [SerializeField, Min(0f)] private float knockbackDistance = 0.25f;
    [SerializeField, Min(0.01f)] private float knockbackDuration = 0.18f;
    [Tooltip("0 = at rest, 1 = fully pushed out. Default snaps out fast, drifts back slowly, then snaps to rest fast at the very end (inertia).")]
    [SerializeField] private AnimationCurve knockbackCurve = CreateDefaultKnockbackCurve();
    [Tooltip("Peak tilt angle in degrees (multiplied by the wobble curve below).")]
    [SerializeField, Min(0f)] private float knockbackTiltDegrees = 12f;
    [Tooltip("Local axis to tilt around. Default assumes the sprite's own local Z reads as a screen-plane tilt - change this if the wobble looks wrong for how these prefabs are rotated.")]
    [SerializeField] private Vector3 knockbackTiltAxis = Vector3.forward;
    [Tooltip("Independent from the push curve above: a damped back-and-forth wobble (roly-poly toy), settling to 0 by the end.")]
    [SerializeField] private AnimationCurve knockbackTiltCurve = CreateDefaultKnockbackTiltCurve();
    [Tooltip("Color the player's sprite blinks to when hit (a plain color swap, not a different sprite).")]
    [SerializeField] private Color playerHitFlashColor = Color.red;
    [Tooltip("Color a black piece's sprite blinks to when hit.")]
    [SerializeField] private Color blackHitFlashColor = Color.yellow;
    [Tooltip("How many on/off blinks happen over Hit Flash Duration below. Independent from the knockback motion, so it can run longer than the knockback itself.")]
    [SerializeField, Min(1)] private int hitFlashBlinkCount = 2;
    [Tooltip("Total time the flash keeps blinking, in seconds.")]
    [SerializeField, Min(0.01f)] private float hitFlashDuration = 0.5f;

    [Header("Death animation")]
    [Tooltip("World-unit distance flown off screen, away from whatever killed it. Wide/far so the arc below reads as a gentle ridge, not a sharp spike.")]
    [SerializeField, Min(0f)] private float deathFallDistance = 16f;
    [SerializeField, Min(0.01f)] private float deathFallDuration = 0.45f;
    [Tooltip("0 at the instant it dies, 1 at the far end of the flight. Default bursts out fast, then gradually slows to a stop.")]
    [SerializeField] private AnimationCurve deathFallCurve = CreateDefaultDeathFallCurve();
    [Tooltip("Total degrees spun while flying off - a big value reads as a dramatic tumble.")]
    [SerializeField] private float deathSpinDegrees = 720f;
    [Tooltip("Local axis to spin around while flying off.")]
    [SerializeField] private Vector3 deathSpinAxis = Vector3.forward;
    [Header("Death animation - arc (a true parabola through these 3 points, always smooth)")]
    [Tooltip("How far through the flight (0-1) the arc reaches its peak. Same timing is used for both the Y and Z parabola below.")]
    [SerializeField, Range(0.05f, 0.95f)] private float deathArcPeakTime = 0.4f;
    [Tooltip("World-unit Y height at the peak (starts and ends at 0).")]
    [SerializeField, Min(0f)] private float deathArcHeight = 1.2f;
    [Tooltip("Z value at the peak (this camera's 'reads as up' axis).")]
    [SerializeField] private float deathArcPeakZ = 2f;
    [Tooltip("Z value once it's fully gone (this camera's 'reads as down/away' axis) - starts at 0.")]
    [SerializeField] private float deathArcEndZ = -2f;

    private Vector2Int lastPlayerAttackerGrid;
    private AchievementSO.TurnEndMethod lastTurnEndMethod;
    private bool playerWasHitThisTurn;

    private SpawnManager spawnManager;
    private ControlManager controlManager;
    private BoardViewManager boardViewManager;
    private PlayerPiece playerPiece;
    private GameOverPopup gameOverPopup;
    private bool forceQueenNextSpawn;
    private int turnsWithoutQueen;
    private int forceKnightTransformTurns;
    private readonly Dictionary<ChessPieceType, int> defeatedCountByType = new();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
    }

    private void Start()
    {
        spawnManager = FindFirstObjectByType<SpawnManager>();
        controlManager = FindFirstObjectByType<ControlManager>();
        boardViewManager = FindFirstObjectByType<BoardViewManager>();
        playerPiece = FindFirstObjectByType<PlayerPiece>();
        gameOverPopup = FindFirstObjectByType<GameOverPopup>();
        if (spawnManager == null || controlManager == null || playerPiece == null)
        {
            Debug.LogError("GameManager requires SpawnManager, ControlManager, and PlayerPiece in the scene.");
            enabled = false;
            return;
        }

        if (playerData != null) playerPiece.pieceData = playerData;
        controlManager.SetupPlayer(playerPiece, 4, 0);
        StartStage(1);
        spawnManager.SpawnInitialBlackPieces(controlManager.GetPlayerGridPosition(), currentTurn);
    }

    private void Update()
    {
        if (isPaused || isSettling || isGameOver || isTutorialPaused) return;

        // Frozen only stops the countdown itself, and turn-end-blocked only stops the space
        // shortcut - neither should stop the other, or a "wait for the timer, no space" step
        // (or an untimed "wait for space, no timer" step) would get stuck with nothing able to end it.
        if (!isTutorialTimerFrozen) turnTimer -= Time.deltaTime;
        bool spaceEndsTurn = !isTutorialTurnEndBlocked && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame;
        if (turnTimer <= 0f || spaceEndsTurn)
        {
            lastTurnEndMethod = spaceEndsTurn ? AchievementSO.TurnEndMethod.Spacebar : AchievementSO.TurnEndMethod.TimeOut;
            StartSettlement();
        }
    }

    public void OnPlayerMoved(int x, int z)
    {
        if (isSettling || isGameOver) return;
        ChessPiece pickedPiece = spawnManager.GetPieceAt(new Vector2Int(x, z));
        if (pickedPiece == null || pickedPiece is BlackEnemyPiece || !(pickedPiece.pieceData is WhitePieceSO)) return;

        if (pickedPiece is WhiteBuffPiece buff)
            playerPiece.ApplyBuff(buff.duration, buff.attackBuffPower);
        else if (pickedPiece is WhiteTransformPiece transform)
            playerPiece.ApplyTransform(transform.duration, transform.pieceData);
        else return;

        spawnManager.RemovePiece(pickedPiece);
    }

    private void StartSettlement()
    {
        if (isSettling || isGameOver) return;
        controlManager.SnapToGrid();
        isSettling = true;
        isMovementLocked = true;
        PreviewTurnResult();
        StartCoroutine(SettlementRoutine());
    }

    // Board positions are frozen for the rest of the turn once settlement starts,
    // so the outcome can be predicted immediately for the avatar UI instead of waiting for the settlement animation.
    private void PreviewTurnResult()
    {
        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();

        bool wasHit = spawnManager.activePieces
            .OfType<BlackEnemyPiece>()
            .Any(enemy => ChessAttackResolver.GetAttackCells(enemy.pieceData, enemy.gridPos).Contains(playerPosition));

        HashSet<Vector2Int> targets = ChessAttackResolver.GetAttackCells(playerPiece.CurrentAttackData, playerPosition);
        bool attackedEnemy = spawnManager.activePieces.OfType<BlackEnemyPiece>().Any(enemy => targets.Contains(enemy.gridPos));

        LastTurnPlayerWasHit = wasHit;
        LastTurnAttackHitEnemy = attackedEnemy;
        LastTurnFatal = wasHit && playerPiece.hp <= 1;
        LastSettlementTime = Time.time;
    }

    private IEnumerator SettlementRoutine()
    {
        // 1. Each piece jumps for its attack; the instant it lands, its own range starts
        // spreading outward ring by ring (no instant full-range flash).
        yield return StartCoroutine(PlayAttackJumpPhase(GetSettlingPieces()));

        // 2. Black attacks resolve first, then the player's.
        yield return StartCoroutine(ResolveBlackAttacks());
        if (playerPiece.hp <= 0)
        {
            Vector2Int playerGrid = controlManager.GetPlayerGridPosition();
            yield return StartCoroutine(AnimateDeath(playerPiece.transform, playerGrid, lastPlayerAttackerGrid, useLocalPosition: true, null));
            GameOver();
            yield break;
        }

        yield return StartCoroutine(ResolvePlayerAttack());
        playerScore += TurnClearScore;

        // 3. Surviving black pieces jump again to their new positions. The player can already move
        // once this phase starts, so their position may change before it's done.
        yield return StartCoroutine(PlayRepositionPhase(controlManager.GetPlayerGridPosition()));

        // 4. Settlement itself is over - the turn's outcome is fully resolved and on screen.
        // Normal play immediately advances to the next turn; the tutorial holds here so it can
        // show "turn just ended" dialogue before currentTurn actually increments, then advances
        // on its own via a call to AdvanceTurn() once the player is ready.
        EndSettlement();
        if (!isTutorialActive)
        {
            ReportMilestoneAchievements();
            AdvanceTurn();
        }
    }

    private void EndSettlement()
    {
        isSettling = false;
    }

    private List<ChessPiece> GetSettlingPieces()
    {
        List<ChessPiece> pieces = new() { playerPiece };
        pieces.AddRange(spawnManager.activePieces.OfType<BlackEnemyPiece>());
        return pieces;
    }

    private IEnumerator PlayAttackJumpPhase(List<ChessPiece> pieces)
    {
        List<ChessPiece> ordered = pieces
            .Where(piece => piece != null)
            .OrderBy(piece => ChessBoardUtility.GetSortingOrder(piece.gridPos, piece == playerPiece))
            .ToList();

        float step = ordered.Count > 1 ? jumpStaggerWindow / (ordered.Count - 1) : 0f;
        float longestFinish = 0f;
        for (int i = 0; i < ordered.Count; i++)
        {
            ChessPiece piece = ordered[i];
            bool isPlayerPiece = piece == playerPiece;
            float delay = i * step;

            ChessPieceSO attackData = isPlayerPiece ? playerPiece.CurrentAttackData : piece.pieceData;
            // Only wait for the jump itself to land - the ring-by-ring range reveal keeps playing
            // on its own independent schedule and doesn't need to finish before hit reactions can start.
            longestFinish = Mathf.Max(longestFinish, delay + jumpDuration);

            StartCoroutine(JumpThenReveal(piece, delay, isPlayerPiece, attackData));
        }
        yield return new WaitForSeconds(longestFinish);
    }

    // Jumps the piece in place, then the instant it lands, starts that piece's own ring-by-ring range reveal.
    private IEnumerator JumpThenReveal(ChessPiece piece, float delay, bool isPlayerPiece, ChessPieceSO attackData)
    {
        Vector3 basePosition = isPlayerPiece ? piece.transform.localPosition : piece.transform.position;
        yield return StartCoroutine(AnimateJump(piece.transform, basePosition, basePosition, delay, isPlayerPiece));
        if (piece == null) yield break;
        boardViewManager?.RegisterReveal(piece.gridPos, attackData, isPlayerPiece, revealRingInterval, revealLingerDuration);
    }

    private IEnumerator PlayRepositionPhase(Vector2Int playerPosition)
    {
        // From here on the player can walk around again; only the settlement/turn-timer stays locked
        // (via isSettling) until EndSettlement runs, so a new settlement can't start mid-reposition.
        isMovementLocked = false;

        List<BlackEnemyPiece> survivors = spawnManager.activePieces.OfType<BlackEnemyPiece>().ToList();
        Dictionary<BlackEnemyPiece, Vector3> oldPositions = survivors.ToDictionary(piece => piece, piece => piece.transform.position);

        // The tutorial places every black piece by hand for determinism (safe zones, forced targets),
        // so the normal random reposition is skipped for the whole tutorial session.
        if (!isTutorialActive) spawnManager.RepositionBlackPieces(playerPosition);

        List<BlackEnemyPiece> ordered = survivors
            .Where(piece => piece != null)
            .OrderBy(piece => ChessBoardUtility.GetSortingOrder(piece.gridPos, isPlayer: false))
            .ToList();

        float step = ordered.Count > 1 ? jumpStaggerWindow / (ordered.Count - 1) : 0f;
        for (int i = 0; i < ordered.Count; i++)
        {
            BlackEnemyPiece piece = ordered[i];
            Vector3 newPosition = piece.transform.position;
            Vector3 oldPosition = oldPositions[piece];
            piece.transform.position = oldPosition;
            StartCoroutine(AnimateJump(piece.transform, oldPosition, newPosition, i * step, useLocalPosition: false));
        }
        yield return new WaitForSeconds(jumpStaggerWindow + jumpDuration);
    }

    private IEnumerator AnimateJump(Transform pieceTransform, Vector3 fromPosition, Vector3 toPosition, float delay, bool useLocalPosition)
    {
        if (pieceTransform == null) yield break;
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (pieceTransform == null) yield break;

        Vector3 baseScale = pieceTransform.localScale;
        float spriteHalfHeight = PieceSquashUtility.GetSpriteHalfHeight(pieceTransform);

        float elapsed = 0f;
        while (elapsed < jumpDuration)
        {
            if (pieceTransform == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / jumpDuration);

            float arc = jumpCurve.Evaluate(t);
            Vector3 groundPosition = Vector3.Lerp(fromPosition, toPosition, t);
            Vector3 jumpOffset = new(0f, jumpHeightY * arc, jumpHeightZ * arc);

            float squashY = squashCurve.Evaluate(t);
            float squashXZ = PieceSquashUtility.GetSquashSideScale(squashY, squashSideInfluence);
            Vector3 anchorOffset = PieceSquashUtility.GetGroundAnchorOffset(pieceTransform, spriteHalfHeight, baseScale.y, squashY);

            SetPosition(pieceTransform, groundPosition + jumpOffset + anchorOffset, useLocalPosition);
            pieceTransform.localScale = new Vector3(baseScale.x * squashXZ, baseScale.y * squashY, baseScale.z);
            yield return null;
        }

        if (pieceTransform != null)
        {
            SetPosition(pieceTransform, toPosition, useLocalPosition);
            pieceTransform.localScale = baseScale;
        }
    }

    private static void SetPosition(Transform pieceTransform, Vector3 position, bool useLocalPosition)
    {
        if (useLocalPosition) pieceTransform.localPosition = position;
        else pieceTransform.position = position;
    }

    // Barely moves at first (squash anticipation), bursts up fast, hangs near the peak, then falls fast.
    private static AnimationCurve CreateDefaultJumpCurve()
    {
        AnimationCurve curve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.1f, 0.05f),
            new Keyframe(0.35f, 1f),
            new Keyframe(0.7f, 0.95f),
            new Keyframe(1f, 0f));
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        return curve;
    }

    // 1 = normal. Dips down (squash) just before liftoff, springs slightly tall on the way up,
    // stays normal through the hang, stretches slightly on the fall, then squashes hard on landing.
    private static AnimationCurve CreateDefaultSquashCurve()
    {
        AnimationCurve curve = new(
            new Keyframe(0f, 1f),
            new Keyframe(0.06f, 0.8f),
            new Keyframe(0.2f, 1.08f),
            new Keyframe(0.5f, 1f),
            new Keyframe(0.85f, 1.05f),
            new Keyframe(0.95f, 0.8f),
            new Keyframe(1f, 1f));
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        return curve;
    }

    // 0 at rest, snaps out to 1 fast, drifts back slowly through most of the duration,
    // then snaps back to 0 fast right at the end - an inertia/whiplash feel.
    private static AnimationCurve CreateDefaultKnockbackCurve()
    {
        AnimationCurve curve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.12f, 1f),
            new Keyframe(0.75f, 0.35f),
            new Keyframe(1f, 0f));
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        return curve;
    }

    // Tips over hard, swings back past center, then rocks a couple more times with shrinking
    // amplitude before settling flat - a damped spring, like a roly-poly toy losing momentum.
    private static AnimationCurve CreateDefaultKnockbackTiltCurve()
    {
        AnimationCurve curve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.15f, 1f),
            new Keyframe(0.4f, -0.5f),
            new Keyframe(0.6f, 0.25f),
            new Keyframe(0.8f, -0.1f),
            new Keyframe(1f, 0f));
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        return curve;
    }

    // Bursts out fast right away, then gradually slows to a stop - shot out, then coasting.
    private static AnimationCurve CreateDefaultDeathFallCurve()
    {
        AnimationCurve curve = new(
            new Keyframe(0f, 0f),
            new Keyframe(0.2f, 0.75f),
            new Keyframe(1f, 1f));
        for (int i = 0; i < curve.length; i++) curve.SmoothTangents(i, 0f);
        return curve;
    }

    // The one true quadratic (parabola) passing through (0, start), (peakT, peak), (1, end).
    // Always perfectly smooth - no keyframe/tangent guesswork, unlike an authored AnimationCurve.
    private static float EvaluateParabola(float t, float start, float peak, float end, float peakT)
    {
        float a = (peak - start - (end - start) * peakT) / (peakT * peakT - peakT);
        float b = (end - start) - a;
        return a * t * t + b * t + start;
    }

    private IEnumerator ResolveBlackAttacks()
    {
        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();
        List<BlackEnemyPiece> attackers = spawnManager.activePieces
            .OfType<BlackEnemyPiece>()
            .Where(enemy => ChessAttackResolver.GetAttackCells(enemy.pieceData, enemy.gridPos).Contains(playerPosition))
            .ToList();

        playerWasHitThisTurn = attackers.Count > 0;
        if (!playerWasHitThisTurn) yield break;

        // Even if multiple enemies cover the player, damage is applied only once per turn.
        playerPiece.TakeDamage();

        BlackEnemyPiece attacker = SelectKnockbackAttacker(playerPosition, attackers);
        lastPlayerAttackerGrid = attacker.gridPos;
        if (playerPiece.hp <= 0) yield break; // SettlementRoutine plays the death animation instead of a knockback.

        yield return StartCoroutine(AnimateKnockback(playerPiece.transform, playerPosition, attacker.gridPos, useLocalPosition: true, playerHitFlashColor));
    }

    private IEnumerator ResolvePlayerAttack()
    {
        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();
        HashSet<Vector2Int> targets = ChessAttackResolver.GetAttackCells(playerPiece.CurrentAttackData, playerPosition);

        // Damage is applied to every hit enemy up front so the total kill count for this single
        // attack is known (needed for "kill N in one attack" achievements) before any of the
        // per-enemy handling below runs.
        List<(BlackEnemyPiece enemy, bool died)> hits = spawnManager.activePieces.OfType<BlackEnemyPiece>()
            .ToArray()
            .Where(enemy => targets.Contains(enemy.gridPos))
            .Select(enemy => (enemy, died: enemy.TakeDamage(playerPiece.atk)))
            .ToList();

        int killCountThisAttack = hits.Count(hit => hit.died);
        AchievementSO.PlayerFormFlags playerForm = ToPlayerFormFlag(playerPiece.CurrentAttackData.pieceType);
        bool pawnBuffActive = playerPiece.isBuffActive;

        // Knockbacks must finish before the reposition phase starts, or both animations would
        // fight over the same piece's position (same bug class as the player-move/attack race).
        List<Coroutine> knockbacks = new();

        foreach ((BlackEnemyPiece enemy, bool died) in hits)
        {
            ReportAttackAchievements(enemy.PieceType, died, killCountThisAttack, playerForm, pawnBuffActive);

            if (!died)
            {
                knockbacks.Add(StartCoroutine(AnimateKnockback(enemy.transform, enemy.gridPos, playerPosition, useLocalPosition: false, blackHitFlashColor)));
                continue;
            }

            playerScore += enemy.scoreValue;
            defeatedBlackPieceCount++;
            defeatedCountByType[enemy.PieceType] = defeatedCountByType.GetValueOrDefault(enemy.PieceType) + 1;
            if (enemy.PieceType == ChessPieceType.King)
                playerPiece.RestoreHp(1);

            // Remove it from play immediately (so scoring/reposition never see it again), but let
            // it visually finish dying before the GameObject itself is destroyed.
            spawnManager.activePieces.Remove(enemy);
            StartCoroutine(AnimateDeath(enemy.transform, enemy.gridPos, playerPosition, useLocalPosition: false, () =>
            {
                if (enemy != null) Destroy(enemy.gameObject);
            }));
        }

        foreach (Coroutine knockback in knockbacks)
            yield return knockback;
    }

    private void ReportAttackAchievements(ChessPieceType targetType, bool killed, int killCountThisAttack, AchievementSO.PlayerFormFlags playerForm, bool pawnBuffActive)
    {
        if (isTutorialActive) return;
        AchievementManager achievementManager = GlobalManager.Instance != null ? GlobalManager.Instance.AchievementManager : null;
        if (achievementManager == null) return;

        AchievementEventContext context = new()
        {
            playerForm = playerForm,
            pawnBuffActive = pawnBuffActive,
            targetPieceType = ToPieceTypeFlag(targetType),
            killCountInThisAttack = killCountThisAttack,
            playerDamagedSimultaneously = playerWasHitThisTurn
        };

        if (killed)
        {
            context.eventType = AchievementSO.EventType.Kill;
            achievementManager.ReportEvent(context);
        }

        context.eventType = AchievementSO.EventType.DamageDealt;
        achievementManager.ReportEvent(context);
    }

    private void ReportMilestoneAchievements()
    {
        AchievementManager achievementManager = GlobalManager.Instance != null ? GlobalManager.Instance.AchievementManager : null;
        if (achievementManager == null) return;

        achievementManager.ReportEvent(new AchievementEventContext
        {
            eventType = AchievementSO.EventType.Milestone,
            stage = currentStage,
            turn = currentTurn,
            score = playerScore,
            turnEndMethod = lastTurnEndMethod,
            remainingTime = turnTimer,
            boardBlackPieceCount = spawnManager.BlackPieceCount,
            boardCleared = spawnManager.BlackPieceCount == 0
        });
    }

    private static AchievementSO.PlayerFormFlags ToPlayerFormFlag(ChessPieceType type) => type switch
    {
        ChessPieceType.King => AchievementSO.PlayerFormFlags.King,
        ChessPieceType.Knight => AchievementSO.PlayerFormFlags.Knight,
        ChessPieceType.Bishop => AchievementSO.PlayerFormFlags.Bishop,
        ChessPieceType.Rook => AchievementSO.PlayerFormFlags.Rook,
        ChessPieceType.Queen => AchievementSO.PlayerFormFlags.Queen,
        _ => 0
    };

    private static AchievementSO.PieceTypeFlags ToPieceTypeFlag(ChessPieceType type) => type switch
    {
        ChessPieceType.Pawn => AchievementSO.PieceTypeFlags.Pawn,
        ChessPieceType.Knight => AchievementSO.PieceTypeFlags.Knight,
        ChessPieceType.Bishop => AchievementSO.PieceTypeFlags.Bishop,
        ChessPieceType.Rook => AchievementSO.PieceTypeFlags.Rook,
        ChessPieceType.Queen => AchievementSO.PieceTypeFlags.Queen,
        ChessPieceType.King => AchievementSO.PieceTypeFlags.King,
        _ => 0
    };

    // Among the enemies currently hitting the player, picks which one's position determines the
    // knockback direction: closest first, then the same "further down and further right" priority
    // already used for screen draw order.
    private static BlackEnemyPiece SelectKnockbackAttacker(Vector2Int playerPosition, List<BlackEnemyPiece> attackers)
    {
        return attackers
            .OrderBy(enemy => ChebyshevDistance(enemy.gridPos, playerPosition))
            .ThenByDescending(enemy => ChessBoardUtility.GetSortingOrder(enemy.gridPos, isPlayer: false))
            .First();
    }

    private static int ChebyshevDistance(Vector2Int a, Vector2Int b) =>
        Mathf.Max(Mathf.Abs(a.x - b.x), Mathf.Abs(a.y - b.y));

    // Mathf.Sign(0) returns 1, not 0 - this gives the correct "no push on this axis" result instead.
    private static float SignOrZero(int value) => value == 0 ? 0f : Mathf.Sign(value);

    // Pushes away from the attacker's tile - straight if the attacker is orthogonal, diagonal if the attacker is diagonal - then eases back to the start position.
    // The tilt rocks around the sprite's own bottom-center (like a roly-poly toy), not its transform origin.
    private IEnumerator AnimateKnockback(Transform pieceTransform, Vector2Int selfGrid, Vector2Int attackerGrid, bool useLocalPosition, Color flashColor)
    {
        if (pieceTransform == null) yield break;

        Vector2Int delta = selfGrid - attackerGrid;
        Vector3 direction = new(SignOrZero(delta.x), 0f, SignOrZero(delta.y));
        Vector3 basePosition = useLocalPosition ? pieceTransform.localPosition : pieceTransform.position;
        Vector3 pushedOffset = direction * knockbackDistance;
        Quaternion baseRotation = pieceTransform.localRotation;

        // Pivot for the tilt: straight down from the sprite's own origin to its bottom edge (nine-slice
        // anchor 8 / bottom-center), so it rocks on its "feet" instead of spinning around its middle.
        float spriteHalfHeight = PieceSquashUtility.GetSpriteHalfHeight(pieceTransform);
        Vector3 basePivotOffset = new(0f, -spriteHalfHeight, 0f);

        // Tilt one way or the other depending on which side the push comes from.
        float tiltSign = direction.x != 0f ? direction.x : direction.z;
        if (tiltSign == 0f) tiltSign = 1f;

        SpriteRenderer spriteRenderer = pieceTransform.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) StartCoroutine(AnimateHitFlash(spriteRenderer, flashColor));

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            if (pieceTransform == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / knockbackDuration);
            float k = knockbackCurve.Evaluate(t);
            float tilt = knockbackTiltCurve.Evaluate(t);

            Vector3 slidPosition = basePosition + pushedOffset * k;
            Quaternion tiltedRotation = baseRotation * Quaternion.AngleAxis(knockbackTiltDegrees * tilt * tiltSign, knockbackTiltAxis);

            // Keep the bottom pivot point fixed at the (untilted) slid position while the sprite rocks around it.
            Vector3 pivotPoint = slidPosition + baseRotation * basePivotOffset;
            Vector3 tiltedPosition = pivotPoint - tiltedRotation * basePivotOffset;

            SetPosition(pieceTransform, tiltedPosition, useLocalPosition);
            pieceTransform.localRotation = tiltedRotation;
            yield return null;
        }

        if (pieceTransform != null)
        {
            SetPosition(pieceTransform, basePosition, useLocalPosition);
            pieceTransform.localRotation = baseRotation;
        }
    }

    // Blinks between the sprite's own color and flashColor. Runs on its own clock (Hit Flash
    // Duration), independent of the knockback motion, so it can keep blinking after the knockback settles.
    private IEnumerator AnimateHitFlash(SpriteRenderer spriteRenderer, Color flashColor)
    {
        if (spriteRenderer == null) yield break;
        Color originalColor = spriteRenderer.color;
        float cycleDuration = hitFlashDuration / (hitFlashBlinkCount * 2f);

        float elapsed = 0f;
        while (elapsed < hitFlashDuration)
        {
            if (spriteRenderer == null) yield break;
            elapsed += Time.deltaTime;
            bool showFlash = Mathf.FloorToInt(elapsed / cycleDuration) % 2 == 0;
            spriteRenderer.color = showFlash ? flashColor : originalColor;
            yield return null;
        }

        if (spriteRenderer != null) spriteRenderer.color = originalColor;
    }

    // Fires off instantly (no wind-up) in the direction away from whatever killed it, arcing
    // through both Y and Z as it flies, spinning hard, decelerating toward the end of the flight -
    // then invokes onComplete (destroy, game over, etc).
    private IEnumerator AnimateDeath(Transform pieceTransform, Vector2Int selfGrid, Vector2Int attackerGrid, bool useLocalPosition, System.Action onComplete)
    {
        if (pieceTransform == null) { onComplete?.Invoke(); yield break; }

        Vector3 basePosition = useLocalPosition ? pieceTransform.localPosition : pieceTransform.position;
        Quaternion baseRotation = pieceTransform.localRotation;

        // Away from the attacker's tile, same straight/diagonal logic as the knockback push.
        Vector2Int delta = selfGrid - attackerGrid;
        Vector3 flingDirection = new(SignOrZero(delta.x), 0f, SignOrZero(delta.y));
        if (flingDirection.sqrMagnitude < 0.01f) flingDirection = Vector3.forward;

        float elapsed = 0f;
        while (elapsed < deathFallDuration)
        {
            if (pieceTransform == null) { onComplete?.Invoke(); yield break; }
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / deathFallDuration);
            float progress = deathFallCurve.Evaluate(t);
            float arcY = EvaluateParabola(t, 0f, deathArcHeight, 0f, deathArcPeakTime);
            float arcZ = EvaluateParabola(t, 0f, deathArcPeakZ, deathArcEndZ, deathArcPeakTime);

            Vector3 offset = flingDirection * (progress * deathFallDistance) + new Vector3(0f, arcY, arcZ);
            SetPosition(pieceTransform, basePosition + offset, useLocalPosition);
            pieceTransform.localRotation = baseRotation * Quaternion.AngleAxis(deathSpinDegrees * progress, deathSpinAxis);
            yield return null;
        }

        onComplete?.Invoke();
    }

    // Actually advances the turn count and re-spawns/repositions for the new turn. Normal play
    // calls this immediately after settlement ends; the tutorial calls it itself (see EndSettlement).
    public void AdvanceTurn()
    {
        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();
        int completedTurn = currentTurn;
        currentTurn++;
        // Stage 1 contains turns 1-10; stage 2 starts after turn 10 is cleared.
        bool stageRaised = completedTurn % 10 == 0;
        if (stageRaised)
        {
            StartStage(currentStage + 1);
            forceQueenNextSpawn = true;
        }

        spawnManager.RemoveExpiredWhitePieces(currentTurn);

        bool queenExists = spawnManager.QueenCount > 0;
        turnsWithoutQueen = queenExists ? 0 : turnsWithoutQueen + 1;
        bool mustSpawnQueen = forceQueenNextSpawn || turnsWithoutQueen >= 3;
        // Same reasoning as the reposition skip above: the tutorial spawns every black piece itself.
        if (!isTutorialActive) spawnManager.SpawnBlackPieces(currentStage, playerPosition, currentTurn, mustSpawnQueen);
        forceQueenNextSpawn = false;

        if (spawnManager.QueenCount >= 3) forceKnightTransformTurns = 5;

        if (!isTutorialActive)
        {
            float buffChance = spawnManager.BlackPieceCount >= 6 ? 100f : Mathf.Min(30f, 10f + (currentStage - 1) * 5f);
            spawnManager.TrySpawnBuffPiece(playerPosition, currentTurn, buffChance);

            if (completedTurn % 5 == 0)
            {
                bool forceKnight = forceKnightTransformTurns > 0;
                spawnManager.TrySpawnTransformPiece(playerPosition, currentTurn, forceKnight);
            }
        }
        if (forceKnightTransformTurns > 0) forceKnightTransformTurns--;

        turnTimer = maxTurnTime;
    }

    private void StartStage(int stage)
    {
        currentStage = stage;
        maxTurnTime = Mathf.Max(5f, 10f - (stage - 1));
        turnTimer = maxTurnTime;
    }

    public int GetDefeatedCount(ChessPieceType pieceType) => defeatedCountByType.GetValueOrDefault(pieceType);

    private void GameOver()
    {
        isGameOver = true;
        isSettling = true;
        isPaused = true;
        playerPiece.ClearAllEffects();
        Debug.Log($"Game Over. Final score: {playerScore}");
        gameOverPopup?.Show(playerScore, currentStage, currentTurn);
    }

    // Called by TutorialManager when the tutorial's farewell step ends - clears the board with the
    // same death-fling animation used during normal play, for a clean finale before returning to the lobby.
    public IEnumerator PlayFinaleDeathSequence()
    {
        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();
        List<BlackEnemyPiece> survivors = spawnManager.activePieces.OfType<BlackEnemyPiece>().ToList();

        foreach (BlackEnemyPiece enemy in survivors)
        {
            spawnManager.activePieces.Remove(enemy);
            BlackEnemyPiece capturedEnemy = enemy;
            StartCoroutine(AnimateDeath(enemy.transform, enemy.gridPos, playerPosition, useLocalPosition: false, () =>
            {
                if (capturedEnemy != null) Destroy(capturedEnemy.gameObject);
            }));
        }

        if (survivors.Count > 0)
            yield return new WaitForSeconds(deathFallDuration);
    }
}
