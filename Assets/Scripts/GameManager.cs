using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private const int BoardSize = 8;
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

    [Header("Turn settlement")]
    [Tooltip("Time in seconds that all player/enemy attack ranges remain visible before damage is resolved.")]
    [SerializeField, Min(0f)] private float settlementPreviewDuration = 0.5f;

    private SpawnManager spawnManager;
    private ControlManager controlManager;
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
        StartCoroutine(SettlementRoutine());
    }

    private IEnumerator SettlementRoutine()
    {
        // BoardViewManager reads isSettling and shows all enemy ranges during this wait.
        yield return new WaitForSeconds(settlementPreviewDuration);

        ResolveBlackAttacks();
        if (playerPiece.hp <= 0)
        {
            GameOver();
            yield break;
        }

        ResolvePlayerAttack();
        playerScore += TurnClearScore;
        SetupNextTurn();
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

    private void SetupNextTurn()
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

        Vector2Int playerPosition = controlManager.GetPlayerGridPosition();
        spawnManager.RemoveExpiredWhitePieces(currentTurn);
        spawnManager.RepositionBlackPieces(playerPosition);

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
