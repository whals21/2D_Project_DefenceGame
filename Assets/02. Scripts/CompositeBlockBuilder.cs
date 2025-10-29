using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// (1,1) 크기의 ItemPrefab을 조합하여 다양한 모양의 블록 생성
/// 각 셀은 독립적인 BoxCollider2D를 가지므로 정확한 클릭 및 배치 가능
/// </summary>
public class CompositeBlockBuilder : MonoBehaviour
{
    [Header("References")]
    public GameObject cellPrefab; // (1,1) 크기의 기본 셀 프리팹
    public SpriteInventoryGrid grid;

    [Header("Block Settings")]
    public Sprite blockSprite; // 블록 전체에 사용할 스프라이트 (선택사항)
    public Color blockColor = Color.white;

    /// <summary>
    /// Shape 좌표를 기반으로 복합 블록 생성
    /// </summary>
    public GameObject CreateCompositeBlock(string blockName, Vector2Int[] shape, Sprite sprite, Color color, Vector3 worldPosition)
    {
        if (cellPrefab == null || grid == null)
        {
            Debug.LogError("❌ CellPrefab or Grid is null!");
            return null;
        }

        // 부모 블록 GameObject 생성
        GameObject blockParent = new GameObject($"Block_{blockName}");
        blockParent.transform.position = worldPosition;
        blockParent.transform.SetParent(grid.transform);

        // GridItem 데이터 생성
        int minX = int.MaxValue, minY = int.MaxValue;
        int maxX = int.MinValue, maxY = int.MinValue;

        foreach (Vector2Int cell in shape)
        {
            minX = Mathf.Min(minX, cell.x);
            minY = Mathf.Min(minY, cell.y);
            maxX = Mathf.Max(maxX, cell.x);
            maxY = Mathf.Max(maxY, cell.y);
        }

        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        GridItem blockData = new GridItem(blockName, width, height, sprite);
        blockData.color = color;
        blockData.shape = shape;
        blockData.gridPosition = new Vector2Int(-1, -1); // 그리드 밖

        // 각 셀 생성
        List<GameObject> cells = new List<GameObject>();
        foreach (Vector2Int cellPos in shape)
        {
            GameObject cell = CreateCell(cellPos, sprite, color, blockParent.transform);
            cells.Add(cell);
        }

        // CompositeBlockController 추가
        CompositeBlockController controller = blockParent.AddComponent<CompositeBlockController>();
        controller.blockData = blockData;
        controller.grid = grid;
        controller.cells = cells;
        controller.cellPrefab = cellPrefab;

        // DraggableCompositeBlock 추가 (드래그 기능)
        DraggableCompositeBlock draggable = blockParent.AddComponent<DraggableCompositeBlock>();
        draggable.blockData = blockData;
        draggable.grid = grid;
        draggable.controller = controller;

        Debug.Log($"✅ Created composite block: {blockName} with {shape.Length} cells");

        return blockParent;
    }

    /// <summary>
    /// 개별 셀 생성
    /// </summary>
    GameObject CreateCell(Vector2Int position, Sprite sprite, Color color, Transform parent)
    {
        // 셀의 월드 위치 계산 (부모 기준 로컬 좌표)
        Vector3 localPos = new Vector3(
            position.x * grid.cellSize,
            position.y * grid.cellSize,
            0f
        );

        GameObject cell = Instantiate(cellPrefab, parent);
        cell.transform.localPosition = localPos;
        cell.transform.localScale = Vector3.one * grid.cellSize * 0.95f;
        cell.name = $"Cell_{position.x}_{position.y}";

        // SpriteRenderer 설정
        SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            if (sprite != null)
                sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 10;
        }

        // BoxCollider2D 설정 (각 셀마다 독립적)
        BoxCollider2D collider = cell.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = cell.AddComponent<BoxCollider2D>();
        }
        collider.size = Vector2.one;

        return cell;
    }
}

/// <summary>
/// 복합 블록의 셀들을 관리하는 컨트롤러
/// </summary>
public class CompositeBlockController : MonoBehaviour
{
    public GridItem blockData;
    public SpriteInventoryGrid grid;
    public List<GameObject> cells = new List<GameObject>();
    public GameObject cellPrefab;

    /// <summary>
    /// 블록 회전 (셀들의 위치 재배치)
    /// </summary>
    public void RotateBlock()
    {
        if (blockData == null || !blockData.HasShape()) return;

        // GridItem 회전
        blockData.Rotate();

        // 셀들의 위치 업데이트
        UpdateCellPositions();
    }

    /// <summary>
    /// 회전된 shape에 맞게 셀들의 로컬 위치 업데이트
    /// </summary>
    public void UpdateCellPositions()
    {
        Vector2Int[] rotatedShape = GetRotatedShape();

        for (int i = 0; i < cells.Count && i < rotatedShape.Length; i++)
        {
            Vector2Int cellPos = rotatedShape[i];
            Vector3 localPos = new Vector3(
                cellPos.x * grid.cellSize,
                cellPos.y * grid.cellSize,
                0f
            );

            cells[i].transform.localPosition = localPos;
        }
    }

    /// <summary>
    /// 현재 회전 상태의 shape 좌표 반환
    /// </summary>
    Vector2Int[] GetRotatedShape()
    {
        if (blockData == null || !blockData.HasShape())
            return new Vector2Int[0];

        Vector2Int[] shape = blockData.shape;
        int rotation = blockData.rotation;

        switch (rotation)
        {
            case 0:
                return shape;
            case 90:
                return RotateShape90(shape);
            case 180:
                return RotateShape180(shape);
            case 270:
                return RotateShape270(shape);
            default:
                return shape;
        }
    }

