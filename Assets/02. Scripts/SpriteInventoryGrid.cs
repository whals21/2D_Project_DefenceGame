using UnityEngine;
using System.Collections.Generic;

public class SpriteInventoryGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 6;
    public int gridHeight = 6;
    public float cellSize = 1f;
    
    [Header("Visual Settings")]
    public GameObject cellPrefab;
    public GameObject itemPrefab;
    public Color cellColor = new Color(1f, 1f, 1f, 0.3f);
    public Color occupiedCellColor = new Color(1f, 0.5f, 0.5f, 0.3f);
    
    private bool[,] gridState;
    private List<GridItem> placedItems = new List<GridItem>();
    private Dictionary<GridItem, GameObject> itemObjects = new Dictionary<GridItem, GameObject>();
    private GameObject[,] cellObjects;
    
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
        
        gridState = new bool[gridWidth, gridHeight];
        cellObjects = new GameObject[gridWidth, gridHeight];
        
        // 그리드 셀 생성
        if (cellPrefab != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
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
            Debug.Log($"✅ Created {gridWidth * gridHeight} cells");
        }
        else
        {
            Debug.LogError("❌ cellPrefab is NULL!");
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
    public bool CanPlaceItem(GridItem item, Vector2Int position)
    {
        Vector2Int size = item.GetRotatedSize();
        
        if (position.x < 0 || position.y < 0 ||
            position.x + size.x > gridWidth || 
            position.y + size.y > gridHeight)
        {
            return false;
        }
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                if (gridState[position.x + x, position.y + y])
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
        
        if (!CanPlaceItem(item, position))
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
        RemoveItem(item);
        
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
        RemoveItem(item);
        item.Rotate();
        
        if (CanPlaceItem(item, originalPos))
        {
            PlaceItem(item, originalPos);
            return true;
        }
        else
        {
            item.Rotate();
            item.Rotate();
            item.Rotate();
            PlaceItem(item, originalPos);
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
        
        // 크기 설정 (셀에 맞게)
        itemObj.transform.localScale = new Vector3(
            size.x * cellSize * 0.95f,
            size.y * cellSize * 0.95f,
            1f
        );
        
        itemObj.transform.rotation = Quaternion.Euler(0, 0, -item.rotation);
        
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
        
        // 크기 업데이트 (회전 시 크기가 바뀔 수 있음)
        itemObj.transform.localScale = new Vector3(
            size.x * cellSize * 0.95f,
            size.y * cellSize * 0.95f,
            1f
        );
        
        itemObj.transform.rotation = Quaternion.Euler(0, 0, -item.rotation);
        
        // 색상과 정렬 순서 복구
        SpriteRenderer sr = itemObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.color = item.color;
            sr.sortingOrder = 1;
        }
        
        Debug.Log($"🔄 Updated visual for {item.itemName} at {item.gridPosition}");
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
}