using UnityEngine;
using System.Collections.Generic;

public class InventoryGrid : MonoBehaviour
{
    [Header("Grid Settings")]
    public int gridWidth = 6;
    public int gridHeight = 6;
    public float cellSize = 80f;
    
    [Header("References")]
    public Transform gridParent;
    public GameObject cellPrefab;
    public GameObject itemPrefab;
    
    // 그리드 상태 (true = 차지됨, false = 비어있음)
    private bool[,] gridState;
    
    // 배치된 아이템들
    private List<InventoryItem> placedItems = new List<InventoryItem>();
    
    // 각 아이템의 GameObject
    private Dictionary<InventoryItem, GameObject> itemObjects = new Dictionary<InventoryItem, GameObject>();
    
    void Start()
    {
        InitializeGrid();
    }
    
    void InitializeGrid()
    {
        gridState = new bool[gridWidth, gridHeight];
        
        // 그리드 셀 생성 (시각화용)
        if (cellPrefab != null && gridParent != null)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GameObject cell = Instantiate(cellPrefab, gridParent);
                    RectTransform rt = cell.GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(x * cellSize, y * cellSize);
                    rt.sizeDelta = new Vector2(cellSize - 2, cellSize - 2);
                    cell.name = $"Cell_{x}_{y}";
                }
            }
        }
    }
    
    // 아이템을 배치할 수 있는지 확인
    public bool CanPlaceItem(InventoryItem item, Vector2Int position)
    {
        Vector2Int size = item.GetRotatedSize();
        
        // 그리드 범위 체크
        if (position.x < 0 || position.y < 0 ||
            position.x + size.x > gridWidth || 
            position.y + size.y > gridHeight)
        {
            return false;
        }
        
        // 해당 위치가 비어있는지 체크
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
    public bool PlaceItem(InventoryItem item, Vector2Int position)
    {
        if (!CanPlaceItem(item, position))
        {
            Debug.Log($"Cannot place {item.itemName} at {position}");
            return false;
        }
        
        // 그리드 상태 업데이트
        Vector2Int size = item.GetRotatedSize();
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                gridState[position.x + x, position.y + y] = true;
            }
        }
        
        // 아이템 위치 저장
        item.gridPosition = position;
        placedItems.Add(item);
        
        // 비주얼 생성
        CreateItemVisual(item);
        
        Debug.Log($"Placed {item.itemName} at {position}");
        return true;
    }
    
    // 아이템 제거
    public bool RemoveItem(InventoryItem item)
    {
        if (!placedItems.Contains(item))
        {
            Debug.Log($"{item.itemName} is not placed");
            return false;
        }
        
        // 그리드 상태 업데이트
        Vector2Int size = item.GetRotatedSize();
        Vector2Int pos = item.gridPosition;
        
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                gridState[pos.x + x, pos.y + y] = false;
            }
        }
        
        // 아이템 제거
        item.gridPosition = new Vector2Int(-1, -1);
        placedItems.Remove(item);
        
        // 비주얼 제거
        if (itemObjects.ContainsKey(item))
        {
            Destroy(itemObjects[item]);
            itemObjects.Remove(item);
        }
        
        Debug.Log($"Removed {item.itemName}");
        return true;
    }
    
    // 아이템 이동 (제거 후 재배치)
    public bool MoveItem(InventoryItem item, Vector2Int newPosition)
    {
        if (!placedItems.Contains(item))
            return false;
        
        // 임시로 제거
        RemoveItem(item);
        
        // 새 위치에 배치 시도
        if (PlaceItem(item, newPosition))
        {
            return true;
        }
        else
        {
            // 실패하면 원래 위치로 복구
            PlaceItem(item, item.gridPosition);
            return false;
        }
    }
    
    // 아이템 회전
    public bool RotateItem(InventoryItem item)
    {
        if (!placedItems.Contains(item))
        {
            item.Rotate();
            return true;
        }
        
        // 배치된 상태에서 회전 시도
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
            // 회전 불가능하면 원상복구
            item.Rotate();
            item.Rotate();
            item.Rotate(); // 3번 더 회전해서 원래대로
            PlaceItem(item, originalPos);
            Debug.Log($"Cannot rotate {item.itemName} at current position");
            return false;
        }
    }
    
    // 비주얼 생성
    void CreateItemVisual(InventoryItem item)
    {
        if (itemPrefab == null) return;
        
        GameObject itemObj = Instantiate(itemPrefab, gridParent);
        RectTransform rt = itemObj.GetComponent<RectTransform>();
        
        Vector2Int size = item.GetRotatedSize();
        rt.anchoredPosition = new Vector2(
            item.gridPosition.x * cellSize + (size.x * cellSize) / 2f,
            item.gridPosition.y * cellSize + (size.y * cellSize) / 2f
        );
        rt.sizeDelta = new Vector2(size.x * cellSize - 4, size.y * cellSize - 4);
        
        // 색상 및 스프라이트 설정
        UnityEngine.UI.Image img = itemObj.GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            img.color = item.color;
            if (item.sprite != null)
                img.sprite = item.sprite;
        }
        
        // 텍스트 표시
        var text = itemObj.GetComponentInChildren<UnityEngine.UI.Text>();
        if (text != null)
        {
            text.text = item.itemName;
        }
        
        // 회전 적용
        rt.rotation = Quaternion.Euler(0, 0, -item.rotation);
        
        itemObj.name = $"Item_{item.itemName}";
        itemObjects[item] = itemObj;
    }
    
    // 특정 위치의 아이템 가져오기
    public InventoryItem GetItemAtPosition(Vector2Int position)
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
    
    // 디버그: 그리드 상태 출력
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
    
    // Getter
    public List<InventoryItem> GetPlacedItems() => placedItems;
}
