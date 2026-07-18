using System;
using System.Collections.Generic;
using UnityEngine;

// Snapshot of what just happened in-game, passed to AchievementManager.ReportEvent. Only the
// fields matching context.eventType are checked against a given condition group.
public struct AchievementEventContext
{
    public AchievementSO.EventType eventType;

    // Kill / DamageDealt
    public AchievementSO.PlayerFormFlags playerForm;
    public bool pawnBuffActive;
    public AchievementSO.PieceTypeFlags targetPieceType;
    public int killCountInThisAttack;
    public bool playerDamagedSimultaneously;

    // Milestone
    public int stage;
    public int turn;
    public int score;
    public AchievementSO.TurnEndMethod turnEndMethod;
    public float remainingTime;
    public int boardBlackPieceCount;
    public bool boardCleared;
}

public class AchievementManager : MonoBehaviour
{
    [SerializeField] private AchievementSO[] achievements;

    public event Action<AchievementSO> OnAchievementUnlocked;
    // Fires on every progress tick (even ones that don't reach the target yet), so a live UI can refresh.
    public event Action<AchievementSO> OnAchievementProgressChanged;

    public IReadOnlyList<AchievementSO> Achievements => achievements;

    private DataManager Data => GlobalManager.Instance.DataManager;

    public int GetProgress(string achievementId) => Data.GetAchievementProgress(achievementId);

    public bool IsUnlocked(string achievementId) => Data.IsAchievementUnlocked(achievementId);

    public bool IsAcknowledged(string achievementId) => Data.IsAchievementAcknowledged(achievementId);

    public void AcknowledgeAchievement(string achievementId) => Data.AcknowledgeAchievement(achievementId);

    public void ReportEvent(AchievementEventContext context)
    {
        foreach (AchievementSO achievement in achievements)
        {
            if (IsUnlocked(achievement.achievementId)) continue;
            if (!MatchesAnyGroup(achievement, context)) continue;

            int progress = GetProgress(achievement.achievementId) + 1;
            Data.SetAchievementProgress(achievement.achievementId, progress);
            OnAchievementProgressChanged?.Invoke(achievement);

            if (progress >= achievement.targetCount)
            {
                Data.UnlockAchievement(achievement.achievementId);
                OnAchievementUnlocked?.Invoke(achievement);
            }
        }
    }

    private static bool MatchesAnyGroup(AchievementSO achievement, AchievementEventContext context)
    {
        foreach (AchievementSO.ConditionGroup group in achievement.conditionGroups)
        {
            if (group.eventType == context.eventType && MatchesGroup(group, context)) return true;
        }
        return false;
    }

    private static bool MatchesGroup(AchievementSO.ConditionGroup group, AchievementEventContext context)
    {
        switch (context.eventType)
        {
            case AchievementSO.EventType.Kill:
            case AchievementSO.EventType.DamageDealt:
                if (!MatchesFlags(group.requiredPlayerForm, context.playerForm)) return false;
                if (group.requirePawnBuffActive && !context.pawnBuffActive) return false;
                if (!MatchesFlags(group.targetPieceType, context.targetPieceType)) return false;

                if (context.eventType == AchievementSO.EventType.Kill)
                {
                    if (group.requireMultiKillInSingleAttack && context.killCountInThisAttack < group.minKillCountInSingleAttack) return false;
                }
                else if (group.requirePlayerDamagedSimultaneously && !context.playerDamagedSimultaneously)
                {
                    return false;
                }
                return true;

            case AchievementSO.EventType.Milestone:
                if (group.useStatCondition)
                {
                    int statValue = group.statType switch
                    {
                        AchievementSO.StatType.Stage => context.stage,
                        AchievementSO.StatType.Turn => context.turn,
                        AchievementSO.StatType.Score => context.score,
                        _ => 0
                    };
                    if (!Compare(statValue, group.statCompare, group.statValue)) return false;
                }
                if (group.useTurnEndMethodCondition && context.turnEndMethod != group.turnEndMethod) return false;
                if (group.useRemainingTimeCondition && !Compare(context.remainingTime, group.remainingTimeCompare, group.remainingTimeValue)) return false;
                if (group.useBoardBlackCountCondition && !Compare(context.boardBlackPieceCount, group.boardBlackCountCompare, group.boardBlackCountValue)) return false;
                if (group.useBoardClearedCondition && !context.boardCleared) return false;
                return true;

            default:
                return false;
        }
    }

    // Nothing ticked in a flags field means "no restriction" rather than "never matches".
    private static bool MatchesFlags(AchievementSO.PlayerFormFlags required, AchievementSO.PlayerFormFlags actual) =>
        required == 0 || (required & actual) != 0;

    private static bool MatchesFlags(AchievementSO.PieceTypeFlags required, AchievementSO.PieceTypeFlags actual) =>
        required == 0 || (required & actual) != 0;

    private static bool Compare(float value, AchievementSO.CompareOp op, float target) => op switch
    {
        AchievementSO.CompareOp.GreaterOrEqual => value >= target,
        AchievementSO.CompareOp.LessOrEqual => value <= target,
        AchievementSO.CompareOp.Greater => value > target,
        AchievementSO.CompareOp.Less => value < target,
        AchievementSO.CompareOp.Equal => Mathf.Approximately(value, target),
        _ => false
    };
}