    Vector2Int[] RotateShape90(Vector2Int[] shape)
    {
        Vector2Int[] rotated = new Vector2Int[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            rotated[i] = new Vector2Int(shape[i].y, -shape[i].x);
        }
        return NormalizeShape(rotated);
    }

    Vector2Int[] RotateShape180(Vector2Int[] shape)
    {
        Vector2Int[] rotated = new Vector2Int[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            rotated[i] = new Vector2Int(-shape[i].x, -shape[i].y);
        }
        return NormalizeShape(rotated);
    }

    Vector2Int[] RotateShape270(Vector2Int[] shape)
    {
        Vector2Int[] rotated = new Vector2Int[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            rotated[i] = new Vector2Int(-shape[i].y, shape[i].x);
        }
        return NormalizeShape(rotated);
    }

    Vector2Int[] NormalizeShape(Vector2Int[] shape)
    {
        if (shape.Length == 0) return shape;

        int minX = int.MaxValue, minY = int.MaxValue;
        foreach (Vector2Int cell in shape)
        {
            minX = Mathf.Min(minX, cell.x);
            minY = Mathf.Min(minY, cell.y);
        }

        Vector2Int[] normalized = new Vector2Int[shape.Length];
        for (int i = 0; i < shape.Length; i++)
        {
            normalized[i] = new Vector2Int(shape[i].x - minX, shape[i].y - minY);
        }

        return normalized;
    }

    /// <summary>
    /// 마우스 위치가 블록의 셀 위에 있는지 검사
    /// </summary>
    public bool IsMouseOverBlock(Vector3 mouseWorldPos)
    {
        foreach (GameObject cell in cells)
        {
            if (cell == null) continue;

            Collider2D collider = cell.GetComponent<Collider2D>();
            if (collider != null && collider.OverlapPoint(mouseWorldPos))
            {
                return true;
            }
        }

        return false;
    }
}

/// <summary>
/// 복합 블록의 드래그 앤 드롭 처리
/// </summary>
public class DraggableCompositeBlock : MonoBehaviour
{
    public GridItem blockData;
    public SpriteInventoryGrid grid;
    public CompositeBlockController controller;

    private bool isDragging = false;
    private Vector3 offset;
    private Vector3 originalPosition;
    private Vector2Int originalGridPos;
    private Camera mainCamera;

    void Awake()
    {
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (blockData == null || grid == null || controller == null)
            return;

        Vector3 mousePos = GetMouseWorldPosition();

        // 드래그 시작 검사
        if (!isDragging && Input.GetMouseButtonDown(0))
        {
            if (controller.IsMouseOverBlock(mousePos))
            {
                StartDragging(mousePos);
            }
        }

        // 드래그 중
        if (isDragging)
        {
            transform.position = mousePos + offset;

            // 프리뷰 표시
            if (grid != null)
            {
                grid.ShowPlacementPreview(blockData, mousePos);
            }

            // 회전
            if (Input.GetKeyDown(KeyCode.R) || Input.mouseScrollDelta.y > 0)
            {
                controller.RotateBlock();
                grid.ShowPlacementPreview(blockData, mousePos);
            }
        }

        // 드래그 종료
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            EndDragging(mousePos);
        }

        // 우클릭 회전 (배치된 블록)
        if (!isDragging && Input.GetMouseButtonDown(1))
        {
            if (controller.IsMouseOverBlock(mousePos))
            {
                if (blockData.gridPosition.x >= 0) // 그리드에 배치된 상태
                {
                    grid.RotateItem(blockData);
                    controller.UpdateCellPositions();
                }
                else // 그리드 밖
                {
                    controller.RotateBlock();
                }
            }
        }
    }

    void StartDragging(Vector3 mousePos)
    {
        isDragging = true;
        originalPosition = transform.position;
        originalGridPos = blockData.gridPosition;
        offset = transform.position - mousePos;

        // 반투명하게
        foreach (GameObject cell in controller.cells)
        {
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 0.6f;
                sr.color = c;
                sr.sortingOrder = 10;
            }
        }

        // 그리드에서 임시 제거
        grid.RemoveItem(blockData, destroyVisual: false);

        Debug.Log($"Started dragging {blockData.itemName}");
    }

    void EndDragging(Vector3 mousePos)
    {
        isDragging = false;

        // 프리뷰 숨기기
        grid.HidePlacementPreview();

        // 불투명하게 복구
        foreach (GameObject cell in controller.cells)
        {
            SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color;
                c.a = 1f;
                sr.color = c;
                sr.sortingOrder = 1;
            }
        }

        // 그리드 좌표로 변환
        Vector2Int gridPos = grid.WorldToGridPosition(mousePos);

        // 배치 시도
        if (grid.CanPlaceItem(blockData, gridPos))
        {
            grid.PlaceItem(blockData, gridPos);
            Debug.Log($"Placed {blockData.itemName} at {gridPos}");
        }
        else
        {
            // 배치 실패 - 원래 위치로 또는 Eject
            Vector3 ejectPos = grid.GetEjectPosition();
            transform.position = ejectPos;
            blockData.gridPosition = new Vector2Int(-1, -1);
            Debug.Log($"Cannot place {blockData.itemName}! Ejected.");
        }
    }

    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        return worldPos;
    }
}
