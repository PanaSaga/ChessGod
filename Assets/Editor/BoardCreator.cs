using UnityEngine;
using UnityEditor;

public class BoardCreator
{
    private const string BoardName = "Board";

    [MenuItem("Tools/Chess Board/Paint Existing Board")]
    public static void PaintExistingBoard()
    {
        GameObject board = GameObject.Find(BoardName);

        if (board == null)
        {
            Debug.LogError("Hierarchy에서 Board 오브젝트를 찾지 못했습니다.");
            return;
        }

        Material whiteMaterial = FindMaterial("Board_White");
        Material blackMaterial = FindMaterial("Board_Black");

        if (whiteMaterial == null)
        {
            Debug.LogError("Board_White 머티리얼을 찾지 못했습니다.");
            return;
        }

        if (blackMaterial == null)
        {
            Debug.LogError("Board_Black 머티리얼을 찾지 못했습니다.");
            return;
        }

        int paintedCount = 0;

        for (int z = 0; z < 8; z++)
        {
            for (int x = 0; x < 8; x++)
            {
                Transform tile = board.transform.Find($"Tile_{z}_{x}");

                if (tile == null)
                {
                    Debug.LogWarning($"Tile_{z}_{x} 오브젝트를 찾지 못했습니다.");
                    continue;
                }

                MeshRenderer meshRenderer = tile.GetComponent<MeshRenderer>();

                if (meshRenderer == null)
                {
                    Debug.LogWarning($"Tile_{z}_{x}에 MeshRenderer가 없습니다.");
                    continue;
                }

                Undo.RecordObject(meshRenderer, "Paint Chess Board");

                if ((x + z) % 2 == 0)
                {
                    meshRenderer.sharedMaterial = whiteMaterial;
                }
                else
                {
                    meshRenderer.sharedMaterial = blackMaterial;
                }

                paintedCount++;
            }
        }

        Debug.Log($"체스판 머티리얼 적용 완료: {paintedCount}개");
    }

    private static Material FindMaterial(string materialName)
    {
        string[] guids = AssetDatabase.FindAssets(
            $"{materialName} t:Material"
        );

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            Material material =
                AssetDatabase.LoadAssetAtPath<Material>(path);

            if (material != null && material.name == materialName)
            {
                return material;
            }
        }

        return null;
    }
}