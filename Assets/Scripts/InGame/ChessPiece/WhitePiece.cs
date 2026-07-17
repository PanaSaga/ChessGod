using UnityEngine;

// Shared by every white piece (buff/transform) that reads its active duration from a WhitePieceSO.
public abstract class WhitePiece : ChessPiece
{
    [HideInInspector] public float duration = 20f;

    protected virtual void Start()
    {
        if (pieceData is WhitePieceSO whiteData)
            duration = whiteData.duration;
    }
}
