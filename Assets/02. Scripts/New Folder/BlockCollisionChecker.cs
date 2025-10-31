using System.Collections.Generic;
using UnityEngine;

public class BlockCollisionChecker : MonoBehaviour
{
    private GridMapManager gridMapManager;
    private GridMap gridMap;
    private List<Block> placedBlocks = new List<Block>();

    void Start()
    {
        if (gridMapManager == null)
        {
            gridMapManager = FindObjectOfType<GridMapManager>();
        }
    }

    // 블록 간 충돌 체크
    public bool CheckBlockCollision(Block checkingBlock, Vector2Int targetPosition)
    {
        if (gridMapManager == null) return false;

        if (gridMap == null)
        {
            gridMap = gridMapManager.GetGridMap();
        }

        // 체크 중인 블록의 셀 위치들
        List<Vector2Int> checkingPositions = GetBlockCellPositionsAt(checkingBlock, targetPosition);
        
        // HashSet으로 변환하여 빠른 검색
        HashSet<Vector2Int> checkingPositionsSet = new HashSet<Vector2Int>(checkingPositions);

        // 다른 배치된 블록들과 충돌 체크
        foreach (Block placedBlock in placedBlocks)
        {
            if (placedBlock == checkingBlock) continue; // 자기 자신은 제외

            if (!placedBlock.isPlacedOnGrid) continue;

            // 배치된 블록의 실제 위치 사용 (회전 상태와 무관하게)
            List<Vector2Int> placedPositions = placedBlock.GetLastPlacedPositions();
            
            // 저장된 위치가 없으면 현재 위치 사용
            if (placedPositions.Count == 0)
            {
                placedPositions = placedBlock.GetWorldCellPositions();
            }

            foreach (Vector2Int placedPos in placedPositions)
            {
                if (checkingPositionsSet.Contains(placedPos))
                {
                    return true; // 충돌 발생
                }
            }
        }

        return false;
    }

    // 특정 위치에 블록이 배치되었을 때의 셀 위치들 반환
    List<Vector2Int> GetBlockCellPositionsAt(Block block, Vector2Int gridPosition)
    {
        List<Vector2Int> positions = new List<Vector2Int>();
        BlockData rotatedData = block.GetRotatedData();

        foreach (Vector2Int localPos in rotatedData.cellPositions)
        {
            positions.Add(gridPosition + localPos);
        }

        return positions;
    }

    // 블록 추가 (배치 목록에 등록)
    public void RegisterBlock(Block block)
    {
        if (!placedBlocks.Contains(block))
        {
            placedBlocks.Add(block);
        }
    }

    // 블록 제거 (배치 목록에서 제거)
    public void UnregisterBlock(Block block)
    {
        placedBlocks.Remove(block);
    }

    // 모든 배치된 블록과의 충돌 체크
    public bool CheckAllCollisions(Block checkingBlock, Vector2Int targetPosition)
    {
        // 그리드 범위 체크
        if (gridMap == null) return false;

        List<Vector2Int> cellPositions = GetBlockCellPositionsAt(checkingBlock, targetPosition);
        foreach (Vector2Int pos in cellPositions)
        {
            if (!gridMap.HasCell(pos))
            {
                return true; // 그리드 범위 밖
            }
        }

        // 블록 간 충돌 체크
        return CheckBlockCollision(checkingBlock, targetPosition);
    }
}

