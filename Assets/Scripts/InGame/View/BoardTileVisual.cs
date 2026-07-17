using UnityEngine;

// Leaves the tile's own black/white mesh untouched and layers attack-range sprites on top of it.
// Player and enemy overlays are separate renderers so both can show at once when their ranges
// overlap the same tile, with the player's sprite drawn above the enemy's.
public class BoardTileVisual : MonoBehaviour
{
    [SerializeField] private Vector3 overlayWorldOffset = new(0f, 0.06f, 0f);
    [SerializeField] private Vector3 overlayRotation = new(90f, 0f, 0f);
    [SerializeField, Min(0.01f)] private float overlaySize = 1f;
    [Tooltip("Must stay below every possible piece sorting order (0 and up, see ChessBoardUtility) so pieces always render above the attack-range overlays.")]
    [SerializeField] private int enemyOverlaySortingOrder = -10;
    [Tooltip("Above the enemy overlay, so the player's range sprite shows on top when both overlap the same tile.")]
    [SerializeField] private int playerOverlaySortingOrder = -9;

    private SpriteRenderer playerOverlayRenderer;
    private SpriteRenderer enemyOverlayRenderer;

    private void Awake() => EnsureOverlays();

    public void SetPlayerOverlay(Sprite sprite, float alpha = 1f)
    {
        EnsureOverlays();
        ApplyOverlay(playerOverlayRenderer, sprite, alpha);
    }

    public void SetEnemyOverlay(Sprite sprite, float alpha = 1f)
    {
        EnsureOverlays();
        ApplyOverlay(enemyOverlayRenderer, sprite, alpha);
    }

    public void ClearOverlay()
    {
        EnsureOverlays();
        playerOverlayRenderer.gameObject.SetActive(false);
        enemyOverlayRenderer.gameObject.SetActive(false);
    }

    private void ApplyOverlay(SpriteRenderer renderer, Sprite sprite, float alpha)
    {
        if (sprite == null)
        {
            renderer.gameObject.SetActive(false);
            return;
        }

        renderer.sprite = sprite;
        renderer.color = new Color(1f, 1f, 1f, alpha);
        renderer.gameObject.SetActive(true);

        float spriteWidth = sprite.bounds.size.x;
        float scale = spriteWidth <= 0f ? 1f : overlaySize / spriteWidth;
        renderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    private void EnsureOverlays()
    {
        if (playerOverlayRenderer == null) playerOverlayRenderer = CreateOverlayRenderer("PlayerAttackOverlay", playerOverlaySortingOrder);
        if (enemyOverlayRenderer == null) enemyOverlayRenderer = CreateOverlayRenderer("EnemyAttackOverlay", enemyOverlaySortingOrder);
    }

    // World-space, not local: the tile itself is non-uniformly scaled (thin in Y), which would
    // otherwise squash a local offset/rotation and bury the overlay inside the tile mesh.
    private SpriteRenderer CreateOverlayRenderer(string name, int sortingOrder)
    {
        GameObject overlay = new(name);
        overlay.transform.SetParent(transform, false);
        overlay.transform.position = transform.position + overlayWorldOffset;
        overlay.transform.rotation = Quaternion.Euler(overlayRotation);
        SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
        renderer.sortingOrder = sortingOrder;
        overlay.SetActive(false);
        return renderer;
    }
}
