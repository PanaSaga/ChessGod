using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject boardWPrefab;
    public GameObject boardBPrefab;

    [Header("Board setting")]
    public Transform fieldBoard;

    private readonly Dictionary<Vector2Int, BoardTileVisual> tiles = new();

    private void Awake() => CacheBoardTiles();

    [ContextMenu("Generate Board In Editor")]
    public void GenerateBoard()
    {
        ClearBoard();
        if (fieldBoard == null)
        {
            Debug.LogError("Assign Field Board in the Inspector.");
            return;
        }

        for (int z = 0; z < 8; z++)
            for (int x = 0; x < 8; x++)
            {
                GameObject prefab = (x + z) % 2 == 0 ? boardBPrefab : boardWPrefab;
                if (prefab == null) continue;

                GameObject tile = Instantiate(prefab, new Vector3(x - 3.5f, 0f, z - 3.5f), Quaternion.identity, fieldBoard);
                tile.name = $"Board_{((x + z) % 2 == 0 ? "B" : "W")}_{x}_{z}";
                if (tile.GetComponent<BoardTileVisual>() == null)
                    tile.AddComponent<BoardTileVisual>();
            }
        CacheBoardTiles();
    }

    [ContextMenu("Clear Board In Editor")]
    public void ClearBoard()
    {
        if (fieldBoard == null) return;
        for (int i = fieldBoard.childCount - 1; i >= 0; i--)
            DestroyImmediate(fieldBoard.GetChild(i).gameObject);
        tiles.Clear();
    }

    public bool TryGetTile(Vector2Int position, out BoardTileVisual tile) => tiles.TryGetValue(position, out tile);

    public IEnumerable<BoardTileVisual> GetAllTiles() => tiles.Values;

    private void CacheBoardTiles()
    {
        tiles.Clear();
        if (fieldBoard == null) return;

        foreach (Transform child in fieldBoard)
        {
            if (!TryParseGridPosition(child.name, out Vector2Int position)) continue;
            BoardTileVisual tile = child.GetComponent<BoardTileVisual>();
            if (tile == null)
                tile = child.gameObject.AddComponent<BoardTileVisual>();
            tiles[position] = tile;
        }
    }

    private static bool TryParseGridPosition(string tileName, out Vector2Int position)
    {
        position = default;
        string[] parts = tileName.Split('_');
        if (parts.Length != 4 || !int.TryParse(parts[2], out int x) || !int.TryParse(parts[3], out int z))
            return false;

        position = new Vector2Int(x, z);
        return x >= 0 && x < 8 && z >= 0 && z < 8;
    }
}
