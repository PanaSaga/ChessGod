// WhiteBuffPiece.cs
using UnityEngine;

public class WhiteBuffPiece : ChessPiece
{
    [Header("Buff effect data")]
    public int attackBuffPower = 2; // Attack power while the buff is active

    [HideInInspector] public float buffDuration = 20f;

    private void Start()
    {
        // Read the duration from the parent's pieceData (WhitePieceSO).
        WhitePieceSO whiteData = pieceData as WhitePieceSO;
        if (whiteData != null)
        {
            buffDuration = whiteData.duration;
        }
    }
}
