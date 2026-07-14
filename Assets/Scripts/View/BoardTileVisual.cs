using UnityEngine;

// Keeps each tile's original material and applies temporary attack-range materials.
public class BoardTileVisual : MonoBehaviour
{
    private Renderer tileRenderer;
    private SpriteRenderer spriteRenderer;
    private Material baseMaterial;
    private Color baseSpriteColor;
    private bool isReady;

    private void Awake() => Initialize();

    public void Initialize()
    {
        if (isReady) return;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        tileRenderer = spriteRenderer;
        if (tileRenderer == null)
            tileRenderer = GetComponentInChildren<Renderer>();

        if (tileRenderer != null)
        {
            baseMaterial = tileRenderer.sharedMaterial;
            if (spriteRenderer != null)
                baseSpriteColor = spriteRenderer.color;
            isReady = true;
            return;
        }

        Debug.LogWarning($"Board tile '{name}' has no SpriteRenderer or Renderer.");
    }

    public void SetOverlay(Material overlayMaterial)
    {
        Initialize();
        if (!isReady || overlayMaterial == null) return;
        tileRenderer.sharedMaterial = overlayMaterial;
        // The overlay material supplies the highlight colour; do not multiply it by the tile's black/white tint.
        if (spriteRenderer != null) spriteRenderer.color = Color.white;
    }

    public void ClearOverlay()
    {
        Initialize();
        if (!isReady) return;
        tileRenderer.sharedMaterial = baseMaterial;
        if (spriteRenderer != null) spriteRenderer.color = baseSpriteColor;
    }
}
