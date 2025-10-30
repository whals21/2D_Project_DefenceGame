using System.Collections.Generic;
using UnityEngine;

public class BlockPlacer : MonoBehaviour
{
    public GridMapManager gridMapManager;
    public Material previewMaterial; // 미리보기용 반투명 머티리얼
    private GridMap gridMap;
    private Block currentPreviewBlock;
    private List<GameObject> previewObjects = new List<GameObject>();

    void Start()
    {
        if (gridMapManager == null)
        {
            gridMapManager = FindObjectOfType<GridMapManager>();
        }
    }

    // 블록 미리보기 업데이트
    public void UpdateBlockPreview(Block block)
    {
        if (gridMapManager == null || block == null) return;

        if (gridMap == null)
        {
            gridMap = gridMapManager.GetGridMap();
        }

        // 기존 미리보기 제거
        ClearPreview();

        // 마우스 위치를 그리드 좌표로 변환
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPos.z = 0;
        Vector2Int gridPos = WorldToGridPosition(mouseWorldPos);

        // 블록이 그리드 범위 내에 있는지 확인
        List<Vector2Int> cellPositions = block.GetWorldCellPositionsAt(gridPos);
        bool canPlace = CanPlaceBlockAt(gridPos, block, cellPositions);

        // 미리보기 생성
        CreatePreview(block, gridPos, canPlace);
    }

    Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
    }

    // 블록 배치 시도
    public void TryPlaceBlock(Block block)
    {
        if (gridMapManager == null || block == null) return;

        if (gridMap == null)
        {
            gridMap = gridMapManager.GetGridMap();
        }

        ClearPreview();

        Vector3 worldPos = block.transform.position;
        Vector2Int gridPos = WorldToGridPosition(worldPos);
        List<Vector2Int> cellPositions = block.GetWorldCellPositionsAt(gridPos);

        // 배치 가능 여부 확인
        if (CanPlaceBlockAt(gridPos, block, cellPositions))
        {
            PlaceBlockOnGrid(block, gridPos);
        }
        else
        {
            // 배치 불가능하면 원래 위치로 되돌림
            if (block.isPlacedOnGrid)
            {
                // 이미 배치된 블록이면 원래 그리드 위치로 복귀
                block.transform.position = new Vector3(block.gridPosition.x, block.gridPosition.y, 0);
            }
            else
            {
                // 새 블록이면 그리드 밖으로 이동 (또는 원하는 위치로)
                block.transform.position = worldPos;
            }
        }
    }

    bool CanPlaceBlockAt(Vector2Int gridPos, Block block, List<Vector2Int> cellPositions)
    {
        if (gridMap == null) return false;

        // 충돌 체커 사용
        BlockCollisionChecker collisionChecker = FindObjectOfType<BlockCollisionChecker>();
        if (collisionChecker != null)
        {
            if (collisionChecker.CheckAllCollisions(block, gridPos))
            {
                return false;
            }
        }

        // 각 셀 위치가 유효한지 확인
        List<Vector2Int> currentBlockPositions = block.isPlacedOnGrid ? GetBlockCurrentPositions(block) : new List<Vector2Int>();

        foreach (Vector2Int cellPos in cellPositions)
        {
            // 그리드에 셀이 있는지 확인
            if (!gridMap.HasCell(cellPos))
            {
                return false;
            }

            // 이미 다른 블록이 배치되어 있는지 확인 (현재 블록 제외)
            if (gridMap.IsOccupied(cellPos))
            {
                // 현재 블록이 이미 배치되어 있고 해당 위치가 현재 블록의 일부인지 확인
                if (block.isPlacedOnGrid && currentBlockPositions.Contains(cellPos))
                {
                    continue; // 현재 블록의 일부이면 통과
                }
                else
                {
                    return false; // 다른 블록이 차지하고 있음
                }
            }
        }

        return true;
    }

    List<Vector2Int> GetBlockCurrentPositions(Block block)
    {
        // 현재 그리드에 배치된 블록의 위치 반환
        // 이 정보는 GridMap에 저장되어야 하지만, 일단 간단하게 처리
        return block.GetWorldCellPositions();
    }

    void PlaceBlockOnGrid(Block block, Vector2Int gridPos)
    {
        if (gridMap == null) return;

        // 기존 위치에서 제거 (이미 배치된 경우)
        if (block.isPlacedOnGrid)
        {
            List<Vector2Int> oldPositions = GetBlockCurrentPositions(block);
            foreach (Vector2Int oldPos in oldPositions)
            {
                gridMap.SetOccupied(oldPos, false);
            }
        }

        block.gridPosition = gridPos;
        block.transform.position = new Vector3(gridPos.x, gridPos.y, 0);
        block.isPlacedOnGrid = true;

        // 그리드 셀들을 occupied로 표시
        List<Vector2Int> cellPositions = block.GetWorldCellPositions();
        foreach (Vector2Int cellPos in cellPositions)
        {
            gridMap.SetOccupied(cellPos, true);
        }

        // GridMapManager에 블록 정보 저장
        if (gridMapManager != null)
        {
            gridMapManager.OnBlockPlaced(block);
        }
    }

    public void RemoveBlockFromGrid(Block block)
    {
        if (gridMap == null) return;

        if (!block.isPlacedOnGrid) return;

        // 그리드 셀들을 비워줌
        List<Vector2Int> cellPositions = block.GetWorldCellPositions();
        foreach (Vector2Int cellPos in cellPositions)
        {
            gridMap.SetOccupied(cellPos, false);
        }

        block.isPlacedOnGrid = false;

        // GridMapManager에 블록 제거 알림 (필요시)
        if (gridMapManager != null)
        {
            gridMapManager.OnBlockRemoved(block);
        }
    }

    void CreatePreview(Block block, Vector2Int gridPos, bool canPlace)
    {
        if (block.blockData == null || block.cellPrefab == null) return;

        BlockData rotatedData = block.GetRotatedData();
        Color previewColor = canPlace ? new Color(0, 1, 0, 0.5f) : new Color(1, 0, 0, 0.5f);

        foreach (Vector2Int localPos in rotatedData.cellPositions)
        {
            Vector3 worldPos = new Vector3(gridPos.x + localPos.x, gridPos.y + localPos.y, 0);
            GameObject previewObj = Instantiate(block.cellPrefab, worldPos, Quaternion.identity);
            previewObj.GetComponent<SpriteRenderer>().color = previewColor;
            previewObjects.Add(previewObj);
        }
    }

    public void ClearPreview()
    {
        foreach (GameObject obj in previewObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        previewObjects.Clear();
    }

    void OnDestroy()
    {
        ClearPreview();
    }
}

