using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    public BlockData blockData;
    public Vector2Int gridPosition; // 그리드상의 위치 (중심점 또는 기준점)
    public int rotation = 0; // 0, 1, 2, 3 (0도, 90도, 180도, 270도)
    public bool isPlacedOnGrid = false; // 그리드에 배치되어 있는지 여부
    public GameObject cellPrefab;
    private List<GameObject> cellObjects = new List<GameObject>();
    private BlockData currentRotatedData;
    private List<Vector2Int> lastPlacedPositions = new List<Vector2Int>(); // 마지막으로 배치된 위치들 저장

    void Start()
    {
        if (blockData != null)
        {
            UpdateVisualization();
        }
    }

    // 블록의 셀 위치들을 월드 좌표로 반환 (현재 gridPosition 기준)
    public List<Vector2Int> GetWorldCellPositions()
    {
        List<Vector2Int> worldPositions = new List<Vector2Int>();
        BlockData rotatedData = GetRotatedData();

        foreach (Vector2Int localPos in rotatedData.cellPositions)
        {
            worldPositions.Add(gridPosition + localPos);
        }
        return worldPositions;
    }

    // 특정 위치에서의 블록 셀 위치들 반환
    public List<Vector2Int> GetWorldCellPositionsAt(Vector2Int targetGridPos)
    {
        List<Vector2Int> worldPositions = new List<Vector2Int>();
        BlockData rotatedData = GetRotatedData();

        foreach (Vector2Int localPos in rotatedData.cellPositions)
        {
            worldPositions.Add(targetGridPos + localPos);
        }
        return worldPositions;
    }

    // 현재 회전 상태의 블록 데이터 반환
    public BlockData GetRotatedData()
    {
        if (blockData == null) return null;

        // 캐시된 데이터가 있고 rotation이 변경되지 않았으면 반환
        if (currentRotatedData != null)
        {
            return currentRotatedData;
        }

        BlockData data = new BlockData(blockData.blockName, new List<Vector2Int>(blockData.cellPositions), blockData.blockColor);
        for (int i = 0; i < rotation; i++)
        {
            data = data.Rotate90();
        }
        currentRotatedData = data;
        return data;
    }

    // 블록 회전 (90도씩)
    public void Rotate()
    {
        rotation = (rotation + 1) % 4;
        currentRotatedData = null; // 캐시 초기화
        
        // 그리드에 배치되어 있으면 위치 업데이트
        if (isPlacedOnGrid)
        {
            // 회전 후에도 같은 gridPosition 유지 (시각화만 업데이트)
            UpdateVisualization();
        }
        else
        {
            UpdateVisualization();
        }
    }

    // 블록 시각화 업데이트
    public void UpdateVisualization()
    {
        // 기존 셀 오브젝트 제거
        ClearCells();

        if (blockData == null) return;

        BlockData rotatedData = GetRotatedData();
        Vector3 worldPos = new Vector3(gridPosition.x, gridPosition.y, 0);

        // 각 셀 위치에 오브젝트 생성
        foreach (Vector2Int localPos in rotatedData.cellPositions)
        {
            Vector3 cellWorldPos = worldPos + new Vector3(localPos.x, localPos.y, 0);
            GameObject cellObj = Instantiate(cellPrefab, cellWorldPos, Quaternion.identity, transform);
            cellObj.GetComponent<SpriteRenderer>().color = blockData.blockColor;
            cellObjects.Add(cellObj);
        }
    }

    void ClearCells()
    {
        foreach (GameObject cell in cellObjects)
        {
            if (cell != null)
            {
                Destroy(cell);
            }
        }
        cellObjects.Clear();
    }

    // 마지막으로 배치된 위치들 반환
    public List<Vector2Int> GetLastPlacedPositions()
    {
        return new List<Vector2Int>(lastPlacedPositions);
    }

    // 배치된 위치 저장
    public void SetPlacedPositions(List<Vector2Int> positions)
    {
        lastPlacedPositions = new List<Vector2Int>(positions);
    }
}

