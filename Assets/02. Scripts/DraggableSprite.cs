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
        
        // 그리드에서 임시 제거
        grid.RemoveItem(itemData);
        
        Debug.Log($"✅ Started dragging {itemData.itemName}");
    }
    
    void OnMouseDrag()
    {
        if (!isDragging || itemData == null) return;
        
        Vector3 mousePos = GetMouseWorldPosition();
        transform.position = mousePos + offset;
    }
    
    void OnMouseUp()
    {
        if (!isDragging || itemData == null || grid == null)
        {
            isDragging = false;
            return;
        }
        
        isDragging = false;
        
        // 원래 색상 복구
        spriteRenderer.color = originalColor;
        spriteRenderer.sortingOrder = 1;
        
        // 현재 위치를 그리드 좌표로 변환
        Vector2Int newGridPos = grid.WorldToGridPosition(transform.position);
        
        // 그리드에 배치 시도
        if (grid.CanPlaceItem(itemData, newGridPos))
        {
            grid.PlaceItem(itemData, newGridPos);
            Debug.Log($"Placed {itemData.itemName} at {newGridPos}");
        }
        else
        {
            // 배치 불가능하면 원래 위치로 복구
            Debug.Log($"Cannot place {itemData.itemName} at {newGridPos}, restoring");
            transform.position = originalPosition;
            
            if (originalGridPos.x >= 0 && originalGridPos.y >= 0)
            {
                grid.PlaceItem(itemData, originalGridPos);
            }
        }
    }
    
    // 우클릭으로 회전 (선택사항)
    void Update()
    {
        if (Input.GetMouseButtonDown(1)) // 우클릭
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
    
    Vector3 GetMouseWorldPosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = 0; // 2D이므로 Z는 0
        return worldPos;
    }
}