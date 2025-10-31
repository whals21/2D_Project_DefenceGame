using UnityEngine;
using System.Collections.Generic;

public class BlockDragger : MonoBehaviour
{
    private Block block;
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private BlockPlacer blockPlacer;
    private BlockCollisionChecker collisionChecker;
    private GridMapManager gridMapManager;

    void Start()
    {
        block = GetComponent<Block>();
        mainCamera = Camera.main;
        blockPlacer = FindObjectOfType<BlockPlacer>();
        collisionChecker = FindObjectOfType<BlockCollisionChecker>();
        gridMapManager = FindObjectOfType<GridMapManager>();
    }

    void Update()
    {
        // 드래그 중 R키로 회전
        if (isDragging && Input.GetKeyDown(KeyCode.R))
        {
            block.Rotate();
            
            // 그리드에 배치되어 있으면 위치 업데이트 필요
            if (block.isPlacedOnGrid && blockPlacer != null)
            {
                // 회전 후 위치 재계산
                Vector2Int gridPos = block.gridPosition;
                List<Vector2Int> cellPositions = block.GetWorldCellPositionsAt(gridPos);
                
                // 배치 가능 여부 확인
                if (blockPlacer.CanPlaceBlockAt(gridPos, block, cellPositions))
                {
                    // 배치 가능하면 다시 배치
                    blockPlacer.PlaceBlockOnGrid(block, gridPos);
                }
                else
                {
                    // 배치 불가능하면 원래 위치로 복귀
                    block.Rotate();
                    block.Rotate();
                    block.Rotate(); // 3번 더 회전 = 원래 위치
                }
            }
            
            if (blockPlacer != null)
            {
                blockPlacer.UpdateBlockPreview(block);
            }
        }
    }

    void OnMouseDown()
    {
        if (mainCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        
        offset = transform.position - worldPos;
        isDragging = true;

        // 그리드에 배치된 블록이면 먼저 제거
        if (block != null && block.isPlacedOnGrid && blockPlacer != null)
        {
            blockPlacer.RemoveBlockFromGrid(block);
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging || mainCamera == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        
        // 그리드에 스냅
        Vector2Int gridPos = WorldToGridPosition(worldPos);
        Vector3 snappedPos = new Vector3(gridPos.x, gridPos.y, 0);
        
        // 드래그 중에는 블록을 그리드 위치에 스냅
        transform.position = snappedPos;
        block.gridPosition = gridPos;

        // 그리드에 스냅 및 미리보기 업데이트
        if (blockPlacer != null)
        {
            // 그리드 밖이면 미리보기 제거
            if (IsOutsideGrid())
            {
                blockPlacer.ClearPreview();
            }
            else
            {
                blockPlacer.UpdateBlockPreview(block);
            }
        }
    }

    Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
    }

    void OnMouseUp()
    {
        if (!isDragging) return;

        isDragging = false;

        // 그리드에 배치 시도
        if (blockPlacer != null)
        {
            // 그리드 밖에 있으면 블록 제거 (이미 배치된 경우)
            if (IsOutsideGrid() && block.isPlacedOnGrid)
            {
                blockPlacer.RemoveBlockFromGrid(block);
            }
            else
            {
                blockPlacer.TryPlaceBlock(block);
            }
        }
    }

    bool IsOutsideGrid()
    {
        if (gridMapManager == null) return false;

        GridMap gridMap = gridMapManager.GetGridMap();
        if (gridMap == null) return false;

        // 블록의 셀 위치들이 모두 그리드 밖에 있는지 확인
        List<Vector2Int> cellPositions = block.GetWorldCellPositions();
        foreach (Vector2Int pos in cellPositions)
        {
            if (gridMap.HasCell(pos))
            {
                return false; // 하나라도 그리드 안에 있으면 false
            }
        }
        return true; // 모든 셀이 그리드 밖에 있으면 true
    }
}

