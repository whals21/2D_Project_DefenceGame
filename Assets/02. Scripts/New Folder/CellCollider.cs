using UnityEngine;

/// <summary>
/// Cell GameObject에 붙어서 블록 배치를 물리적으로 차단하는 컴포넌트
/// Cell이 점유되면 Collider를 활성화하여 새로운 블록의 접근을 막음
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class CellCollider : MonoBehaviour
{
    private BoxCollider2D cellCollider;
    private Vector2Int gridPosition;
    private bool isOccupiedByBlock = false; // 블록에 의해 점유되었는지 여부

    // 이 Cell을 점유하고 있는 블록 참조 (선택사항)
    private Block occupyingBlock = null;

    void Awake()
    {
        // BoxCollider2D 가져오기 (없으면 자동 추가됨)
        cellCollider = GetComponent<BoxCollider2D>();

        // 초기 설정
        cellCollider.isTrigger = true; // Trigger로 설정
        cellCollider.size = Vector2.one; // 1x1 크기

        // 초기에는 비활성화 (블록이 배치되면 활성화)
        cellCollider.enabled = false;
    }

    /// <summary>
    /// Cell 초기화 (GridMapManager에서 호출)
    /// </summary>
    public void Initialize(Vector2Int pos)
    {
        gridPosition = pos;
        isOccupiedByBlock = false;
        occupyingBlock = null;
        cellCollider.enabled = false;
    }

    /// <summary>
    /// 블록이 이 Cell을 점유할 때 호출
    /// </summary>
    public void SetOccupied(bool occupied, Block block = null)
    {
        isOccupiedByBlock = occupied;
        occupyingBlock = block;

        // Collider 활성화/비활성화
        if (cellCollider != null)
        {
            cellCollider.enabled = occupied;
        }

        // 디버그 로그
        if (occupied)
        {
            Debug.Log($"Cell {gridPosition} is now occupied by {(block != null ? block.blockData.blockName : "Unknown")}");
        }
        else
        {
            Debug.Log($"Cell {gridPosition} is now free");
        }
    }

    /// <summary>
    /// 점유 상태 확인
    /// </summary>
    public bool IsOccupied()
    {
        return isOccupiedByBlock;
    }

    /// <summary>
    /// 점유하고 있는 블록 반환
    /// </summary>
    public Block GetOccupyingBlock()
    {
        return occupyingBlock;
    }

    /// <summary>
    /// 그리드 위치 반환
    /// </summary>
    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    /// <summary>
    /// Trigger 충돌 감지 (디버그용)
    /// </summary>
    void OnTriggerEnter2D(Collider2D other)
    {
        // 다른 블록이 접근하려고 할 때
        Block incomingBlock = other.GetComponent<Block>();
        if (incomingBlock != null && incomingBlock != occupyingBlock)
        {
            Debug.LogWarning($"Cell {gridPosition} collision detected! " +
                           $"Occupied by: {(occupyingBlock != null ? occupyingBlock.blockData.blockName : "None")}, " +
                           $"Incoming: {incomingBlock.blockData.blockName}");
        }
    }

    /// <summary>
    /// Gizmo로 점유 상태 시각화 (Scene 뷰에서만 보임)
    /// </summary>
    void OnDrawGizmos()
    {
        if (isOccupiedByBlock)
        {
            Gizmos.color = new Color(1, 0, 0, 0.3f); // 빨간색 반투명
            Gizmos.DrawCube(transform.position, Vector3.one * 0.9f);
        }
    }
}
