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
        
        transform.position = worldPos + offset;

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

