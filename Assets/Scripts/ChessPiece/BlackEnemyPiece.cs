using UnityEngine;

public class BlackEnemyPiece : ChessPiece
{
    [Header("Runtime data")]
    public int currentHp;
    public int scoreValue;

    public void Initialize(BlackPieceSO data)
    {
        pieceData = data;
        currentHp = data.defaultHp;
        scoreValue = data.scoreValue;
    }

    // The GameManager owns removal and score allocation after an enemy dies.
    public bool TakeDamage(int damage)
    {
        currentHp -= damage;
        Debug.Log($"{PieceType} took {damage} damage. HP: {currentHp}");
        return currentHp <= 0;
    }
}
