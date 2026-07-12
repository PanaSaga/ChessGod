using UnityEngine;
using UnityEngine.InputSystem;

public class ControlManager : MonoBehaviour
{
    private int playerX = 4;
    private int playerZ = 0;
    private GameObject playerObject;
    private bool isInitialized = false;

    public void SetupPlayer(GameObject player, int startX, int startZ)
    {
        if (player == null)
        {
            isInitialized = false;
            return;
        }
        playerObject = player;
        playerX = startX;
        playerZ = startZ;
        isInitialized = true;
        UpdatePlayerVisualPosition();
    }

    void Update()
    {
        if (!isInitialized || playerObject == null) return;

        // 게임 매니저가 정산 중인 상태라면 조작 차단
        if (GameManager.Instance != null && GameManager.Instance.isSettling) return;

        Keyboard currentKeyboard = Keyboard.current;
        if (currentKeyboard == null) return;

        if (currentKeyboard.upArrowKey.wasPressedThisFrame || currentKeyboard.wKey.wasPressedThisFrame)
        {
            MovePlayer(0, 1);
        }
        else if (currentKeyboard.downArrowKey.wasPressedThisFrame || currentKeyboard.sKey.wasPressedThisFrame)
        {
            MovePlayer(0, -1);
        }
        else if (currentKeyboard.leftArrowKey.wasPressedThisFrame || currentKeyboard.aKey.wasPressedThisFrame)
        {
            MovePlayer(-1, 0);
        }
        else if (currentKeyboard.rightArrowKey.wasPressedThisFrame || currentKeyboard.dKey.wasPressedThisFrame)
        {
            MovePlayer(1, 0);
        }
    }

    private void MovePlayer(int moveX, int moveZ)
    {
        int targetX = playerX + moveX;
        int targetZ = playerZ + moveZ;

        if (targetX >= 0 && targetX < 8 && targetZ >= 0 && targetZ < 8)
        {
            playerX = targetX;
            playerZ = targetZ;
            UpdatePlayerVisualPosition();

            // SpawnManager 내 플레이어의 논리적 위치 정보 동기화
            SpawnManager spawnManager = Object.FindFirstObjectByType<SpawnManager>();
            if (spawnManager != null && spawnManager.playerPiece != null)
            {
                spawnManager.playerPiece.gridPos = new Vector2Int(playerX, playerZ);
            }

            // 실시간 버프 말 및 변신 말 획득 판정을 위해 신호 송신
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnPlayerMoved(playerX, playerZ);
            }
        }
    }

    private void UpdatePlayerVisualPosition()
    {
        if (playerObject == null) return;
        Vector3 targetLocalPos = new Vector3(playerX - 3.5f, 0.101f, playerZ - 3.5f);
        playerObject.transform.localPosition = targetLocalPos;
    }

    public Vector2Int GetPlayerGridPosition()
    {
        return new Vector2Int(playerX, playerZ);
    }
}