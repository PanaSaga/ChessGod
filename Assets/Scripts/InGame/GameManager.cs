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
    [Tooltip("How long the fully-revealed attack-range tiles stay lit after damage resolves, before fading out. Does not delay the reposition jump, which starts immediately.")]
    [SerializeField, Min(0f)] private float revealLingerDuration = 1f;

    private SpawnManager spawnManager;
    private ControlManager controlManager;
    private BoardViewManager boardViewManager;
    private PlayerPiece playerPiece;
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
        if (isSettling || isGameOver) return;
        turnTimer -= Time.deltaTime;
        if (turnTimer <= 0f || (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame))
            StartSettlement();
    }

    public void OnPlayerMoved(int x, int z)
    {
        if (isSettling || isGameOver) return;
        ChessPiece pickedPiece = spawnManager.GetPieceAt(new Vector2Int(x, z));
        if (pickedPiece == null || pickedPiece is BlackEnemyPiece || !(pickedPiece.pieceData is WhitePieceSO)) return;

        if (pickedPiece is WhiteBuffPiece buff)
            playerPiece.ApplyBuff(buff.buffDuration, buff.attackBuffPower);
        else if (pickedPiece is WhiteTransformPiece transform)
            playerPiece.ApplyTransform(transform.transformDuration, transform.pieceData);
        else return;

        spawnManager.RemovePiece(pickedPiece);
    }

    private void StartSettlement()
    {
        if (isSettling || isGameOver) return;
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
        boardViewManager?.ClearReveals();

        // 1. Each piece jumps for its attack; the instant it lands, its own range starts
        // spreading outward ring by ring (no instant full-range flash).
        yield return StartCoroutine(PlayAttackJumpPhase(GetSettlingPieces()));

        // 2. Black attacks resolve first, then the player's.
        ResolveBlackAttacks();
        if (playerPiece.hp <= 0)
        {
            GameOver();
            yield break;
        }

        ResolvePlayerAttack();
        playerScore += TurnClearScore;

        // The lit tiles linger for a moment on their own; this does not block the reposition
        // jump below, so black pieces can already be jumping while the red tiles fade out.
        StartCoroutine(ClearRevealsAfterDelay(revealLingerDuration));

        // 3. Surviving black pieces jump again to their new positions. The player can already move
        // once this phase starts, so their position may change before it's done.
        yield return StartCoroutine(PlayRepositionPhase(controlManager.GetPlayerGridPosition()));

        // 4. Only now does the next turn actually begin - re-read the player's position in case they moved.
        FinishTurn(controlManager.GetPlayerGridPosition());
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
            int maxRing = ChessAttackResolver.GetMaxRing(attackData, piece.gridPos);
            longestFinish = Mathf.Max(longestFinish, delay + jumpDuration + maxRing * revealRingInterval);

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
        boardViewManager?.RegisterReveal(piece.gridPos, attackData, isPlayerPiece, revealRingInterval);
    }

    private IEnumerator ClearRevealsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        boardViewManager?.ClearReveals();
    }

    private IEnumerator PlayRepositionPhase(Vector2Int playerPosition)
    {
        // From here on the player can walk around again; only the settlement/turn-timer stays locked
        // (via isSettling) until FinishTurn runs, so a new settlement can't start mid-reposition.
        isMovementLocked = false;

        List<BlackEnemyPiece> survivors = spawnManager.activePieces.OfType<BlackEnemyPiece>().ToList();
        Dictionary<BlackEnemyPiece, Vector3> oldPositions = survivors.ToDictionary(piece => piece, piece => piece.transform.position);

        spawnManager.RepositionBlackPieces(playerPosition);

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
        SpriteRenderer spriteRenderer = pieceTransform.GetComponent<SpriteRenderer>();
        // How far the sprite's bottom edge sits from its own pivot, used to keep that edge anchored to the ground while squashing.
        float spriteHalfHeight = spriteRenderer != null && spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.extents.y : 0f;

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
            float squashXZ = 1f + (1f - squashY) * squashSideInfluence;
            float heightDelta = spriteHalfHeight * baseScale.y * (1f - squashY);
            Vector3 anchorOffset = pieceTransform.TransformDirection(new Vector3(0f, -heightDelta, 0f));

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

    private void ResolveBlackAttacks()
    {
        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();
        bool playerHit = spawnManager.activePieces
            .OfType<BlackEnemyPiece>()
            .Any(enemy => ChessAttackResolver.GetAttackCells(enemy.pieceData, enemy.gridPos).Contains(playerPosition));

        // Even if multiple enemies cover the player, damage is applied only once per turn.
        if (playerHit) playerPiece.TakeDamage();
    }

    private void ResolvePlayerAttack()
    {
        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();
        HashSet<Vector2Int> targets = ChessAttackResolver.GetAttackCells(playerPiece.CurrentAttackData, playerPosition);

        foreach (BlackEnemyPiece enemy in spawnManager.activePieces.OfType<BlackEnemyPiece>().ToArray())
        {
            if (!targets.Contains(enemy.gridPos)) continue;
            if (!enemy.TakeDamage(playerPiece.atk)) continue;

            playerScore += enemy.scoreValue;
            defeatedBlackPieceCount++;
            defeatedCountByType[enemy.PieceType] = defeatedCountByType.GetValueOrDefault(enemy.PieceType) + 1;
            if (enemy.PieceType == ChessPieceType.King)
                playerPiece.RestoreHp(1);
            spawnManager.RemovePiece(enemy);
        }
    }

    private void FinishTurn(Vector2Int playerPosition)
    {
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
        spawnManager.SpawnBlackPieces(currentStage, playerPosition, currentTurn, mustSpawnQueen);
        forceQueenNextSpawn = false;

        if (spawnManager.QueenCount >= 3) forceKnightTransformTurns = 5;
        float buffChance = spawnManager.BlackPieceCount >= 6 ? 100f : Mathf.Min(30f, 10f + (currentStage - 1) * 5f);
        spawnManager.TrySpawnBuffPiece(playerPosition, currentTurn, buffChance);

        if (completedTurn % 5 == 0)
        {
            bool forceKnight = forceKnightTransformTurns > 0;
            spawnManager.TrySpawnTransformPiece(playerPosition, currentTurn, forceKnight);
        }
        if (forceKnightTransformTurns > 0) forceKnightTransformTurns--;

        isSettling = false;
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
        Debug.Log($"Game Over. Final score: {playerScore}");
        // Connect the lobby scene/UI transition here when it is available.
    }

}
