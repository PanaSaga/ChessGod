using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : MonoBehaviour
{
    private int playerX = 4;
    private int playerZ;
    private PlayerPiece playerPiece;
    private bool isInitialized;

    public void SetupPlayer(PlayerPiece player, int startX, int startZ)
    {
        playerPiece = player;
        isInitialized = playerPiece != null;
        if (!isInitialized) return;

        playerX = startX;
        playerZ = startZ;
        playerPiece.gridPos = new Vector2Int(playerX, playerZ);
        UpdatePlayerVisualPosition();
    }

    private void Update()
    {
        if (!isInitialized || playerPiece == null ||
            (GameManager.Instance != null && GameManager.Instance.isSettling)) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard.upArrowKey.wasPressedThisFrame || keyboard.wKey.wasPressedThisFrame) MovePlayer(0, 1);
        else if (keyboard.downArrowKey.wasPressedThisFrame || keyboard.sKey.wasPressedThisFrame) MovePlayer(0, -1);
        else if (keyboard.leftArrowKey.wasPressedThisFrame || keyboard.aKey.wasPressedThisFrame) MovePlayer(-1, 0);
        else if (keyboard.rightArrowKey.wasPressedThisFrame || keyboard.dKey.wasPressedThisFrame) MovePlayer(1, 0);
    }

    private void MovePlayer(int moveX, int moveZ)
    {
        int targetX = playerX + moveX;
        int targetZ = playerZ + moveZ;
        if (targetX < 0 || targetX >= 8 || targetZ < 0 || targetZ >= 8) return;

        playerX = targetX;
        playerZ = targetZ;
        playerPiece.gridPos = new Vector2Int(playerX, playerZ);
        UpdatePlayerVisualPosition();
        GameManager.Instance?.OnPlayerMoved(playerX, playerZ);
    }

    private void UpdatePlayerVisualPosition()
    {
        playerPiece.transform.localPosition = new Vector3(playerX - 3.5f, 0.101f, playerZ - 3.5f);
    }

    public Vector2Int GetPlayerGridPosition() => new Vector2Int(playerX, playerZ);
}
