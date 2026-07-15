using System.Collections.Generic;
using UnityEngine;

// World-space heart HP display plus Up/Tr duration bars.
[RequireComponent(typeof(ChessPiece))]
public class PieceStatusBars : MonoBehaviour
{
    [Header("Heart HP layout")]
    [SerializeField] private Vector3 heartRowOffset = new(0f, 0.25f, 0.35f);
    [SerializeField, Range(0.05f, 0.5f)] private float heartSize = 0.18f;
    [SerializeField, Range(0f, 0.2f)] private float heartSpacing = 0.04f;
    [SerializeField] private Vector3 displayRotation = new(90f, 0f, 0f);

    [Header("Piece type icon (left of hearts, or centered alone for HP-less pieces)")]
    [SerializeField, Range(0.05f, 0.75f)] private float typeIconSize = 0.27f;
    [SerializeField, Range(0f, 0.2f)] private float typeIconSpacing = 0.06f;

    [Header("Player duration bars")]
    [SerializeField] private Vector3 upBarOffset = new(0f, 0.25f, -0.35f);
    [SerializeField] private Vector3 trBarOffset = new(0f, 0.25f, -0.52f);
    [SerializeField, Range(0.1f, 3f)] private float barWidth = 1.5f;
    [SerializeField, Range(0.02f, 0.5f)] private float barHeight = 0.12f;
    [SerializeField, Min(1)] private int maxPlayerHp = 3;
    [SerializeField, Min(0.1f)] private float effectDuration = 20f;
    [SerializeField] private int sortingOrder = 200;

    private PlayerPiece playerPiece;
    private BlackEnemyPiece blackPiece;
    private ChessPiece basePiece;
    private StatusBarSpriteLibrary sprites;
    private HeartRow hearts;
    private BarPair up;
    private BarPair tr;
    private bool initialized;

    private void Awake()
    {
        playerPiece = GetComponent<PlayerPiece>();
        blackPiece = GetComponent<BlackEnemyPiece>();
        basePiece = GetComponent<ChessPiece>();
    }

    private void LateUpdate()
    {
        if (!initialized)
        {
            sprites = FindFirstObjectByType<StatusBarSpriteLibrary>();
            if (sprites == null) return;
            BuildVisuals();
        }

        if (blackPiece != null)
        {
            BlackPieceSO data = blackPiece.pieceData as BlackPieceSO;
            Sprite blackIcon = sprites.GetBlackTypeIcon(blackPiece.PieceType);
            hearts.Set(blackPiece.currentHp, data == null ? 0 : data.defaultHp, blackIcon);
            hearts.Follow(transform);
            return;
        }

        if (playerPiece != null)
        {
            ChessPieceType currentType = playerPiece.CurrentAttackData != null ? playerPiece.CurrentAttackData.pieceType : ChessPieceType.King;
            Sprite playerIcon = sprites.GetWhiteTypeIcon(currentType);
            hearts.Set(playerPiece.hp, maxPlayerHp, playerIcon);
            hearts.Follow(transform);
            up.Set(playerPiece.buffTimer / effectDuration, playerPiece.isBuffActive);
            tr.Set(playerPiece.transformTimer / effectDuration, playerPiece.isTransformActive);
            up.Follow(transform);
            tr.Follow(transform);
            return;
        }

        // White buff/transform pieces on the board: no HP, just a centered type icon.
        if (basePiece == null) return;
        Sprite whiteIcon = sprites.GetWhiteTypeIcon(basePiece.PieceType);
        hearts.Set(0, 0, whiteIcon);
        hearts.Follow(transform);
    }

    private void BuildVisuals()
    {
        hearts = CreateHeartRow();
        if (playerPiece != null)
        {
            up = CreateBar("Up", sprites.upLine, sprites.upBar, upBarOffset, sortingOrder + 10);
            tr = CreateBar("Tr", sprites.trLine, sprites.trBar, trBarOffset, sortingOrder + 12);
        }
        initialized = true;
    }

    private HeartRow CreateHeartRow()
    {
        GameObject root = new("Status_Hearts");
        root.transform.SetParent(transform, false);
        return new HeartRow(root, sprites.hpHeart, sprites.hpEmpty, heartSize, heartSpacing,
            typeIconSize, typeIconSpacing, sortingOrder, heartRowOffset, Quaternion.Euler(displayRotation));
    }

    private BarPair CreateBar(string barName, Sprite lineSprite, Sprite fillSprite, Vector3 offset, int order)
    {
        if (lineSprite == null || fillSprite == null)
        {
            Debug.LogError($"{name}: {barName} Line and Bar sprites must be assigned in StatusBarSpriteLibrary.");
            return new BarPair();
        }

        GameObject root = new($"Status_{barName}");
        root.transform.SetParent(transform, false);
        SpriteRenderer line = root.AddComponent<SpriteRenderer>();
        line.sprite = lineSprite;
        line.drawMode = SpriteDrawMode.Sliced;
        Vector2 barSize = new(barWidth, barHeight);
        line.size = barSize;
        line.sortingOrder = order;

        GameObject fillObject = new("Bar");
        fillObject.transform.SetParent(root.transform, false);
        SpriteRenderer fill = fillObject.AddComponent<SpriteRenderer>();
        fill.sprite = fillSprite;
        fill.drawMode = SpriteDrawMode.Sliced;
        fill.sortingOrder = order + 1;
        return new BarPair(root, fill, barSize, offset, Quaternion.Euler(displayRotation));
    }

