using UnityEngine;
using System.Collections.Generic;

public class SpriteInventoryTest : MonoBehaviour
{
    public SpriteInventoryGrid inventoryGrid;
    
    private List<GridItem> testItems = new List<GridItem>();
    
    void Start()
    {
        Debug.Log("🎮 SpriteInventoryTest.Start() called!");
        
        if (inventoryGrid == null)
        {
            Debug.LogError("❌ inventoryGrid is NULL! Please assign it in Inspector!");
            return;
        }
        
        Debug.Log($"✅ inventoryGrid found: {inventoryGrid.name}");
        
        CreateTestItems();
        Invoke("RunAutoPlacementTest", 1f);
    }
    
    void CreateTestItems()
    {
        testItems.Add(new GridItem("Long Sword", 1, 4) { color = new Color(1f, 0.3f, 0.3f) });
        testItems.Add(new GridItem("Shield", 2, 3) { color = new Color(0.3f, 0.3f, 1f) });
        testItems.Add(new GridItem("Potion", 1, 2) { color = new Color(0.3f, 1f, 0.3f) });
        testItems.Add(new GridItem("Armor", 3, 2) { color = new Color(0.8f, 0.8f, 0.3f) });
        testItems.Add(new GridItem("Ring", 1, 1) { color = new Color(1f, 0.8f, 0.3f) });
        
        Debug.Log($"Created {testItems.Count} test items");
    }
    
    void RunAutoPlacementTest()
    {
        Debug.Log("=== Starting Auto Placement Test ===");
        
        inventoryGrid.PlaceItem(testItems[0], new Vector2Int(0, 0));
        inventoryGrid.PlaceItem(testItems[1], new Vector2Int(1, 0));
        inventoryGrid.PlaceItem(testItems[2], new Vector2Int(3, 0));
        inventoryGrid.PlaceItem(testItems[3], new Vector2Int(0, 4));
        inventoryGrid.PlaceItem(testItems[4], new Vector2Int(4, 0));
        
        inventoryGrid.PrintGridState();
    }
    
    void Update()
    {
        // R키: 첫 번째 아이템 회전
        if (Input.GetKeyDown(KeyCode.R))
        {
            var items = inventoryGrid.GetPlacedItems();
            if (items.Count > 0)
            {
                inventoryGrid.RotateItem(items[0]);
            }
        }
        
        // P키: 그리드 상태 출력
        if (Input.GetKeyDown(KeyCode.P))
        {
            inventoryGrid.PrintGridState();
        }
        
        // C키: 모든 아이템 제거
        if (Input.GetKeyDown(KeyCode.C))
        {
            var items = new List<GridItem>(inventoryGrid.GetPlacedItems());
            foreach (var item in items)
            {
                inventoryGrid.RemoveItem(item);
            }
            Debug.Log("Cleared all items");
        }
    }
}