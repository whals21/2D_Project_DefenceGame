using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class DraggableSprite : MonoBehaviour
{
    [SerializeField] public GridItem itemData;  // Inspector에 표시됨
    [SerializeField] public SpriteInventoryGrid grid;  // Inspector에 표시됨
    
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D boxCollider;
    private Vector3 offset;
    private Vector3 originalPosition;
    private Vector2Int originalGridPos;
    private bool isDragging = false;
    private Color originalColor;
    private Camera mainCamera;
    
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        boxCollider = GetComponent<BoxCollider2D>();
        mainCamera = Camera.main;
        
        if (boxCollider != null)
        {
            boxCollider.size = Vector2.one;
        }
    }
    
    void OnMouseDown()
    {
        Debug.Log($"OnMouseDown called on {gameObject.name}");
        Debug.Log($"itemData: {(itemData != null ? itemData.itemName : "NULL")}");
        Debug.Log($"grid: {(grid != null ? "EXISTS" : "NULL")}");
        
        if (itemData == null || grid == null)
        {
            Debug.LogError($"❌ Cannot drag! itemData={itemData}, grid={grid}");
            return;
        }
        
        isDragging = true;
        originalPosition = transform.position;
        originalGridPos = itemData.gridPosition;
        originalColor = spriteRenderer.color;
        
        // 마우스 위치와 오브젝트 위치의 오프셋 계산
        Vector3 mousePos = GetMouseWorldPosition();
        offset = transform.position - mousePos;
        
        // 반투명하게
        Color c = spriteRenderer.color;
        c.a = 0.6f;
        spriteRenderer.color = c;
        
        // 정렬 순서 변경 (맨 위로)
        spriteRenderer.sortingOrder = 10;

        // 그리드에서 임시 제거 (비주얼은 유지)
        grid.RemoveItem(itemData, destroyVisual: false);

        Debug.Log($"✅ Started dragging {itemData.itemName}");
    }
    
    void OnMouseDrag()
    {
        if (!isDragging || itemData == null) return;

        Vector3 mousePos = GetMouseWorldPosition();
        transform.position = mousePos + offset;

        // 배치 프리뷰 표시 (백팩 히어로 스타일)
        if (grid != null)
        {
            grid.ShowPlacementPreview(itemData, mousePos);
        }
    }
    
    void OnMouseUp()
    {
        if (!isDragging || itemData == null || grid == null)
        {
            isDragging = false;
            if (grid != null) grid.HidePlacementPreview();
            return;
        }

        isDragging = false;

        // 프리뷰 숨기기
        grid.HidePlacementPreview();

        // 원래 색상 복구
        spriteRenderer.color = originalColor;
        spriteRenderer.sortingOrder = 1;

        // 현재 위치를 그리드 좌표로 변환
        Vector3 mousePos = GetMouseWorldPosition();
        Vector2Int newGridPos = grid.WorldToGridPosition(mousePos);

        // 해당 위치에 배치 가능한지 확인 (셀 존재 여부 포함)
        if (grid.CanPlaceItem(itemData, newGridPos))
        {
            // 그리드에 배치
            grid.PlaceItem(itemData, newGridPos);
            Debug.Log($"Placed {itemData.itemName} at {newGridPos}");
        }
        else
        {
            // 배치 불가능한 경우 - 그리드 밖으로 튕김
            Vector3 ejectPosition = grid.GetEjectPosition();
            transform.position = ejectPosition;
            itemData.gridPosition = new Vector2Int(-1, -1); // 그리드에 없음을 표시

            Debug.Log($"❌ Cannot place {itemData.itemName}! Ejected to {ejectPosition}");
        }
    }
    
    void Update()
    {
        // 드래그 중 회전 (R키 또는 마우스 휠)
        if (isDragging)
        {
            // R키로 회전
            if (Input.GetKeyDown(KeyCode.R))
            {
                RotateDuringDrag();
            }

            // 마우스 휠로 회전
            float scrollDelta = Input.mouseScrollDelta.y;
            if (scrollDelta > 0)
            {
                RotateDuringDrag();
            }
        }
        // 우클릭으로 회전 (배치된 아이템)
        else if (Input.GetMouseButtonDown(1))
        {
            Vector3 mousePos = GetMouseWorldPosition();
            Collider2D hit = Physics2D.OverlapPoint(mousePos);

            if (hit != null && hit.gameObject == gameObject)
            {
                if (grid != null && itemData != null)
                {
                    grid.RotateItem(itemData);
                    Debug.Log($"Rotated {itemData.itemName}");
                }
            }
        }
    }

    void RotateDuringDrag()
    {
        if (itemData == null || grid == null) return;

        itemData.Rotate();

        // 회전 시각적 업데이트 - 크기는 원본 크기로, 회전만 변경
        transform.localScale = new Vector3(
            itemData.width * grid.cellSize * 0.95f,
            itemData.height * grid.cellSize * 0.95f,
            1f
        );
        // Z축 기준으로 90도씩 회전
        transform.rotation = Quaternion.Euler(0, 0, itemData.rotation);

        // 프리뷰 업데이트
        Vector3 mousePos = GetMouseWorldPosition();
        grid.ShowPlacementPreview(itemData, mousePos);

        Debug.Log($"Rotated {itemData.itemName} during drag (rotation: {itemData.rotation})");
    }
    
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0; // 2D이므로 Z는 0
        return worldPos;
    }
}