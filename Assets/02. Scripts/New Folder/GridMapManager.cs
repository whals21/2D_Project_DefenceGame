using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class GridMapManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    private GridMap map;
    public GameObject cellPrefab;
    public GameObject ghostCellPrefab;


    void Start()
    {
        map = new GridMap(width, height);
        Debug.Log("맵 생성 완료: " + width + "x" + height);
        VisualizeGrid();

    }

    void VisualizeGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                Instantiate(cellPrefab, pos, Quaternion.identity, transform);
            }
        }
    }

    public void ShowExpandableCells()
    {
        List<Vector2Int> expandablePositions = GetExpandablePositions();

        foreach (Vector2Int pos in expandablePositions)
        {
            GameObject ghostCell = Instantiate(ghostCellPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);
            ghostCell.GetComponent<Button>().onClick.AddListener(() => ExpandAt(pos));
        }
    }

    public List<Vector2Int> GetExpandablePositions()
    {
        List<Vector2Int> positions = new List<Vector2Int>();

        // 상하좌우 외곽 셀 기준으로 확장 가능 위치 계산
        for (int x = -1; x <= map.width; x++)
        {
            positions.Add(new Vector2Int(x, -1)); // 아래
            positions.Add(new Vector2Int(x, map.height)); // 위
        }
        for (int y = 0; y < map.height; y++)
        {
            positions.Add(new Vector2Int(-1, y)); // 왼쪽
            positions.Add(new Vector2Int(map.width, y)); // 오른쪽
        }

        return positions;
    }

    public void ExpandAt(Vector2Int pos)
    {
        // 방향 판단
        if (pos.y == -1) map.ExpandBottom();
        else if (pos.y == map.height) map.ExpandTop();
        else if (pos.x == -1) map.ExpandLeft();
        else if (pos.x == map.width) map.ExpandRight();

        // 기존 셀 제거 & 재생성
        RedrawMap();
    }

}