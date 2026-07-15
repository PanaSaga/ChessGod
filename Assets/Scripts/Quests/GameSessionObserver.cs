using System;
using System.Collections.Generic;
using UnityEngine;

// Reconstructs quest-relevant signals (enemy defeated, multi-kill, stage cleared,
// score changed, game over) purely by diffing GameManager's public state every frame.
// GameManager.cs is never modified -- this is the observer side of that boundary.
public class GameSessionObserver : MonoBehaviour
{
    public event Action<ChessPieceType> OnEnemyDefeated;
    public event Action<int> OnMultiKill;
    public event Action<int> OnStageCleared;
    public event Action<int> OnScoreChanged;
    public event Action<int> OnGameOver;

    private static readonly ChessPieceType[] AllPieceTypes = (ChessPieceType[])Enum.GetValues(typeof(ChessPieceType));

    private GameManager gameManager;
    private int lastScore;
    private int lastStage;
    private bool lastGameOver;
    private readonly Dictionary<ChessPieceType, int> lastDefeatedByType = new();

    private void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogWarning("[GameSessionObserver] GameManager.Instance를 찾을 수 없습니다.");
            enabled = false;
            return;
        }

        lastScore = gameManager.playerScore;
        lastStage = gameManager.currentStage;
        lastGameOver = gameManager.isGameOver;
        foreach (ChessPieceType type in AllPieceTypes)
            lastDefeatedByType[type] = gameManager.GetDefeatedCount(type);
    }

    private void Update()
    {
        if (gameManager == null) return;

        DiffDefeatedCounts();

        if (gameManager.playerScore != lastScore)
        {
            lastScore = gameManager.playerScore;
            OnScoreChanged?.Invoke(lastScore);
        }

        if (gameManager.currentStage != lastStage)
        {
            lastStage = gameManager.currentStage;
            OnStageCleared?.Invoke(lastStage);
        }

        // Edge-triggered: GameManager.isGameOver never resets, so this fires exactly once.
        // NOTE: scene transition back to the lobby is intentionally NOT handled here --
        // MainScene doesn't exist yet. Subscribe to OnGameOver from the lobby-flow script
        // once it does, rather than adding SceneManager calls to this observer.
        if (gameManager.isGameOver && !lastGameOver)
        {
            lastGameOver = true;
            OnGameOver?.Invoke(gameManager.playerScore);
        }
    }

    // A settlement resolves at most once between two Update() calls, so a combined delta
    // of 2+ across piece types here always corresponds to a single ResolvePlayerAttack().
    private void DiffDefeatedCounts()
    {
        int totalDelta = 0;

        foreach (ChessPieceType type in AllPieceTypes)
        {
            int current = gameManager.GetDefeatedCount(type);
            int delta = current - lastDefeatedByType[type];
            if (delta <= 0) continue;

            lastDefeatedByType[type] = current;
            totalDelta += delta;
            for (int i = 0; i < delta; i++)
                OnEnemyDefeated?.Invoke(type);
        }

        if (totalDelta >= 2)
            OnMultiKill?.Invoke(totalDelta);
    }
}
