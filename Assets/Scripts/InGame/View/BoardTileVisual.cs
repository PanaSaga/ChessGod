using UnityEngine;

// Leaves the tile's own black/white mesh untouched and layers an attack-range sprite on top of it.
public class BoardTileVisual : MonoBehaviour
{
    [SerializeField] private Vector3 overlayWorldOffset = new(0f, 0.06f, 0f);
    [SerializeField] private Vector3 overlayRotation = new(90f, 0f, 0f);
    [SerializeField, Min(0.01f)] private float overlaySize = 1f;
    [Tooltip("Must stay below every possible piece sorting order (0 and up, see ChessBoardUtility) so pieces always render above the attack-range overlay.")]
    [SerializeField] private int overlaySortingOrder = -10;

    private SpriteRenderer overlayRenderer;

    private void Awake() => EnsureOverlay();

    public void SetOverlay(Sprite sprite)
    {
        EnsureOverlay();
        if (sprite == null)
        {
            overlayRenderer.gameObject.SetActive(false);
            return;
        }

        overlayRenderer.sprite = sprite;
        overlayRenderer.gameObject.SetActive(true);

        float spriteWidth = sprite.bounds.size.x;
        float scale = spriteWidth <= 0f ? 1f : overlaySize / spriteWidth;
        overlayRenderer.transform.localScale = new Vector3(scale, scale, 1f);
    }

    public void ClearOverlay()
    {
        EnsureOverlay();
        overlayRenderer.gameObject.SetActive(false);
    }

    private void EnsureOverlay()
    {
        if (overlayRenderer != null) return;

        GameObject overlay = new("AttackOverlay");
        overlay.transform.SetParent(transform, false);
        // World-space, not local: the tile itself is non-uniformly scaled (thin in Y), which would
        // otherwise squash a local offset/rotation and bury the overlay inside the tile mesh.
        overlay.transform.position = transform.position + overlayWorldOffset;
        overlay.transform.rotation = Quaternion.Euler(overlayRotation);
        overlayRenderer = overlay.AddComponent<SpriteRenderer>();
        overlayRenderer.sortingOrder = overlaySortingOrder;
        overlay.SetActive(false);
    }
}
