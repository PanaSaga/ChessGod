using UnityEngine;

public class PlayerPiece : ChessPiece
{
    [Header("Player state")]
    public int maxHp = 3;
    public int hp = 3;
    public int atk = 1;

    [Header("Active transform")]
    public ChessPieceSO transformedAttackData;
    public float transformTimer;
    public bool isTransformActive;

    [Header("Active buff")]
    public float buffTimer;
    public bool isBuffActive;

    [Header("Transform visual")]
    [SerializeField] private SpriteRenderer playerRenderer;
    private Sprite defaultSprite;
    private Color defaultColor;
    private bool defaultFlipX;
    private bool defaultFlipY;

    public SpriteRenderer BodyRenderer => playerRenderer;

    // This is the sole source for the player's attack calculation and board highlight.
    public ChessPieceSO CurrentAttackData => isTransformActive && transformedAttackData != null
        ? transformedAttackData
        : pieceData;

    private void Awake()
    {
        if (playerRenderer == null)
            playerRenderer = GetComponentInChildren<SpriteRenderer>();

        if (playerRenderer == null)
        {
            Debug.LogError("PlayerPiece requires the player's SpriteRenderer for transform visuals.");
            return;
        }

        defaultSprite = playerRenderer.sprite;
        defaultColor = playerRenderer.color;
        defaultFlipX = playerRenderer.flipX;
        defaultFlipY = playerRenderer.flipY;
    }

    private void Update()
    {
        if (isBuffActive)
        {
            buffTimer -= Time.deltaTime;
            if (buffTimer <= 0f)
            {
                isBuffActive = false;
                atk = 1;
                Debug.Log("Buff ended. Attack returned to 1.");
            }
        }

        if (isTransformActive)
        {
            transformTimer -= Time.deltaTime;
            if (transformTimer <= 0f)
            {
                isTransformActive = false;
                transformedAttackData = null;
                RestoreKingVisual();
                Debug.Log("Transform ended. Attack range returned to the player SO.");
            }
        }
    }

    public void ApplyBuff(float duration, int buffAtk)
    {
        isBuffActive = true;
        buffTimer = duration;
        atk = buffAtk;
    }

    public void ApplyTransform(float duration, ChessPieceSO newAttackData)
    {
        if (newAttackData == null) return;
        isTransformActive = true;
        transformTimer = duration;
        transformedAttackData = newAttackData;
        ApplyTransformVisual(newAttackData);
    }

    public void TakeDamage()
    {
        hp--;
        Debug.Log($"Player hit. Remaining HP: {hp}");
    }

    public void RestoreHp(int amount)
    {
        if (amount <= 0 || hp >= maxHp) return;
        int previousHp = hp;
        hp = Mathf.Min(maxHp, hp + amount);
        Debug.Log($"Player recovered {hp - previousHp} HP. Current HP: {hp}");
    }

    private void ApplyTransformVisual(ChessPieceSO transformData)
    {
        if (playerRenderer == null) return;

        WhitePieceSO whiteData = transformData as WhitePieceSO;
        if (whiteData == null || whiteData.transformSprite == null)
        {
            Debug.LogError($"{transformData.name} needs a Transform Sprite assigned for the player's transform visual.");
            return;
        }

        playerRenderer.sprite = whiteData.transformSprite;
    }

    private void RestoreKingVisual()
    {
        if (playerRenderer == null) return;
        playerRenderer.sprite = defaultSprite;
        playerRenderer.color = defaultColor;
        playerRenderer.flipX = defaultFlipX;
        playerRenderer.flipY = defaultFlipY;
    }
}
