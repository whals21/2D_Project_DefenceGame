using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Bounds 기반 블록 드래그 시스템
/// Collider2D 없이 블록의 공간 범위(Bounds)를 계산하여 클릭 및 드래그 감지
/// </summary>
public class BlockDragger : MonoBehaviour
{
    private Block block;
    private bool isDragging = false;
    private Vector3 dragOffset; // 마우스와 블록 중심의 오프셋
    private Camera mainCamera;
    private BlockPlacer blockPlacer;
    private BlockCollisionChecker collisionChecker;
    private GridMapManager gridMapManager;

    // Bounds 관련 필드
    private Bounds blockBounds; // 블록의 실제 공간 범위
    private const float CELL_SIZE = 1.0f; // 그리드 셀 한 칸의 크기

    // ✨ FIX: 드래그 시작 시 원래 위치 저장 (배치 실패 시 복귀용)
    private Vector2Int originalGridPosition;
    private bool hadOriginalPosition = false;

    // ✨ NEW: 블록의 최초 스폰 위치 저장 (배치 실패 시 그리드 밖으로 복귀)
    private Vector3 initialSpawnPosition;
    private bool hasInitialSpawnPosition = false;

    void Start()
    {
        block = GetComponent<Block>();
        mainCamera = Camera.main;
        blockPlacer = FindObjectOfType<BlockPlacer>();
        collisionChecker = FindObjectOfType<BlockCollisionChecker>();
        gridMapManager = FindObjectOfType<GridMapManager>();

        // ✨ NEW: 최초 스폰 위치 저장 (그리드 밖의 위치)
        initialSpawnPosition = transform.position;
        hasInitialSpawnPosition = true;
        Debug.Log($"[Start] Initial spawn position saved: {initialSpawnPosition}");

        // 초기 Bounds 계산
        UpdateBlockBounds();
    }

    void Update()
    {
        // 마우스 월드 좌표 계산
        Vector3 mouseWorldPos = GetMouseWorldPosition();

        // 마우스 왼쪽 버튼 클릭 (드래그 시작)
        if (Input.GetMouseButtonDown(0))
        {
            TryStartDrag(mouseWorldPos);
        }
        // 드래그 중
        else if (Input.GetMouseButton(0) && isDragging)
        {
            PerformDrag(mouseWorldPos);
        }
        // 마우스 버튼 릴리즈 (드래그 종료)
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndDrag();
        }

