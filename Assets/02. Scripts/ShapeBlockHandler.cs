using System.Linq;
using UnityEngine;

/// <summary>
/// 좌표 기반 블록 충돌 검사 및 회전 시스템
/// Collider 대신 셀 좌표 리스트로 블록을 정의하고 처리
/// </summary>
public class ShapeBlockHandler : MonoBehaviour
{
    public GridItem blockData;
    public SpriteInventoryGrid grid;

    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
    }

    /// <summary>
    /// 마우스 위치가 블록의 활성 셀 내부에 있는지 검사
    /// </summary>
    public bool IsMouseOverBlock(Vector3 mouseWorldPos)
    {
        if (blockData == null || !blockData.HasShape() || grid == null)
            return false;

        // 블록의 현재 월드 위치
        Vector3 blockWorldPos = transform.position;

        // 회전된 shape 좌표 가져오기
        Vector2Int[] rotatedShape = GetRotatedShapeCells();

        foreach (Vector2Int cell in rotatedShape)
        {
            // 각 셀의 월드 위치 계산
            Vector3 cellWorldPos = blockWorldPos + new Vector3(
                cell.x * grid.cellSize,
                cell.y * grid.cellSize,
                0f
            );

            // 마우스가 이 셀 내부에 있는지 체크
            float halfCell = grid.cellSize * 0.5f;
            if (Mathf.Abs(mouseWorldPos.x - cellWorldPos.x) <= halfCell &&
                Mathf.Abs(mouseWorldPos.y - cellWorldPos.y) <= halfCell)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 블록의 현재 회전 상태에 맞는 셀 좌표 반환
    /// </summary>
    public Vector2Int[] GetRotatedShapeCells()
    {
        if (blockData == null || !blockData.HasShape())
            return new Vector2Int[0];

        Vector2Int[] shape = blockData.shape;
        int rotation = blockData.rotation;

        // 회전 각도에 따라 좌표 변환
        switch (rotation)
        {
            case 0:
                return shape;
            case 90:
                return Rotate90(shape);
            case 180:
                return Rotate180(shape);
            case 270:
                return Rotate270(shape);
            default:
                return shape;
        }
    }

    /// <summary>
    /// 90도 회전 (시계 방향)
    /// </summary>
    Vector2Int[] Rotate90(Vector2Int[] shape)
    {
        // (x, y) -> (y, -x) 변환 후 위치 보정
        Vector2Int[] rotated = shape.Select(cell => new Vector2Int(cell.y, -cell.x)).ToArray();
        return NormalizeShape(rotated);
    }

    /// <summary>
    /// 180도 회전
    /// </summary>
    Vector2Int[] Rotate180(Vector2Int[] shape)
    {
        // (x, y) -> (-x, -y) 변환 후 위치 보정
        Vector2Int[] rotated = shape.Select(cell => new Vector2Int(-cell.x, -cell.y)).ToArray();
        return NormalizeShape(rotated);
    }

    /// <summary>
    /// 270도 회전 (시계 방향)
    /// </summary>
    Vector2Int[] Rotate270(Vector2Int[] shape)
    {
        // (x, y) -> (-y, x) 변환 후 위치 보정
        Vector2Int[] rotated = shape.Select(cell => new Vector2Int(-cell.y, cell.x)).ToArray();
        return NormalizeShape(rotated);
    }

    /// <summary>
    /// Shape 좌표를 (0, 0) 기준으로 정규화 (왼쪽 하단이 (0, 0)이 되도록)
    /// </summary>
    Vector2Int[] NormalizeShape(Vector2Int[] shape)
    {
        if (shape.Length == 0) return shape;

        // 최소 좌표 찾기
        int minX = shape.Min(cell => cell.x);
        int minY = shape.Min(cell => cell.y);

        // 모든 셀을 이동시켜 (0, 0) 기준으로 맞춤
        return shape.Select(cell => new Vector2Int(cell.x - minX, cell.y - minY)).ToArray();
    }

    /// <summary>
    /// 특정 그리드 위치에 블록을 배치할 수 있는지 검사
    /// </summary>
    public bool CanPlaceAtGridPosition(Vector2Int gridPosition)
    {
        if (blockData == null || grid == null)
            return false;

        return grid.CanPlaceItem(blockData, gridPosition);
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환 (블록의 기준점)
    /// </summary>
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        if (grid == null)
            return Vector2Int.zero;

        return grid.WorldToGridPosition(worldPos);
    }

    /// <summary>
    /// 블록의 바운딩 박스 크기 반환 (회전 고려)
    /// </summary>
    public Vector2Int GetBoundingSize()
    {
        Vector2Int[] rotatedShape = GetRotatedShapeCells();

        if (rotatedShape.Length == 0)
            return Vector2Int.one;

        int maxX = rotatedShape.Max(cell => cell.x);
        int maxY = rotatedShape.Max(cell => cell.y);

        return new Vector2Int(maxX + 1, maxY + 1);
    }

    /// <summary>
    /// 마우스 월드 좌표 가져오기
    /// </summary>
    public Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0;
        return worldPos;
    }

    void OnDrawGizmosSelected()
    {
        // 에디터에서 블록의 활성 셀 시각화
        if (blockData == null || !blockData.HasShape() || grid == null)
            return;

        Gizmos.color = Color.yellow;
        Vector2Int[] rotatedShape = GetRotatedShapeCells();
        Vector3 blockWorldPos = transform.position;

        foreach (Vector2Int cell in rotatedShape)
        {
            Vector3 cellWorldPos = blockWorldPos + new Vector3(
                cell.x * grid.cellSize,
                cell.y * grid.cellSize,
                0f
            );

            Gizmos.DrawWireCube(cellWorldPos, Vector3.one * grid.cellSize * 0.95f);
        }
    }
}
