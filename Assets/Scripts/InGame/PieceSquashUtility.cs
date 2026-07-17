using UnityEngine;

// Shared "squash on landing" math, used by both the attack-jump animation (GameManager)
// and the spawn-drop landing animation (SpawnManager).
public static class PieceSquashUtility
{
    public static float GetSpriteHalfHeight(Transform pieceTransform)
    {
        SpriteRenderer spriteRenderer = pieceTransform.GetComponent<SpriteRenderer>();
        return spriteRenderer != null && spriteRenderer.sprite != null ? spriteRenderer.sprite.bounds.extents.y : 0f;
    }

    public static float GetSquashSideScale(float squashY, float sideInfluence) => 1f + (1f - squashY) * sideInfluence;

    // How far to shift the piece so its sprite's bottom edge stays anchored to the ground while squashing.
    public static Vector3 GetGroundAnchorOffset(Transform pieceTransform, float spriteHalfHeight, float baseScaleY, float squashY)
    {
        float heightDelta = spriteHalfHeight * baseScaleY * (1f - squashY);
        return pieceTransform.TransformDirection(new Vector3(0f, -heightDelta, 0f));
    }
}
