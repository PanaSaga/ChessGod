using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Game Flow Data")]
    public int currentStage = 1;
    public int currentTurn = 1;

    [Header("Player Status")]
    public int playerHp = 3;
    public int playerScore = 0;
    public int playerAtk = 1;
    public string playerRangeType = "King";

    [Header("Timer Systems")]
    public float turnTimer = 10f;
    public float maxTurnTime = 10f;
    public bool isSettling = false;

    [Header("Buff Systems")]
    public float buffTimer = 0f;
    public bool isBuffActive = false;
    public float transformTimer = 0f;
    public bool isTransformActive = false;

    private SpawnManager spawnManager;
    private ControlManager controlManager;
    private bool queueQueenSpawn = false;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        spawnManager = Object.FindFirstObjectByType<SpawnManager>();
        controlManager = Object.FindFirstObjectByType<ControlManager>();
        StartStage(currentStage);
    }

    void Update()
    {
        if (isSettling) return;

        if (isBuffActive)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                isBuffActive = false;
                playerAtk = 1;
                Debug.Log("Buff Expired. Attack power restored to 1.");
            }
        }

        if (isTransformActive)
        {
            transformTimer -= Time.deltaTime;
            if (transformTimer <= 0f)
            {
                isTransformActive = false;
                playerRangeType = "King";
                Debug.Log("Transformation Expired. Attack range restored to King.");
            }
        }

        turnTimer -= Time.deltaTime;

        Keyboard currentKeyboard = Keyboard.current;
        bool spacePressed = (currentKeyboard != null && currentKeyboard.spaceKey.wasPressedThisFrame);

        if (turnTimer <= 0f || spacePressed)
        {
            StartSettlement();
        }
    }

    public void StartStage(int stageNumber)
    {
        currentStage = stageNumber;
        currentTurn = 1;
        playerHp = 3;
        maxTurnTime = Mathf.Max(5f, 10f - (currentStage - 1));
        turnTimer = maxTurnTime;

        if (spawnManager != null)
        {
            spawnManager.SpawnFirstTurn();
        }
    }

    public void OnPlayerMoved(int x, int z)
    {
        if (spawnManager == null) return;

        // ChessRemove 오타를 ChessPieceData로 정상 수정했습니다.
        List<ChessPieceData> toRemove = new List<ChessPieceData>();
        List<ChessPieceData> piecesCopy = new List<ChessPieceData>(spawnManager.activePieces);

        foreach (var piece in piecesCopy)
        {
            if (piece.gridPos.x == x && piece.gridPos.y == z)
            {
                if (piece.pieceType == "WhitePawn")
                {
                    isBuffActive = true;
                    buffTimer = 20f;
                    playerAtk = 2;
                    Debug.Log("Acquired White Pawn. Attack Power becomes 2 for 20 seconds.");
                    toRemove.Add(piece);
                }
                else if (piece.pieceType == "WhiteKnight" || piece.pieceType == "WhiteBishop" || piece.pieceType == "WhiteRook" || piece.pieceType == "WhiteQueen")
                {
                    isTransformActive = true;
                    transformTimer = 20f;

                    if (piece.pieceType == "WhiteKnight") playerRangeType = "Knight";
                    else if (piece.pieceType == "WhiteBishop") playerRangeType = "Bishop";
                    else if (piece.pieceType == "WhiteRook") playerRangeType = "Rook";
                    else if (piece.pieceType == "WhiteQueen") playerRangeType = "Queen";

                    Debug.Log("Acquired Transformation Piece. Range Mode changed to: " + playerRangeType + " for 20 seconds.");
                    toRemove.Add(piece);
                }
            }
        }

        foreach (var item in toRemove)
        {
            if (item.instance != null) Destroy(item.instance);
            spawnManager.activePieces.Remove(item);
        }
    }

    private void StartSettlement()
    {
        isSettling = true;
        Debug.Log("Turn settlement activated. Current Turn: " + currentTurn);

        Vector2Int playerPos = Vector2Int.zero;
        if (controlManager != null)
        {
            playerPos = controlManager.GetPlayerGridPosition();
        }

        bool playerHit = false;
        foreach (var piece in spawnManager.activePieces)
        {
            if (piece.pieceType == "Pawn" || piece.pieceType == "Knight" || piece.pieceType == "Queen")
            {
                if (CheckAttackRange(piece.pieceType, piece.gridPos, playerPos))
                {
                    playerHit = true;
                }
            }
        }

        if (playerHit)
        {
            playerHp--;
            Debug.Log("Player under attack. Current HP: " + playerHp);
            if (playerHp <= 0)
            {
                Debug.Log("Player HP is 0. Game Over. Returning to Lobby.");
                isSettling = false;
                return;
            }
        }

        List<ChessPieceData> deadEnemies = new List<ChessPieceData>();
        int currentAtk = isBuffActive ? 2 : 1;
        string currentRange = isTransformActive ? playerRangeType : "King";

        foreach (var piece in spawnManager.activePieces)
        {
            if (piece.pieceType == "Pawn" || piece.pieceType == "Knight" || piece.pieceType == "Queen")
            {
                if (CheckAttackRange(currentRange, playerPos, piece.gridPos))
                {
                    piece.hp -= currentAtk;
                    if (piece.hp <= 0)
                    {
                        deadEnemies.Add(piece);
                    }
                }
            }
        }

        foreach (var enemy in deadEnemies)
        {
            int scoreGain = 10;
            if (enemy.pieceType == "Knight") scoreGain = 20;
            if (enemy.pieceType == "Queen") scoreGain = 50;

            playerScore += scoreGain;
            Debug.Log("Defeated " + enemy.pieceType + ". Gained Score: " + scoreGain);

            if (enemy.instance != null) Destroy(enemy.instance);
            spawnManager.activePieces.Remove(enemy);
        }

        playerScore += 100;
        Debug.Log("Turn clear score gained. Total Score: " + playerScore);

        ProcessNextTurnSetup(playerPos);
    }

    private void ProcessNextTurnSetup(Vector2Int playerPos)
    {
        currentTurn++;

        if ((currentTurn - 1) % 10 == 0 && currentTurn > 1)
        {
            currentStage++;
            maxTurnTime = Mathf.Max(5f, 10f - (currentStage - 1));
            queueQueenSpawn = true;
            Debug.Log("Stage Up. Current Stage: " + currentStage + ", Turn Time: " + maxTurnTime);
        }

        if (spawnManager != null)
        {
            List<ChessPieceData> survivingBlacks = new List<ChessPieceData>();
            List<ChessPieceData> itemsToClear = new List<ChessPieceData>();

            foreach (var piece in spawnManager.activePieces)
            {
                if (piece.pieceType == "Pawn" || piece.pieceType == "Knight" || piece.pieceType == "Queen")
                {
                    survivingBlacks.Add(piece);
                }
                else
                {
                    itemsToClear.Add(piece);
                }
            }

            foreach (var item in itemsToClear)
            {
                if (item.instance != null) Destroy(item.instance);
                spawnManager.activePieces.Remove(item);
            }

            List<Vector2Int> availableSlots = new List<Vector2Int>();
            bool safeZonePass = false;
            int infinitePreventCounter = 0;

            while (!safeZonePass && infinitePreventCounter < 100)
            {
                infinitePreventCounter++;
                availableSlots.Clear();

                for (int z = 0; z < 8; z++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        if (x == playerPos.x && z == playerPos.y) continue;
                        availableSlots.Add(new Vector2Int(x, z));
                    }
                }

                for (int i = 0; i < availableSlots.Count; i++)
                {
                    Vector2Int temp = availableSlots[i];
                    int r = Random.Range(i, availableSlots.Count);
                    availableSlots[i] = availableSlots[r];
                    availableSlots[r] = temp;
                }

                int slotIndex = 0;
                foreach (var black in survivingBlacks)
                {
                    black.gridPos = availableSlots[slotIndex++];
                }

                int newSpawnsCount = currentStage;
                if (survivingBlacks.Count + newSpawnsCount > 16)
                {
                    newSpawnsCount = 16 - survivingBlacks.Count;
                }

                List<string> mockNewTypes = new List<string>();
                bool forceQueen = queueQueenSpawn;
                for (int i = 0; i < newSpawnsCount; i++)
                {
                    if (forceQueen)
                    {
                        mockNewTypes.Add("Queen");
                        forceQueen = false;
                    }
                    else
                    {
                        mockNewTypes.Add(Random.Range(0, 2) == 0 ? "Pawn" : "Knight");
                    }
                }

                List<Vector2Int> testPositions = new List<Vector2Int>();
                List<string> testTypes = new List<string>();

                foreach (var b in survivingBlacks)
                {
                    testPositions.Add(b.gridPos);
                    testTypes.Add(b.pieceType);
                }
                for (int i = 0; i < mockNewTypes.Count; i++)
                {
                    testPositions.Add(availableSlots[slotIndex + i]);
                    testTypes.Add(mockNewTypes[i]);
                }

                int safeTiles = 0;
                for (int z = 0; z < 8; z++)
                {
                    for (int x = 0; x < 8; x++)
                    {
                        Vector2Int checkTile = new Vector2Int(x, z);
                        bool isThreatened = false;
                        for (int k = 0; k < testPositions.Count; k++)
                        {
                            if (CheckAttackRange(testTypes[k], testPositions[k], checkTile))
                            {
                                isThreatened = true;
                                break;
                            }
                        }
                        if (!isThreatened) safeTiles++;
                    }
                }

                if (safeTiles >= 8)
                {
                    safeZonePass = true;

                    foreach (var black in survivingBlacks)
                    {
                        spawnManager.UpdateVisualPosition(black);
                    }

                    for (int i = 0; i < mockNewTypes.Count; i++)
                    {
                        Vector2Int pos = availableSlots[slotIndex + i];
                        GameObject prefab = spawnManager.chPBPawn;
                        if (mockNewTypes[i] == "Knight") prefab = spawnManager.chPBKnight;
                        if (mockNewTypes[i] == "Queen") prefab = spawnManager.chPBQueen;

                        spawnManager.SpawnEnemy(prefab, pos.x, pos.y, mockNewTypes[i]);
                    }

                    if (queueQueenSpawn) queueQueenSpawn = false;
                    slotIndex += mockNewTypes.Count;

                    if (Random.value < 0.20f && slotIndex < availableSlots.Count)
                    {
                        Vector2Int pos = availableSlots[slotIndex++];
                        spawnManager.SpawnEnemy(spawnManager.whitePawnPrefab, pos.x, pos.y, "WhitePawn");
                    }

                    if (currentStage % 5 == 0 && (currentTurn - 1) % 10 == 1 && slotIndex < availableSlots.Count)
                    {
                        Vector2Int pos = availableSlots[slotIndex++];
                        int randTransform = Random.Range(0, 4);
                        GameObject targetPrefab = null;
                        string targetType = "";

                        if (randTransform == 0) { targetPrefab = spawnManager.whiteKnightPrefab; targetType = "WhiteKnight"; }
                        else if (randTransform == 1) { targetPrefab = spawnManager.whiteBishopPrefab; targetType = "WhiteBishop"; }
                        else if (randTransform == 2) { targetPrefab = spawnManager.whiteRookPrefab; targetType = "WhiteRook"; }
                        else if (randTransform == 3) { targetPrefab = spawnManager.whiteQueenPrefab; targetType = "WhiteQueen"; }

                        spawnManager.SpawnEnemy(targetPrefab, pos.x, pos.y, targetType);
                    }
                }
            }
        }

        turnTimer = maxTurnTime;
        isSettling = false;
        Debug.Log("Next Turn initialized. Turn: " + currentTurn);
    }

    private bool CheckAttackRange(string rangeType, Vector2Int origin, Vector2Int target)
    {
        int dx = Mathf.Abs(origin.x - target.x);
        int dy = Mathf.Abs(origin.y - target.y);

        if (rangeType == "King")
        {
            return dx <= 1 && dy <= 1;
        }
        if (rangeType == "Pawn")
        {
            return dx == 1 && dy == 1;
        }
        if (rangeType == "Knight")
        {
            return (dx == 1 && dy == 2) || (dx == 2 && dy == 1);
        }
        if (rangeType == "Bishop")
        {
            return dx == dy;
        }
        if (rangeType == "Rook")
        {
            return origin.x == target.x || origin.y == target.y;
        }
        if (rangeType == "Queen")
        {
            return origin.x == target.x || origin.y == target.y || dx == dy;
        }
        return false;
    }
}