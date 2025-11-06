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

        // 블록의 현재 그리드 위치 사용 (드래그 중에는 이미 스냅됨)
        Vector2Int gridPos = block.gridPosition;

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
            Debug.Log($"✅ Block '{block.blockData.blockName}' successfully placed at {gridPos}");
        }
        else
        {
            // 배치 불가능!
            Debug.LogWarning($"❌ Cannot place block '{block.blockData.blockName}' at {gridPos} - position occupied!");

            // ✨ FIX: 배치 실패 시 아무것도 하지 않음!
            // BlockDragger가 원래 위치로 복귀를 처리함
            // 여기서 transform.position을 변경하면 block.gridPosition이 이미 새 위치라서 문제 발생
        }
    }

    public bool CanPlaceBlockAt(Vector2Int gridPos, Block block, List<Vector2Int> cellPositions)
    {
        if (gridMap == null) return false;

        // 현재 블록의 기존 위치들 (충돌 체크에서 제외하기 위해)
        HashSet<Vector2Int> currentBlockPositions = new HashSet<Vector2Int>();
        if (block.isPlacedOnGrid)
        {
            List<Vector2Int> oldPositions = GetBlockCurrentPositions(block);
            foreach (Vector2Int pos in oldPositions)
            {
                currentBlockPositions.Add(pos);
            }
        }

        // 각 셀 위치가 유효한지 확인
        foreach (Vector2Int cellPos in cellPositions)
        {
            // 그리드에 셀이 있는지 확인
            if (!gridMap.HasCell(cellPos))
            {
                return false;
            }

            // ✨ OPTION 3: GridMapManager의 IsCellOccupied()를 사용하여 CellCollider 체크
            if (gridMapManager != null && gridMapManager.IsCellOccupied(cellPos))
            {
                // 자기 자신의 Cell은 허용
                if (!currentBlockPositions.Contains(cellPos))
                {
                    // CellCollider에서 점유 블록 확인
                    GameObject cellObj = gridMapManager.GetCellGameObject(cellPos);
                    if (cellObj != null)
                    {
                        CellCollider cellCollider = cellObj.GetComponent<CellCollider>();
                        if (cellCollider != null)
                        {
                            Block occupyingBlock = cellCollider.GetOccupyingBlock();
                            if (occupyingBlock != null && occupyingBlock != block)
                            {
                                // 다른 블록이 이 Cell을 점유하고 있음!
                                Debug.LogWarning($"[CellCollider Block] Cell {cellPos} is occupied by '{occupyingBlock.blockData.blockName}'");
                                return false; // ❌ 배치 불가!
                            }
                        }
                    }
                }
            }

            // GridMap.IsOccupied 체크도 유지 (이중 검증)
            if (gridMap.IsOccupied(cellPos))
            {
                // 현재 블록이 이미 배치되어 있고 해당 위치가 현재 블록의 일부인지 확인
                if (currentBlockPositions.Contains(cellPos))
                {
                    continue; // 현재 블록의 일부이면 통과
                }
                else
                {
                    Debug.LogWarning($"[GridMap Block] Cell {cellPos} is occupied");
                    return false; // 다른 블록이 차지하고 있음
                }
            }
        }

        // 충돌 체커 사용 (블록 간 충돌 체크) - 추가 안전장치
        BlockCollisionChecker collisionChecker = FindObjectOfType<BlockCollisionChecker>();
        if (collisionChecker != null)
        {
            if (collisionChecker.CheckBlockCollision(block, gridPos))
            {
                Debug.LogWarning($"[BlockCollisionChecker] Collision detected at {gridPos}");
                return false;
            }
        }

        return true; // ✅ 배치 가능!
    }


    List<Vector2Int> GetBlockCurrentPositions(Block block)
    {
        // 블록이 배치되어 있을 때의 실제 그리드 위치 반환
        if (!block.isPlacedOnGrid)
        {
            return new List<Vector2Int>();
        }
        
        // 마지막으로 배치된 위치 반환 (회전 등으로 인한 변화 반영)
        List<Vector2Int> lastPositions = block.GetLastPlacedPositions();
        if (lastPositions.Count > 0)
        {
            return lastPositions;
        }
        
        // 저장된 위치가 없으면 현재 위치 사용
        return block.GetWorldCellPositions();
    }

    public void PlaceBlockOnGrid(Block block, Vector2Int gridPos)
    {
        if (gridMap == null) return;

        Debug.Log($"📍 PlaceBlockOnGrid: Placing '{block.blockData.blockName}' at {gridPos}, isPlacedOnGrid={block.isPlacedOnGrid}");

        // ✨ CRITICAL FIX: 기존 위치에서 제거 (이미 배치된 경우)
        // GridMapManager.OnBlockRemoved()를 호출하여 CellCollider도 함께 해제
        if (block.isPlacedOnGrid)
        {
            Debug.Log($"  🔄 Block was already placed. Removing from old position first...");

            // ✨ RemoveBlockFromGrid를 호출하여 완전히 제거
            // (GridMap.SetOccupied + GridMapManager.OnBlockRemoved)
            RemoveBlockFromGrid(block);
        }

        // 새로운 위치 설정
        block.gridPosition = gridPos;
        block.transform.position = new Vector3(gridPos.x, gridPos.y, 0);

        // 그리드 셀들을 occupied로 표시 (배치 전에 충돌 체크가 완료되었으므로 안전)
        List<Vector2Int> cellPositions = block.GetWorldCellPositions();

        // 배치된 위치 저장
        block.SetPlacedPositions(cellPositions);

        Debug.Log($"  📌 Setting {cellPositions.Count} cells as occupied in GridMap...");
        foreach (Vector2Int cellPos in cellPositions)
        {
            gridMap.SetOccupied(cellPos, true);
        }

        block.isPlacedOnGrid = true;

        // GridMapManager에 블록 정보 저장 (마지막에 수행)
        // 여기서 CellCollider도 활성화됨
        if (gridMapManager != null)
        {
            Debug.Log($"  🔔 Calling GridMapManager.OnBlockPlaced()...");
            gridMapManager.OnBlockPlaced(block);
        }

        Debug.Log($"✅ PlaceBlockOnGrid complete: '{block.blockData.blockName}' placed at {gridPos}");
    }

    public void RemoveBlockFromGrid(Block block)
    {
        if (gridMap == null) return;

        if (!block.isPlacedOnGrid) return;

        // 마지막으로 배치된 위치 사용
        List<Vector2Int> cellPositions = block.GetLastPlacedPositions();
        if (cellPositions.Count == 0)
        {
            // 저장된 위치가 없으면 현재 위치 사용
            cellPositions = block.GetWorldCellPositions();
        }

        foreach (Vector2Int cellPos in cellPositions)
        {
            gridMap.SetOccupied(cellPos, false);
        }

        block.isPlacedOnGrid = false;
        block.SetPlacedPositions(new List<Vector2Int>()); // 위치 초기화

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