    private sealed class HeartRow
    {
        private readonly GameObject root;
        private readonly Sprite filledSprite;
        private readonly Sprite emptySprite;
        private readonly float size;
        private readonly float spacing;
        private readonly float iconSize;
        private readonly float iconSpacing;
        private readonly int order;
        private readonly Vector3 worldOffset;
        private readonly Quaternion worldRotation;
        private readonly List<SpriteRenderer> renderers = new();
        private readonly SpriteRenderer typeIconRenderer;

        public HeartRow(GameObject root, Sprite filledSprite, Sprite emptySprite, float size, float spacing,
            float iconSize, float iconSpacing, int order, Vector3 worldOffset, Quaternion worldRotation)
        {
            this.root = root;
            this.filledSprite = filledSprite;
            this.emptySprite = emptySprite;
            this.size = size;
            this.spacing = spacing;
            this.iconSize = iconSize;
            this.iconSpacing = iconSpacing;
            this.order = order;
            this.worldOffset = worldOffset;
            this.worldRotation = worldRotation;

            GameObject iconObject = new("TypeIcon");
            iconObject.transform.SetParent(root.transform, false);
            typeIconRenderer = iconObject.AddComponent<SpriteRenderer>();
            typeIconRenderer.sortingOrder = order + 1;
        }

        public void Set(int currentHp, int maxHp, Sprite typeIcon)
        {
            bool showHearts = filledSprite != null && emptySprite != null && maxHp > 0;
            bool showIcon = typeIcon != null;

            if (!showHearts && !showIcon)
            {
                root.SetActive(false);
                return;
            }
            root.SetActive(true);

            float step = size + spacing;
            float scale = 1f;
            float heartsRowWidth = 0f;
            if (showHearts)
            {
                currentHp = Mathf.Clamp(currentHp, 0, maxHp);
                while (renderers.Count < maxHp) AddHeart();
                float heartWidth = filledSprite.bounds.size.x;
                scale = heartWidth <= 0f ? 1f : size / heartWidth;
                heartsRowWidth = (maxHp - 1) * step + size;
            }
            else
            {
                foreach (SpriteRenderer heart in renderers) heart.gameObject.SetActive(false);
            }

            // With no hearts, the icon just sits centered on its own; otherwise it sits to the left of the heart row.
            float iconBlockWidth = showIcon ? iconSize + iconSpacing : 0f;
            float groupWidth = showHearts ? iconBlockWidth + heartsRowWidth : iconSize;
            float leftEdge = -groupWidth * 0.5f;

            typeIconRenderer.gameObject.SetActive(showIcon);
            if (showIcon)
            {
                typeIconRenderer.sprite = typeIcon;
                float iconSpriteWidth = typeIcon.bounds.size.x;
                float iconScale = iconSpriteWidth <= 0f ? 1f : iconSize / iconSpriteWidth;
                float iconCenterX = showHearts ? leftEdge + iconSize * 0.5f : 0f;
                typeIconRenderer.transform.localPosition = new Vector3(iconCenterX, 0f, -0.01f);
                typeIconRenderer.transform.localScale = new Vector3(iconScale, iconScale, 1f);
            }

            if (!showHearts) return;

            float heartsFirstX = leftEdge + iconBlockWidth + size * 0.5f;
            for (int i = 0; i < renderers.Count; i++)
            {
                bool active = i < maxHp;
                renderers[i].gameObject.SetActive(active);
                if (!active) continue;

                renderers[i].sprite = i < currentHp ? filledSprite : emptySprite;
                renderers[i].transform.localPosition = new Vector3(heartsFirstX + i * step, 0f, -0.01f);
                renderers[i].transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        public void Follow(Transform owner)
        {
            root.transform.position = owner.position + worldOffset;
            root.transform.rotation = worldRotation;
        }

        private void AddHeart()
        {
            GameObject heart = new("Heart");
            heart.transform.SetParent(root.transform, false);
            SpriteRenderer renderer = heart.AddComponent<SpriteRenderer>();
            renderer.sortingOrder = order + 1;
            renderers.Add(renderer);
        }
    }

    private sealed class BarPair
    {
        private readonly GameObject root;
        private readonly SpriteRenderer fillRenderer;
        private readonly Vector2 fullSize;
        private readonly Vector3 worldOffset;
        private readonly Quaternion worldRotation;

        public BarPair() { }

        public BarPair(GameObject root, SpriteRenderer fillRenderer, Vector2 fullSize, Vector3 worldOffset, Quaternion worldRotation)
        {
            this.root = root;
            this.fillRenderer = fillRenderer;
            this.fullSize = fullSize;
            this.worldOffset = worldOffset;
            this.worldRotation = worldRotation;
        }

        public void Set(float value, bool visible)
        {
            if (root == null) return;
            root.SetActive(visible);
            if (!visible) return;

            value = Mathf.Clamp01(value);
            fillRenderer.enabled = value > 0f;
            if (!fillRenderer.enabled) return;
            float currentWidth = fullSize.x * value;
            fillRenderer.size = new Vector2(currentWidth, fullSize.y);
            fillRenderer.transform.localPosition = new Vector3((currentWidth - fullSize.x) * 0.5f, 0f, -0.01f);
        }

        public void Follow(Transform owner)
        {
            if (root == null) return;
            root.transform.position = owner.position + worldOffset;
            root.transform.rotation = worldRotation;
        }
    }
}