        // 드래그 중 회전 키로 회전
        if (isDragging && KeyBindingManager.Instance != null && KeyBindingManager.Instance.GetRotateBlockKeyDown())
        {
            RotateBlock();
        }
    }

    /// <summary>
    /// 블록의 모든 셀을 포함하는 Bounds를 계산
    /// AABB (Axis-Aligned Bounding Box) 방식 사용
    /// </summary>
    void UpdateBlockBounds()
    {
        // 블록이 차지하는 모든 셀의 월드 좌표 가져오기
        List<Vector2Int> cellPositions = block.GetWorldCellPositions();

        if (cellPositions.Count == 0)
        {
            // 셀이 없으면 기본 Bounds 설정
            blockBounds = new Bounds(transform.position, Vector3.one * CELL_SIZE);
            return;
        }

        // 모든 셀을 포함하는 최소/최대 좌표 찾기
        Vector3 minPoint = new Vector3(float.MaxValue, float.MaxValue, 0);
        Vector3 maxPoint = new Vector3(float.MinValue, float.MinValue, 0);

        foreach (Vector2Int pos in cellPositions)
        {
            // 셀의 중심 좌표
            Vector3 cellCenter = new Vector3(pos.x, pos.y, 0);

            // 셀의 범위 (중심 ± 0.5f)
            Vector3 cellMin = cellCenter - Vector3.one * (CELL_SIZE * 0.5f);
            Vector3 cellMax = cellCenter + Vector3.one * (CELL_SIZE * 0.5f);

            // 전체 범위 업데이트
            minPoint = Vector3.Min(minPoint, cellMin);
            maxPoint = Vector3.Max(maxPoint, cellMax);
        }

        // Bounds 생성 및 설정
        blockBounds = new Bounds();
        blockBounds.SetMinMax(minPoint, maxPoint);

        // 디버그 로그 (선택사항)
        // Debug.Log($"Block Bounds Updated: Center={blockBounds.center}, Size={blockBounds.size}");
    }

    /// <summary>
    /// 마우스 클릭 시 Bounds 체크로 드래그 시작 시도
    /// </summary>
    void TryStartDrag(Vector3 mouseWorldPos)
    {
        // Bounds 최신 상태로 업데이트
        UpdateBlockBounds();

        // 마우스가 블록의 Bounds 안에 있는지 확인
        if (blockBounds.Contains(mouseWorldPos))
        {
            // 드래그 시작!
            isDragging = true;

            // ✨ FIX: 드래그 시작 전에 원래 위치 저장!
            if (block.isPlacedOnGrid)
            {
                originalGridPosition = block.gridPosition;
                hadOriginalPosition = true;
                Debug.Log($"[TryStartDrag] Original position saved: {originalGridPosition}");
            }
            else
            {
                hadOriginalPosition = false;
            }

            // transform.position을 gridPosition에 맞게 정렬
            // 처음 생성된 블록의 경우 소수점 좌표일 수 있으므로 정수 좌표로 스냅
            Vector2Int currentGridPos = WorldToGridPosition(transform.position);
            block.gridPosition = currentGridPos;
            transform.position = new Vector3(currentGridPos.x, currentGridPos.y, 0);

            // 마우스와 블록 중심의 오프셋 계산 (드래그 중 블록 위치 보정용)
            // 이제 transform.position이 정수 좌표이므로 오프셋도 정확해짐
            dragOffset = transform.position - mouseWorldPos;

            // 그리드에 배치된 블록이면 먼저 제거
            if (block != null && block.isPlacedOnGrid && blockPlacer != null)
            {
                blockPlacer.RemoveBlockFromGrid(block);
            }

            Debug.Log($"Drag Started! Mouse at {mouseWorldPos}, Block at {transform.position}, GridPos: {currentGridPos}");
        }
    }

    /// <summary>
    /// 드래그 중 블록 이동 처리
    /// </summary>
    void PerformDrag(Vector3 mouseWorldPos)
    {
        // 마우스 위치 + 오프셋으로 블록의 목표 위치 계산
        Vector3 targetWorldPos = mouseWorldPos + dragOffset;

        // 그리드 좌표로 스냅 (정수 좌표로 변환)
        Vector2Int gridPos = WorldToGridPosition(targetWorldPos);
        Vector3 snappedPos = new Vector3(gridPos.x, gridPos.y, 0);

        // 블록 이동
        transform.position = snappedPos;
        block.gridPosition = gridPos;

        // Bounds 업데이트 (블록이 이동했으므로)
        UpdateBlockBounds();

        // 배치 미리보기 업데이트 (초록색/빨간색 표시)
        if (blockPlacer != null)
        {
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

    /// <summary>
    /// 드래그 종료 처리
    /// </summary>
    void EndDrag()
    {
        isDragging = false;

        // 그리드에 배치 시도
        if (blockPlacer != null)
        {
            // 그리드 밖에 있으면 블록 제거
            if (IsOutsideGrid())
            {
                Debug.Log("Block is outside grid. Destroying...");
                Destroy(gameObject);
                return;
            }

            // ✨ FIX: 배치 전 상태 저장
            Vector2Int attemptedPosition = block.gridPosition;
            bool wasPlaced = block.isPlacedOnGrid;

            // 그리드에 배치 시도
            blockPlacer.TryPlaceBlock(block);

            // ✨ NEW: 배치 실패 시 처리 변경 - 스폰 위치로 복귀!
            if (!block.isPlacedOnGrid)
            {
                // 배치에 실패했음 (isPlacedOnGrid가 여전히 false)
                if (hadOriginalPosition)
                {
                    // ✨ 원래 배치되어 있던 블록 → 스폰 위치로 복귀 (그리드 밖)
                    if (hasInitialSpawnPosition)
                    {
                        transform.position = initialSpawnPosition;
                        block.gridPosition = WorldToGridPosition(initialSpawnPosition);
                        block.isPlacedOnGrid = false; // 그리드에 배치 안 된 상태로
                        Debug.Log($"[EndDrag] ⚠️ Placement failed! Block returned to spawn position: {initialSpawnPosition}");
                    }
                    else
                    {
                        // 스폰 위치가 없으면 원래 위치로 복귀 (백업)
                        block.gridPosition = originalGridPosition;
                        transform.position = new Vector3(originalGridPosition.x, originalGridPosition.y, 0);

                        List<Vector2Int> originalCellPositions = block.GetWorldCellPositionsAt(originalGridPosition);
                        if (blockPlacer.CanPlaceBlockAt(originalGridPosition, block, originalCellPositions))
                        {
                            blockPlacer.PlaceBlockOnGrid(block, originalGridPosition);
                            Debug.Log($"[EndDrag] Block returned to original grid position: {originalGridPosition}");
                        }
                        else
                        {
                            Debug.LogError($"[EndDrag] Cannot return to original position! {originalGridPosition}");
                        }
                    }
                }
                else
                {
                    // ✨ 새로 생성된 블록 (처음 배치 시도) → 스폰 위치로 복귀
                    if (hasInitialSpawnPosition)
                    {
                        transform.position = initialSpawnPosition;
                        block.gridPosition = WorldToGridPosition(initialSpawnPosition);
                        block.isPlacedOnGrid = false;
                        Debug.Log($"[EndDrag] ⚠️ New block placement failed! Returned to spawn position: {initialSpawnPosition}");
                    }
                    else
                    {
                        Debug.Log($"[EndDrag] New block placement failed. Keeping at {attemptedPosition}");
                    }
                }
            }
            else
            {
                Debug.Log($"✅ Drag Ended - Block successfully placed at {block.gridPosition}");
            }
        }

        // 원래 위치 정보 초기화
        hadOriginalPosition = false;
    }

    /// <summary>
    /// 블록 회전 처리
    /// </summary>
    void RotateBlock()
    {
        block.Rotate();

        // Bounds 업데이트 (회전으로 모양이 바뀌었으므로)
        UpdateBlockBounds();

        // 그리드에 배치되어 있으면 재배치 시도
        if (block.isPlacedOnGrid && blockPlacer != null)
        {
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
                // 배치 불가능하면 원래 회전 상태로 복귀 (역회전 3번)
                block.Rotate();
                block.Rotate();
                block.Rotate();
                UpdateBlockBounds();
            }
        }

        // 미리보기 업데이트
        if (blockPlacer != null)
        {
            blockPlacer.UpdateBlockPreview(block);
        }
    }

    /// <summary>
    /// 마우스의 스크린 좌표를 월드 좌표로 변환
    /// </summary>
    Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null) return Vector3.zero;

        Vector3 mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        mouseWorldPos.z = 0; // 2D이므로 z는 0으로 고정

        return mouseWorldPos;
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환 (정수 좌표)
    /// </summary>
    Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        return new Vector2Int(Mathf.RoundToInt(worldPos.x), Mathf.RoundToInt(worldPos.y));
    }

    /// <summary>
    /// 블록이 그리드 범위 밖에 있는지 확인
    /// </summary>
    bool IsOutsideGrid()
    {
        if (gridMapManager == null) return false;

        GridMap gridMap = gridMapManager.GetGridMap();
        if (gridMap == null) return false;

        // 블록의 모든 셀 위치 확인
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

    /// <summary>
    /// 디버그용: Bounds 시각화 (Scene 뷰에서만 보임)
    /// </summary>
    void OnDrawGizmos()
    {
        if (isDragging)
        {
            // 드래그 중일 때 Bounds를 시각화
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(blockBounds.center, blockBounds.size);
        }
    }
}

