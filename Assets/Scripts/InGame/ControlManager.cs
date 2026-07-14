using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : MonoBehaviour
{
    [Header("Movement timing")]
    [SerializeField, Min(0.01f)] private float moveDuration = 0.1f;
    [SerializeField, Min(0.01f)] private float moveCooldown = 0.15f;

    private int playerX = 4;
    private int playerZ;
    private PlayerPiece playerPiece;
    private bool isInitialized;

    private Vector2Int heldDirection;
    private float cooldownTimer;
    private bool isMoving;
    private Vector3 moveStartPosition;
    private Vector3 moveTargetPosition;
    private float moveElapsed;

    public void SetupPlayer(PlayerPiece player, int startX, int startZ)
    {
        playerPiece = player;
        isInitialized = playerPiece != null;
        if (!isInitialized) return;

        playerX = startX;
        playerZ = startZ;
        playerPiece.gridPos = new Vector2Int(playerX, playerZ);
        SnapPlayerVisualPosition();
    }

    private void Update()
    {
        if (!isInitialized || playerPiece == null) return;

        UpdateHeldDirection();

        if (isMoving)
        {
            AnimateMove();
            return;
        }

        if (GameManager.Instance != null && GameManager.Instance.isSettling) return;

        cooldownTimer -= Time.deltaTime;
        if (cooldownTimer <= 0f && heldDirection != Vector2Int.zero)
            TryStartMove(heldDirection);
    }

    // Only updates which direction is queued. The actual move only fires once the cooldown allows it,
    // so holding a key auto-repeats and switching direction mid-cooldown does not trigger an extra move.
    private void UpdateHeldDirection()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) { heldDirection = Vector2Int.zero; return; }

        if (keyboard.upArrowKey.isPressed || keyboard.wKey.isPressed) heldDirection = new Vector2Int(0, 1);
        else if (keyboard.downArrowKey.isPressed || keyboard.sKey.isPressed) heldDirection = new Vector2Int(0, -1);
        else if (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed) heldDirection = new Vector2Int(-1, 0);
        else if (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed) heldDirection = new Vector2Int(1, 0);
        else heldDirection = Vector2Int.zero;
    }

    private void TryStartMove(Vector2Int direction)
    {
        int targetX = playerX + direction.x;
        int targetZ = playerZ + direction.y;
        if (targetX < 0 || targetX >= 8 || targetZ < 0 || targetZ >= 8) return;

        playerX = targetX;
        playerZ = targetZ;
        playerPiece.gridPos = new Vector2Int(playerX, playerZ);

        moveStartPosition = playerPiece.transform.localPosition;
        moveTargetPosition = new Vector3(playerX - 3.5f, 0.101f, playerZ - 3.5f);
        moveElapsed = 0f;
        isMoving = true;
        cooldownTimer = moveCooldown;

        GameManager.Instance?.OnPlayerMoved(playerX, playerZ);
    }

    private void AnimateMove()
    {
        moveElapsed += Time.deltaTime;
        float t = Mathf.Clamp01(moveElapsed / moveDuration);
        playerPiece.transform.localPosition = Vector3.Lerp(moveStartPosition, moveTargetPosition, t);
        if (t >= 1f) isMoving = false;
    }

    private void SnapPlayerVisualPosition()
    {
        playerPiece.transform.localPosition = new Vector3(playerX - 3.5f, 0.101f, playerZ - 3.5f);
    }

    public Vector2Int GetPlayerGridPosition() => new Vector2Int(playerX, playerZ);
}
