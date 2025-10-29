using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class DraggableSprite : MonoBehaviour
{
    [SerializeField] public GridItem itemData;  // Inspector에 표시됨
    [SerializeField] public SpriteInventoryGrid grid;  // Inspector에 표시됨

    private SpriteRenderer spriteRenderer;
    private ShapeBlockHandler shapeHandler;
    private Vector3 offset;
    private Vector3 originalPosition;
    private Vector2Int originalGridPos;
    private bool isDragging = false;
    private Color originalColor;
    private Camera mainCamera;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;

        // ShapeBlockHandler 추가 (shape 블록용)
        shapeHandler = GetComponent<ShapeBlockHandler>();
        if (shapeHandler == null && itemData != null && itemData.HasShape())
        {
            shapeHandler = gameObject.AddComponent<ShapeBlockHandler>();
            shapeHandler.blockData = itemData;
            shapeHandler.grid = grid;
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

        // 드래그 시작 시 현재 회전 상태 유지
        transform.rotation = Quaternion.Euler(0, 0, itemData.rotation);

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
        // Shape 블록: 좌표 기반 클릭 검사
        if (itemData != null && itemData.HasShape() && shapeHandler != null)
        {
            HandleShapeBlockInput();
        }

        // 드래그 중 처리
        if (isDragging)
        {
            HandleDragging();
        }

        // 드래그 종료 처리
        if (isDragging && Input.GetMouseButtonUp(0))
        {
            HandleDragEnd();
        }

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

            // Shape 블록은 좌표 기반 검사
            if (itemData.HasShape() && shapeHandler != null)
            {
                if (shapeHandler.IsMouseOverBlock(mousePos))
                {
                    if (grid != null && itemData != null)
                    {
                        grid.RotateItem(itemData);
                        Debug.Log($"Rotated {itemData.itemName}");
                    }
                }
            }
            else
            {
                // 일반 블록은 Collider 기반
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
    }

    /// <summary>
    /// Shape 블록의 좌표 기반 입력 처리
    /// </summary>
    void HandleShapeBlockInput()
    {
        if (isDragging) return;

        Vector3 mousePos = GetMouseWorldPosition();

        // 마우스 클릭 시작
        if (Input.GetMouseButtonDown(0))
        {
            if (shapeHandler.IsMouseOverBlock(mousePos))
            {
                StartDragging();
            }
        }
    }

    /// <summary>
    /// 드래그 시작 (OnMouseDown 대체)
    /// </summary>
    void StartDragging()
    {
        if (itemData == null || grid == null) return;

        Debug.Log($"Started dragging {itemData.itemName} (coordinate-based)");

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

        // 드래그 시작 시 현재 회전 상태 유지
        transform.rotation = Quaternion.Euler(0, 0, itemData.rotation);

        // 그리드에서 임시 제거 (비주얼은 유지)
        grid.RemoveItem(itemData, destroyVisual: false);
    }

    /// <summary>
    /// 드래그 중 처리 (OnMouseDrag 대체)
    /// </summary>
    void HandleDragging()
    {
        if (itemData == null) return;

        Vector3 mousePos = GetMouseWorldPosition();
        transform.position = mousePos + offset;

        // 배치 프리뷰 표시
        if (grid != null)
        {
            grid.ShowPlacementPreview(itemData, mousePos);
        }
    }

    /// <summary>
    /// 드래그 종료 처리 (OnMouseUp 대체)
    /// </summary>
    void HandleDragEnd()
    {
        if (itemData == null || grid == null)
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

        // 해당 위치에 배치 가능한지 확인
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
            itemData.gridPosition = new Vector2Int(-1, -1);

            Debug.Log($"❌ Cannot place {itemData.itemName}! Ejected to {ejectPosition}");
        }
    }

    void RotateDuringDrag()
    {
        if (itemData == null || grid == null) return;

        itemData.Rotate();

        // 회전 시각적 업데이트 - shape가 있으면 0.95 고정, 없으면 기존 방식
        if (itemData.HasShape())
        {
            // 테트리스/펜토미노 블록 (통짜 스프라이트) - 크기 고정
            transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        }
        else
        {
            // 기존 아이템 (셀 조합형) - 크기는 원본 크기로
            transform.localScale = new Vector3(
                itemData.width * grid.cellSize * 0.95f,
                itemData.height * grid.cellSize * 0.95f,
                1f
            );
        }

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