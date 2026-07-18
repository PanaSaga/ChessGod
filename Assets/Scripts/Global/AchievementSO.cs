using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAchievement", menuName = "ChessGame/AchievementData")]
public class AchievementSO : ScriptableObject
{
    [Header("Achievement")]
    public string achievementId;
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    [Tooltip("While true and not yet achieved, the list panel shows a placeholder instead of the real name/description/icon.")]
    public bool isSecret;
    public int targetCount = 1;

    [Tooltip("An event only needs to satisfy ONE of these groups to count (OR between groups). Every condition enabled inside a single group must ALL be true (AND within a group).")]
    public List<ConditionGroup> conditionGroups = new();

    public enum EventType
    {
        Kill,
        DamageDealt,
        Milestone
    }

    // Target black piece type(s). Leave every box unticked ("Nothing") to mean "any type".
    [Flags]
    public enum PieceTypeFlags
    {
        Pawn = 1 << 0,
        Knight = 1 << 1,
        Bishop = 1 << 2,
        Rook = 1 << 3,
        Queen = 1 << 4,
        King = 1 << 5
    }

    // Player's own transform form. No Pawn here - Pawn is only ever a buff (see requirePawnBuffActive),
    // never a form the player can be in. Leave every box unticked ("Nothing") to mean "any form".
    [Flags]
    public enum PlayerFormFlags
    {
        King = 1 << 0,
        Knight = 1 << 1,
        Bishop = 1 << 2,
        Rook = 1 << 3,
        Queen = 1 << 4
    }

    public enum StatType
    {
        Stage,
        Turn,
        Score
    }

    public enum CompareOp
    {
        GreaterOrEqual,
        LessOrEqual,
        Greater,
        Less,
        Equal
    }

    public enum TurnEndMethod
    {
        Any,
        Spacebar,
        TimeOut
    }

    [Serializable]
    public class ConditionGroup
    {
        public EventType eventType;

        [Header("Kill / DamageDealt - player state")]
        [Tooltip("Player's transform form when the event happened. Any ticked form counts (OR). Nothing ticked = any form.")]
        public PlayerFormFlags requiredPlayerForm;
        public bool requirePawnBuffActive;

        [Header("Kill / DamageDealt - target")]
        [Tooltip("Which black piece type(s) this applies to. Any ticked type counts (OR). Nothing ticked = any type.")]
        public PieceTypeFlags targetPieceType;

        [Header("Kill only")]
        public bool requireMultiKillInSingleAttack;
        [Min(2)] public int minKillCountInSingleAttack = 2;

        [Header("DamageDealt only")]
        [Tooltip("The player must also take damage in the same exchange (e.g. a mutual trade).")]
        public bool requirePlayerDamagedSimultaneously;

        [Header("Milestone - stat threshold")]
        public bool useStatCondition;
        public StatType statType;
        public CompareOp statCompare = CompareOp.GreaterOrEqual;
        public int statValue;

        [Header("Milestone - turn end method")]
        public bool useTurnEndMethodCondition;
        public TurnEndMethod turnEndMethod = TurnEndMethod.Spacebar;

        [Header("Milestone - remaining time")]
        public bool useRemainingTimeCondition;
        public CompareOp remainingTimeCompare = CompareOp.LessOrEqual;
        public float remainingTimeValue;

        [Header("Milestone - board state")]
        public bool useBoardBlackCountCondition;
        public CompareOp boardBlackCountCompare = CompareOp.GreaterOrEqual;
        public int boardBlackCountValue;
        public bool useBoardClearedCondition;
    }
}
