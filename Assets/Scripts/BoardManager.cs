using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [Header("Tile Prefabs")]
    public GameObject boardWPrefab; // 백색 타일 원본 (Board_W)
    public GameObject boardBPrefab; // 흑색 타일 원본 (Board_B)

    [Header("Board_Setting")]
    public Transform fieldBoard; // 타일들을 묶어줄 부모 오브젝트 (Field_Board)

    // [컴포넌트 우클릭 메뉴] 에디터에서 보드 생성
    [ContextMenu("Generate Board In Editor")]
    public void GenerateBoard()
    {
        // 중복 생성 방지를 위해 기존 타일 먼저 제거
        ClearBoard();

        if (fieldBoard == null)
        {
            Debug.LogError("Field_Board not found! check Inspector.");
            return;
        }

        for (int z = 0; z < 8; z++)
        {
            for (int x = 0; x < 8; x++)
            {
                GameObject selectedPrefab = ((x + z) % 2 == 0) ? boardBPrefab : boardWPrefab;

                if (selectedPrefab != null)
                {
                    // [수정됨] 0~7 좌표에서 3.5를 빼서 정중앙(0,0,0)을 기준으로 배치되게 합니다.
                    Vector3 spawnPosition = new Vector3(x - 3.5f, 0, z - 3.5f);

                    GameObject newTile = Instantiate(selectedPrefab, spawnPosition, Quaternion.identity);

                    newTile.transform.SetParent(fieldBoard);

                    string tileColorTag = ((x + z) % 2 == 0) ? "W" : "B";
                    // 이름은 논리 좌표인 0~7을 그대로 유지합니다. (예: Board_W_0_0)
                    newTile.name = $"Board_{tileColorTag}_{x}_{z}";
                }
            }
        }

        Debug.Log("Create board in InGame Scene!");
    }

    // [컴포넌트 우클릭 메뉴] 에디터에서 보드 삭제
    [ContextMenu("Clear Board In Editor")]
    public void ClearBoard()
    {
        if (fieldBoard == null) return;

        // 에디터 상태에서 자식 오브젝트들을 즉시 안전하게 삭제
        for (int i = fieldBoard.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(fieldBoard.GetChild(i).gameObject);
        }

        Debug.Log("reset board!");
    }
}