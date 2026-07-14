// WhiteTransformPiece.cs
using UnityEngine;

public class WhiteTransformPiece : ChessPiece
{
    [HideInInspector] public float transformDuration = 20f;

    private void Start()
    {
        // Read the duration from the parent's pieceData (WhitePieceSO).
        WhitePieceSO whiteData = pieceData as WhitePieceSO;
        if (whiteData != null)
        {
            transformDuration = whiteData.duration;
        }
    }
}
