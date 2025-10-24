using UnityEngine;
using System.Collections.Generic;

public class SpriteInventoryGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 4;
    public int gridHeight = 4;
    public float cellSize = 1f;

    [Header("Grid Expansion Settings")]
    public int maxGridWidth = 18;
    public int maxGridHeight = 6;
    public int pathMaxWidth = 20;   // 경로 타일 최대 범위 (그리드보다 큼)
    public int pathMaxHeight = 8;   // 경로 타일 최대 범위 (그리드보다 큼)
    public int gridOffsetX = 3;     // 그리드 시작 오프셋 (왼쪽 경로 공간)
    public int gridOffsetY = 3;     // 그리드 시작 오프셋 (아래쪽 경로 공간)
    private bool isPurchaseMode = false; // 구매 모드 활성화 여부
    
    [Header("Visual Settings")]
    public GameObject cellPrefab;
    public GameObject itemPrefab;
    public Color cellColor = new Color(1f, 1f, 1f, 0.3f);
    public Color occupiedCellColor = new Color(1f, 0.5f, 0.5f, 0.3f);
    public Color validPlacementColor = new Color(0f, 1f, 0f, 0.5f);      // 배치 가능 (녹색)
    public Color invalidPlacementColor = new Color(1f, 0f, 0f, 0.5f);    // 배치 불가능 (빨강)
    public Color purchasableCellColor = new Color(1f, 1f, 0f, 0.2f);     // 구매 가능한 셀 (노란색 반투명)
    public Color pathTileColor = new Color(0.1f, 0.1f, 0.1f, 1f);        // 몬스터 경로 타일 (검은색)

    private bool[,] gridState;
    private List<GridItem> placedItems = new List<GridItem>();
    private Dictionary<GridItem, GameObject> itemObjects = new Dictionary<GridItem, GameObject>();
    private GameObject[,] cellObjects;
    private List<GameObject> purchasableCells = new List<GameObject>(); // 구매 가능한 셀들
    private List<GameObject> pathTiles = new List<GameObject>();        // 몬스터 경로 타일들

    // 프리뷰 시스템
    private GameObject previewObject;
    private List<GameObject> highlightedCells = new List<GameObject>();
    
    // 그리드 월드 좌표 시작점 (왼쪽 하단)
    private Vector3 gridOrigin;
    
    void Start()
    {
        Debug.Log("🎮 SpriteInventoryGrid.Start() called!");
        gridOrigin = transform.position;
        Debug.Log($"Grid Origin: {gridOrigin}");
        InitializeGrid();
    }
    
    void InitializeGrid()
    {
        Debug.Log($"🔧 Initializing grid: {gridWidth}x{gridHeight}, cell size: {cellSize}");

        // 경로 타일 최대 범위로 배열 초기화 (그리드보다 큼)
        int arrayWidth = Mathf.Max(maxGridWidth, pathMaxWidth);
        int arrayHeight = Mathf.Max(maxGridHeight, pathMaxHeight);
        gridState = new bool[arrayWidth, arrayHeight];
        cellObjects = new GameObject[arrayWidth, arrayHeight];

        // 현재 그리드 크기만큼만 셀 생성 (오프셋 적용)
        CreateCells(gridOffsetX, gridOffsetX + gridWidth, gridOffsetY, gridOffsetY + gridHeight);

        // 구매 모드가 활성화되면 구매 가능한 셀 표시 (초기에는 비활성화)
        // UpdatePurchasableCells();

        Debug.Log($"✅ Initialized grid with {gridWidth}x{gridHeight} cells (max: {maxGridWidth}x{maxGridHeight})");
    }

    void CreateCells(int startX, int endX, int startY, int endY)
    {
        if (cellPrefab == null)
        {
            Debug.LogError("❌ cellPrefab is NULL!");
            return;
        }

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                // 이미 셀이 있으면 건너뛰기
                if (cellObjects[x, y] != null) continue;

                Vector3 cellPos = GridToWorldPosition(x, y);
                GameObject cell = Instantiate(cellPrefab, cellPos, Quaternion.identity, transform);
                cell.name = $"Cell_{x}_{y}";

                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    sr.color = cellColor;
                    sr.sortingOrder = -1;
                }

                // Cell 크기를 cellSize에 맞게 (약간 작게)
                cell.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f);
                cellObjects[x, y] = cell;
            }
        }
    }
    
    // 그리드 좌표를 월드 좌표로 변환 (셀 중앙)
    public Vector3 GridToWorldPosition(int x, int y)
    {
        return gridOrigin + new Vector3(
            x * cellSize + cellSize / 2f,
            y * cellSize + cellSize / 2f,
            0
        );
    }
    
    public Vector3 GridToWorldPosition(Vector2Int gridPos)
    {
        return GridToWorldPosition(gridPos.x, gridPos.y);
    }
    
    // 월드 좌표를 그리드 좌표로 변환
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        Vector3 localPos = worldPos - gridOrigin;
        int x = Mathf.FloorToInt(localPos.x / cellSize);
        int y = Mathf.FloorToInt(localPos.y / cellSize);
        
        // 음수 방지
        x = Mathf.Max(0, x);
        y = Mathf.Max(0, y);
        
        return new Vector2Int(x, y);
    }
    
    // 아이템을 배치할 수 있는지 확인
    public bool CanPlaceItem(GridItem item, Vector2Int position, bool showWarning = false)
    {
        Vector2Int size = item.GetRotatedSize();

        int arrayWidth = gridState.GetLength(0);
        int arrayHeight = gridState.GetLength(1);

        // 배열 범위 체크
        if (position.x < 0 || position.y < 0 ||
            position.x + size.x > arrayWidth ||
            position.y + size.y > arrayHeight)
        {
            return false;
        }

        // 해당 위치에 실제 셀이 존재하는지 확인
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int checkX = position.x + x;
                int checkY = position.y + y;

                // 셀이 존재하지 않으면 배치 불가
                if (cellObjects[checkX, checkY] == null)
                {
                    if (showWarning)
                    {
                        Debug.LogWarning($"Cannot place item: cell at ({checkX}, {checkY}) does not exist");
                    }
                    return false;
                }

                // 이미 다른 아이템이 있으면 배치 불가
                if (gridState[checkX, checkY])
                {
                    return false;
                }
            }
        }

        return true;
    }
    
    // 아이템 배치
    public bool PlaceItem(GridItem item, Vector2Int position)
    {
        Debug.Log($"🔹 PlaceItem called: {item.itemName} at {position}");

        if (!CanPlaceItem(item, position, showWarning: true))
        {
            Debug.LogWarning($"❌ Cannot place {item.itemName} at {position}");
            return false;
        }
        
        Vector2Int size = item.GetRotatedSize();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                gridState[position.x + x, position.y + y] = true;
                UpdateCellVisual(position.x + x, position.y + y);
            }
        }
        
        item.gridPosition = position;
        placedItems.Add(item);
        
        // GameObject가 이미 존재하면 위치만 업데이트, 없으면 새로 생성
        if (itemObjects.ContainsKey(item))
        {
            UpdateItemVisual(item);
        }
        else
        {
            CreateItemVisual(item);
        }
        
        Debug.Log($"✅ Placed {item.itemName} at {position}");
        return true;
    }
    
    // 아이템 제거
    public bool RemoveItem(GridItem item, bool destroyVisual = true)
    {
        if (!placedItems.Contains(item))
        {
            return false;
        }
        
        Vector2Int size = item.GetRotatedSize();
        Vector2Int pos = item.gridPosition;
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                gridState[pos.x + x, pos.y + y] = false;
                UpdateCellVisual(pos.x + x, pos.y + y);
            }
        }
        
        item.gridPosition = new Vector2Int(-1, -1);
        placedItems.Remove(item);
        
        // 드래그 중에는 비주얼을 유지
        if (destroyVisual && itemObjects.ContainsKey(item))
        {
            Destroy(itemObjects[item]);
            itemObjects.Remove(item);
        }
        
        Debug.Log($"Removed {item.itemName} (destroyVisual: {destroyVisual})");
        return true;
    }
    
    // 아이템 이동
    public bool MoveItem(GridItem item, Vector2Int newPosition)
    {
        if (!placedItems.Contains(item))
            return false;
        
        Vector2Int oldPos = item.gridPosition;
        //RemoveItem(item);
        
        if (PlaceItem(item, newPosition))
        {
            return true;
        }
        else
        {
            PlaceItem(item, oldPos);
            return false;
        }
    }
    
    // 아이템 회전
    public bool RotateItem(GridItem item)
    {
        if (!placedItems.Contains(item))
        {
            item.Rotate();
            return true;
        }

        Vector2Int originalPos = item.gridPosition;
        Vector2Int originalSize = item.GetRotatedSize();

        // 원래 위치 해제
        for (int x = 0; x < originalSize.x; x++)
        {
            for (int y = 0; y < originalSize.y; y++)
            {
                gridState[originalPos.x + x, originalPos.y + y] = false;
                UpdateCellVisual(originalPos.x + x, originalPos.y + y);
            }
        }

        // 회전
        item.Rotate();
        Vector2Int newSize = item.GetRotatedSize();

        // 회전 후 배치 가능한지 확인
        if (CanPlaceItem(item, originalPos))
        {
            // 새 위치에 배치
            for (int x = 0; x < newSize.x; x++)
            {
                for (int y = 0; y < newSize.y; y++)
                {
                    gridState[originalPos.x + x, originalPos.y + y] = true;
                    UpdateCellVisual(originalPos.x + x, originalPos.y + y);
                }
            }

            // 비주얼 업데이트
            UpdateItemVisual(item);
            Debug.Log($"Rotated {item.itemName} to {item.rotation} degrees");
            return true;
        }
        else
        {
            // 회전 실패 - 원래대로 되돌림
            item.Rotate();
            item.Rotate();
            item.Rotate();

            // 원래 위치 복구
            for (int x = 0; x < originalSize.x; x++)
            {
                for (int y = 0; y < originalSize.y; y++)
                {
                    gridState[originalPos.x + x, originalPos.y + y] = true;
                    UpdateCellVisual(originalPos.x + x, originalPos.y + y);
                }
            }

            Debug.Log($"Cannot rotate {item.itemName} at current position");
            return false;
        }
    }
    
    // 아이템 비주얼 생성
    void CreateItemVisual(GridItem item)
    {
        if (itemPrefab == null)
        {
            Debug.LogError("❌ itemPrefab is null!");
            return;
        }
        
        Vector2Int size = item.GetRotatedSize();
        
        // 아이템 중앙 위치 계산
        Vector3 itemCenter = GridToWorldPosition(item.gridPosition) + 
                            new Vector3(
                                (size.x - 1) * cellSize / 2f,
                                (size.y - 1) * cellSize / 2f,
                                -0.1f
                            );
        
        GameObject itemObj = Instantiate(itemPrefab, itemCenter, Quaternion.identity, transform);
        itemObj.name = $"Item_{item.itemName}";
        
        SpriteRenderer sr = itemObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = item.color;
            if (item.sprite != null)
                sr.sprite = item.sprite;
            sr.sortingOrder = 1;
        }
        
        // 크기는 항상 원본 크기로 (width x height) - 회전은 rotation으로만 처리
        itemObj.transform.localScale = new Vector3(
            item.width * cellSize * 0.95f,
            item.height * cellSize * 0.95f,
            1f
        );

        // Z축 기준으로 회전 (90도씩)
        itemObj.transform.rotation = Quaternion.Euler(0, 0, item.rotation);

        // DraggableSprite 컴포넌트 초기화
        DraggableSprite draggable = itemObj.GetComponent<DraggableSprite>();
        if (draggable != null)
        {
            draggable.itemData = item;
            draggable.grid = this;
            Debug.Log($"✅ Initialized {item.itemName}: width={item.width}, height={item.height}");
        }
        else
        {
            Debug.LogError($"❌ DraggableSprite component not found on {itemObj.name}!");
        }
        
        itemObjects[item] = itemObj;
    }
    
    // 아이템 비주얼 업데이트 (드래그 후 재배치)
    void UpdateItemVisual(GridItem item)
    {
        if (!itemObjects.ContainsKey(item)) return;

        GameObject itemObj = itemObjects[item];
        Vector2Int size = item.GetRotatedSize();

        // 위치 업데이트
        Vector3 itemCenter = GridToWorldPosition(item.gridPosition) +
                            new Vector3(
                                (size.x - 1) * cellSize / 2f,
                                (size.y - 1) * cellSize / 2f,
                                -0.1f
                            );

        itemObj.transform.position = itemCenter;

        // 크기는 항상 원본 크기로 (width x height) - 회전은 rotation으로만 처리
        itemObj.transform.localScale = new Vector3(
            item.width * cellSize * 0.95f,
            item.height * cellSize * 0.95f,
            1f
        );

        // Z축 기준으로 회전 (90도씩)
        itemObj.transform.rotation = Quaternion.Euler(0, 0, item.rotation);

        // 색상과 정렬 순서 복구
        SpriteRenderer sr = itemObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = item.color;
            sr.sortingOrder = 1;
        }

        Debug.Log($"🔄 Updated visual for {item.itemName} at {item.gridPosition}, rotation: {item.rotation}");
    }
    
    // 셀 비주얼 업데이트
    void UpdateCellVisual(int x, int y)
    {
        if (cellObjects[x, y] != null)
        {
            SpriteRenderer sr = cellObjects[x, y].GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = gridState[x, y] ? occupiedCellColor : cellColor;
            }
        }
    }
    
    // 특정 위치의 아이템 가져오기
    public GridItem GetItemAtPosition(Vector2Int position)
    {
        foreach (var item in placedItems)
        {
            Vector2Int size = item.GetRotatedSize();
            Vector2Int pos = item.gridPosition;
            
            if (position.x >= pos.x && position.x < pos.x + size.x &&
                position.y >= pos.y && position.y < pos.y + size.y)
            {
                return item;
            }
        }
        return null;
    }
    
    public GridItem GetItemAtWorldPosition(Vector3 worldPos)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPos);
        return GetItemAtPosition(gridPos);
    }
    
    public List<GridItem> GetPlacedItems() => placedItems;
    
    public void PrintGridState()
    {
        string gridString = "Grid State:\n";
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                gridString += gridState[x, y] ? "[X]" : "[ ]";
            }
            gridString += "\n";
        }
        Debug.Log(gridString);
    }

    // ===== 배치 프리뷰 시스템 =====

    /// <summary>
    /// 드래그 중 배치 프리뷰 표시 (백팩 히어로 스타일)
    /// </summary>
    public void ShowPlacementPreview(GridItem item, Vector3 worldPosition)
    {
        Vector2Int gridPos = WorldToGridPosition(worldPosition);
        bool canPlace = CanPlaceItem(item, gridPos);

        // 프리뷰 오브젝트 생성 또는 업데이트
        if (previewObject == null && itemPrefab != null)
        {
            previewObject = Instantiate(itemPrefab, transform);
            previewObject.name = "PreviewObject";

            // 콜라이더 비활성화 (프리뷰는 클릭 불가)
            BoxCollider2D collider = previewObject.GetComponent<BoxCollider2D>();
            if (collider != null) collider.enabled = false;

            // DraggableSprite 비활성화
            DraggableSprite draggable = previewObject.GetComponent<DraggableSprite>();
            if (draggable != null) draggable.enabled = false;
        }

        if (previewObject != null)
        {
            Vector2Int size = item.GetRotatedSize();

            // 프리뷰 위치 설정 (그리드에 스냅)
            Vector3 previewPos = GridToWorldPosition(gridPos) +
                                new Vector3(
                                    (size.x - 1) * cellSize / 2f,
                                    (size.y - 1) * cellSize / 2f,
                                    -0.2f  // 아이템보다 약간 앞
                                );

            previewObject.transform.position = previewPos;
            // 크기는 항상 원본 크기로 (width x height) - 회전은 rotation으로만 처리
            previewObject.transform.localScale = new Vector3(
                item.width * cellSize * 0.95f,
                item.height * cellSize * 0.95f,
                1f
            );
            // Z축 기준으로 회전
            previewObject.transform.rotation = Quaternion.Euler(0, 0, item.rotation);

            // 프리뷰 색상 설정
            SpriteRenderer sr = previewObject.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (item.sprite != null) sr.sprite = item.sprite;

                Color previewColor = canPlace ? validPlacementColor : invalidPlacementColor;
                sr.color = previewColor;
                sr.sortingOrder = 5;  // 드래그 중인 아이템보다 낮게
            }
        }

        // 셀 하이라이트
        HighlightCells(item, gridPos, canPlace);
    }

    /// <summary>
    /// 배치될 셀들을 하이라이트 표시
    /// </summary>
    private void HighlightCells(GridItem item, Vector2Int position, bool canPlace)
    {
        // 기존 하이라이트 제거
        ClearCellHighlights();

        Vector2Int size = item.GetRotatedSize();
        Color highlightColor = canPlace ? validPlacementColor : invalidPlacementColor;

        // 각 셀 하이라이트
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                int cellX = position.x + x;
                int cellY = position.y + y;

                // 그리드 범위 체크
                if (cellX >= 0 && cellX < gridWidth && cellY >= 0 && cellY < gridHeight)
                {
                    if (cellObjects[cellX, cellY] != null)
                    {
                        SpriteRenderer sr = cellObjects[cellX, cellY].GetComponent<SpriteRenderer>();
                        if (sr != null)
                        {
                            sr.color = highlightColor;
                            highlightedCells.Add(cellObjects[cellX, cellY]);
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 셀 하이라이트 제거
    /// </summary>
    private void ClearCellHighlights()
    {
        foreach (GameObject cell in highlightedCells)
        {
            if (cell != null)
            {
                SpriteRenderer sr = cell.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    // 원래 색상으로 복구
                    Vector2Int pos = WorldToGridPosition(cell.transform.position);
                    sr.color = gridState[pos.x, pos.y] ? occupiedCellColor : cellColor;
                }
            }
        }
        highlightedCells.Clear();
    }

    /// <summary>
    /// 프리뷰 숨기기
    /// </summary>
    public void HidePlacementPreview()
    {
        if (previewObject != null)
        {
            Destroy(previewObject);
            previewObject = null;
        }

        ClearCellHighlights();
    }

    // ===== 그리드 확장 시스템 =====

    /// <summary>
    /// 구매 가능한 셀들을 생성/업데이트
    /// </summary>
    void UpdatePurchasableCells()
    {
        // 기존 구매 가능 셀 제거
        foreach (var cell in purchasableCells)
        {
            if (cell != null) Destroy(cell);
        }
        purchasableCells.Clear();

        if (cellPrefab == null) return;

        // 그리드 유효 범위 계산 (오프셋 고려)
        int minValidX = gridOffsetX;
        int maxValidX = gridOffsetX + maxGridWidth - 1;
        int minValidY = gridOffsetY;
        int maxValidY = gridOffsetY + maxGridHeight - 1;

        int arrayWidth = cellObjects.GetLength(0);
        int arrayHeight = cellObjects.GetLength(1);

        // 현재 그리드와 인접한 빈 셀 찾기
        for (int x = 0; x < arrayWidth; x++)
        {
            for (int y = 0; y < arrayHeight; y++)
            {
                // 그리드 유효 범위를 벗어나면 건너뛰기
                if (x < minValidX || x > maxValidX || y < minValidY || y > maxValidY)
                    continue;

                // 이미 실제 셀이 존재하는지 확인 (cellObjects로 체크)
                if (cellObjects[x, y] != null) continue;

                // 인접한 셀이 실제로 존재하는지 확인
                bool isAdjacent = false;

                // 왼쪽에 실제 셀이 있는지 확인
                if (x > 0 && cellObjects[x - 1, y] != null)
                    isAdjacent = true;

                // 아래쪽에 실제 셀이 있는지 확인
                if (y > 0 && cellObjects[x, y - 1] != null)
                    isAdjacent = true;

                // 오른쪽에 실제 셀이 있는지 확인
                if (x < arrayWidth - 1 && cellObjects[x + 1, y] != null)
                    isAdjacent = true;

                // 위쪽에 실제 셀이 있는지 확인
                if (y < arrayHeight - 1 && cellObjects[x, y + 1] != null)
                    isAdjacent = true;

                if (isAdjacent)
                {
                    // 구매 가능한 셀 생성
                    Vector3 cellPos = GridToWorldPosition(x, y);
                    GameObject purchasableCell = Instantiate(cellPrefab, cellPos, Quaternion.identity, transform);
                    purchasableCell.name = $"PurchasableCell_{x}_{y}";

                    SpriteRenderer sr = purchasableCell.GetComponent<SpriteRenderer>();
                    if (sr != null)
                    {
                        sr.color = purchasableCellColor;
                        sr.sortingOrder = -1;
                    }

                    purchasableCell.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f);

                    // 클릭 가능하도록 PurchasableCell 컴포넌트 추가
                    PurchasableCell cellComponent = purchasableCell.AddComponent<PurchasableCell>();
                    cellComponent.gridX = x;
                    cellComponent.gridY = y;
                    cellComponent.grid = this;

                    purchasableCells.Add(purchasableCell);
                }
            }
        }

        Debug.Log($"Created {purchasableCells.Count} purchasable cells");
    }

    /// <summary>
    /// 특정 셀을 그리드에 추가 (구매) - 한 칸씩
    /// </summary>
    public bool PurchaseCell(int x, int y)
    {
        // 그리드 유효 범위 체크 (오프셋 고려)
        int minValidX = gridOffsetX;
        int maxValidX = gridOffsetX + maxGridWidth - 1;
        int minValidY = gridOffsetY;
        int maxValidY = gridOffsetY + maxGridHeight - 1;

        if (x < minValidX || x > maxValidX || y < minValidY || y > maxValidY)
        {
            Debug.LogWarning($"❌ Cell ({x}, {y}) is outside the valid grid range! Valid range: ({minValidX}-{maxValidX}, {minValidY}-{maxValidY})");
            return false;
        }

        // 배열 범위 체크
        int arrayWidth = cellObjects.GetLength(0);
        int arrayHeight = cellObjects.GetLength(1);

        if (x < 0 || x >= arrayWidth || y < 0 || y >= arrayHeight)
        {
            Debug.LogWarning($"❌ Invalid cell position: ({x}, {y})");
            return false;
        }

        // 이미 실제 셀이 존재하는지 확인
        if (cellObjects[x, y] != null)
        {
            Debug.LogWarning($"❌ Cell ({x}, {y}) is already owned!");
            return false;
        }

        // 정확히 해당 셀만 생성 (한 칸씩)
        CreateCells(x, x + 1, y, y + 1);

        // 구매 가능한 셀 업데이트 (구매 모드일 때만)
        if (isPurchaseMode)
        {
            UpdatePurchasableCells();
        }

        Debug.Log($"✅ Purchased cell at ({x}, {y})!");
        return true;
    }

    /// <summary>
    /// 구매 모드 토글
    /// </summary>
    public void TogglePurchaseMode()
    {
        isPurchaseMode = !isPurchaseMode;

        if (isPurchaseMode)
        {
            // 구매 모드 활성화 - 구매 가능한 셀 표시, 경로 타일 숨김
            UpdatePurchasableCells();
            HidePathTiles();
            Debug.Log("🛒 Purchase mode ENABLED");
        }
        else
        {
            // 구매 모드 비활성화 - 구매 가능한 셀 숨김, 경로 타일 표시
            HidePurchasableCells();
            CreatePathTiles();
            Debug.Log("🛒 Purchase mode DISABLED - Monster path created");
        }
    }

    /// <summary>
    /// 구매 모드 상태 확인
    /// </summary>
    public bool IsPurchaseMode() => isPurchaseMode;

    /// <summary>
    /// 구매 가능한 셀 숨김
    /// </summary>
    void HidePurchasableCells()
    {
        foreach (var cell in purchasableCells)
        {
            if (cell != null) Destroy(cell);
        }
        purchasableCells.Clear();
    }

    /// <summary>
    /// 그리드 외곽 경로 타일 생성 (몬스터가 이동할 길)
    /// 구매한 셀들을 둘러싸는 연결된 경로 생성
    /// </summary>
    void CreatePathTiles()
    {
        if (cellPrefab == null) return;

        // 기존 경로 타일 제거
        foreach (var tile in pathTiles)
        {
            if (tile != null) Destroy(tile);
        }
        pathTiles.Clear();

        // 현재 그리드의 실제 범위 계산
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        int arrayWidth = cellObjects.GetLength(0);
        int arrayHeight = cellObjects.GetLength(1);

        for (int x = 0; x < arrayWidth; x++)
        {
            for (int y = 0; y < arrayHeight; y++)
            {
                if (cellObjects[x, y] != null)
                {
                    minX = Mathf.Min(minX, x);
                    maxX = Mathf.Max(maxX, x);
                    minY = Mathf.Min(minY, y);
                    maxY = Mathf.Max(maxY, y);
                }
            }
        }

        // 경로 타일 범위 설정 (그리드보다 넓게)
        // 그리드 중심을 기준으로 pathMaxWidth/Height 만큼 확장
        int centerX = (minX + maxX) / 2;
        int centerY = (minY + maxY) / 2;

        int pathMinX = Mathf.Max(0, centerX - pathMaxWidth / 2);
        int pathMaxX = Mathf.Min(arrayWidth - 1, centerX + pathMaxWidth / 2);
        int pathMinY = Mathf.Max(0, centerY - pathMaxHeight / 2);
        int pathMaxY = Mathf.Min(arrayHeight - 1, centerY + pathMaxHeight / 2);

        // 경로 타일 생성 범위 내에서 셀이 없는 곳에 경로 생성
        for (int x = pathMinX; x <= pathMaxX; x++)
        {
            for (int y = pathMinY; y <= pathMaxY; y++)
            {
                // 셀이 없는 곳에만 경로 타일 생성
                if (cellObjects[x, y] == null)
                {
                    // 상하좌우 중 하나라도 셀이 있으면 경로 생성
                    bool hasAdjacentCell = false;

                    // 위쪽
                    if (y + 1 < arrayHeight && cellObjects[x, y + 1] != null)
                        hasAdjacentCell = true;
                    // 아래쪽
                    if (y > 0 && cellObjects[x, y - 1] != null)
                        hasAdjacentCell = true;
                    // 오른쪽
                    if (x + 1 < arrayWidth && cellObjects[x + 1, y] != null)
                        hasAdjacentCell = true;
                    // 왼쪽
                    if (x > 0 && cellObjects[x - 1, y] != null)
                        hasAdjacentCell = true;

                    // 대각선도 체크 (모서리 연결용)
                    if (!hasAdjacentCell)
                    {
                        // 오른쪽 위
                        if (x + 1 < arrayWidth && y + 1 < arrayHeight && cellObjects[x + 1, y + 1] != null)
                            hasAdjacentCell = true;
                        // 왼쪽 위
                        if (x > 0 && y + 1 < arrayHeight && cellObjects[x - 1, y + 1] != null)
                            hasAdjacentCell = true;
                        // 오른쪽 아래
                        if (x + 1 < arrayWidth && y > 0 && cellObjects[x + 1, y - 1] != null)
                            hasAdjacentCell = true;
                        // 왼쪽 아래
                        if (x > 0 && y > 0 && cellObjects[x - 1, y - 1] != null)
                            hasAdjacentCell = true;
                    }

                    if (hasAdjacentCell)
                    {
                        CreatePathTile(x, y);
                    }
                }
            }
        }

        Debug.Log($"Created {pathTiles.Count} path tiles around grid (range: {pathMinX}-{pathMaxX}, {pathMinY}-{pathMaxY})");
    }

    /// <summary>
    /// 특정 위치에 경로 타일 생성
    /// </summary>
    void CreatePathTile(int x, int y)
    {
        Vector3 tilePos = GridToWorldPosition(x, y);
        GameObject pathTile = Instantiate(cellPrefab, tilePos, Quaternion.identity, transform);
        pathTile.name = $"PathTile_{x}_{y}";

        SpriteRenderer sr = pathTile.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = pathTileColor;
            sr.sortingOrder = -2; // 그리드 셀보다 뒤에
        }

        pathTile.transform.localScale = new Vector3(cellSize * 0.95f, cellSize * 0.95f, 1f);

        // 콜라이더 제거 (클릭 불가)
        BoxCollider2D collider = pathTile.GetComponent<BoxCollider2D>();
        if (collider != null) Destroy(collider);

        pathTiles.Add(pathTile);
    }

    /// <summary>
    /// 경로 타일 제거
    /// </summary>
    void HidePathTiles()
    {
        foreach (var tile in pathTiles)
        {
            if (tile != null) Destroy(tile);
        }
        pathTiles.Clear();
    }

    /// <summary>
    /// 현재 그리드 정보 반환
    /// </summary>
    public string GetGridInfo()
    {
        return $"Current: {gridWidth}x{gridHeight} / Max: {maxGridWidth}x{maxGridHeight}";
    }
}