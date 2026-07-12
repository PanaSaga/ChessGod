using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ChessPieceData
{
    public GameObject instance;
    public Vector2Int gridPos;
    public string pieceType; // King, Pawn, Knight, Queen, Bishop, Rook, WhitePawn, WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen
    public int hp = 1;
}

public class SpawnManager : MonoBehaviour
{
    [Header("Black Enemy Prefabs")]
    public GameObject chPWKing;
    public GameObject chPBPawn;
    public GameObject chPBKnight;
    public GameObject chPBQueen;

    [Header("White Buff Prefabs")]
    public GameObject whitePawnPrefab; // 백의 폰 버프말 프리팹

    [Header("White Transform Prefabs")]
    public GameObject whiteKnightPrefab; // 백의 변신 나이트 프리팹
    public GameObject whiteBishopPrefab; // 백의 변신 비숍 프리팹
    public GameObject whiteRookPrefab;   // 백의 변신 룩 프리팹
    public GameObject whiteQueenPrefab;  // 백의 변신 퀸 프리팹

    [Header("Parent Transform")]
    public Transform fieldChSD;

    [HideInInspector]
    public List<ChessPieceData> activePieces = new List<ChessPieceData>();
    [HideInInspector]
    public ChessPieceData playerPiece;

    public void ClearAllCharacters()
    {
        foreach (var piece in activePieces)
        {
            if (piece.instance != null) Destroy(piece.instance);
        }
        activePieces.Clear();

        if (playerPiece != null && playerPiece.instance != null)
        {
            Destroy(playerPiece.instance);
        }
        playerPiece = null;
    }

    public void SpawnPlayer(int x, int z)
    {
        if (chPWKing == null || fieldChSD == null) return;

        GameObject go = Instantiate(chPWKing, fieldChSD);
        playerPiece = new ChessPieceData();
        playerPiece.instance = go;
        playerPiece.gridPos = new Vector2Int(x, z);
        playerPiece.pieceType = "King";
        playerPiece.hp = 1;

        UpdateVisualPosition(playerPiece);
    }

    public void SpawnFirstTurn()
    {
        ClearAllCharacters();
        SpawnPlayer(4, 0); // E1 위치 고정 시작

        List<Vector2Int> emptyPositions = new List<Vector2Int>();
        for (int z = 0; z < 8; z++)
        {
            for (int x = 0; x < 8; x++)
            {
                if (x == 4 && z == 0) continue;
                emptyPositions.Add(new Vector2Int(x, z));
            }
        }
        ShuffleList(emptyPositions);

        // 1스테이지 1턴 기본 흑의 말 3개 생성
        SpawnEnemy(chPBQueen, emptyPositions[0].x, emptyPositions[0].y, "Queen");
        SpawnEnemy(chPBKnight, emptyPositions[1].x, emptyPositions[1].y, "Knight");
        SpawnEnemy(chPBPawn, emptyPositions[2].x, emptyPositions[2].y, "Pawn");

        ControlManager controlManager = Object.FindFirstObjectByType<ControlManager>();
        if (controlManager != null)
        {
            controlManager.SetupPlayer(playerPiece.instance, 4, 0);
        }
    }

    public void SpawnEnemy(GameObject prefab, int x, int z, string type)
    {
        if (prefab == null || fieldChSD == null) return;

        GameObject go = Instantiate(prefab, fieldChSD);
        ChessPieceData data = new ChessPieceData();
        data.instance = go;
        data.gridPos = new Vector2Int(x, z);
        data.pieceType = type;
        data.hp = 1;

        activePieces.Add(data);
        UpdateVisualPosition(data);
    }

    public void UpdateVisualPosition(ChessPieceData piece)
    {
        if (piece == null || piece.instance == null) return;
        Vector3 localPos = new Vector3(piece.gridPos.x - 3.5f, 0.101f, piece.gridPos.y - 3.5f);
        piece.instance.transform.localPosition = localPos;
    }

    private void ShuffleList(List<Vector2Int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Vector2Int temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}