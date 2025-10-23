using UnityEngine;
using System.Collections.Generic;

public class InventoryTest : MonoBehaviour
{
    public InventoryGrid inventoryGrid;
    
    private List<InventoryItem> testItems = new List<InventoryItem>();
    
    void Start()
    {
        // 테스트 아이템 생성
        CreateTestItems();
        
        // 2초 후 자동 배치 테스트
        Invoke("RunAutoPlacementTest", 2f);
    }
    
    void CreateTestItems()
    {
        // 다양한 크기의 블록 생성
        testItems.Add(new InventoryItem("Long Sword", 1, 4, null) { color = new Color(1f, 0.3f, 0.3f) });
        testItems.Add(new InventoryItem("Shield", 2, 3, null) { color = new Color(0.3f, 0.3f, 1f) });
        testItems.Add(new InventoryItem("Potion", 1, 2, null) { color = new Color(0.3f, 1f, 0.3f) });
        testItems.Add(new InventoryItem("Armor", 3, 2, null) { color = new Color(0.8f, 0.8f, 0.3f) });
        testItems.Add(new InventoryItem("Ring", 1, 1, null) { color = new Color(1f, 0.8f, 0.3f) });
        testItems.Add(new InventoryItem("Bow", 2, 4, null) { color = new Color(0.6f, 0.3f, 0.6f) });
        
        Debug.Log($"Created {testItems.Count} test items");
    }
    
    void RunAutoPlacementTest()
    {
        Debug.Log("=== Starting Auto Placement Test ===");
        
        // 1. 아이템 배치 테스트
        inventoryGrid.PlaceItem(testItems[0], new Vector2Int(0, 0)); // Long Sword
        inventoryGrid.PlaceItem(testItems[1], new Vector2Int(1, 0)); // Shield
        inventoryGrid.PlaceItem(testItems[2], new Vector2Int(3, 0)); // Potion
        inventoryGrid.PlaceItem(testItems[3], new Vector2Int(0, 4)); // Armor
        inventoryGrid.PlaceItem(testItems[4], new Vector2Int(4, 0)); // Ring
        
        inventoryGrid.PrintGridState();
        
        // 3초 후 회전 테스트
        Invoke("TestRotation", 3f);
    }
    
    void TestRotation()
    {
        Debug.Log("=== Testing Rotation ===");
        
        // Long Sword 회전
        if (inventoryGrid.RotateItem(testItems[0]))
        {
            Debug.Log("Successfully rotated Long Sword");
        }
        
        inventoryGrid.PrintGridState();
        
        // 3초 후 이동 테스트
        Invoke("TestMovement", 3f);
    }
    
    void TestMovement()
    {
        Debug.Log("=== Testing Movement ===");
        
        // Shield 이동
        if (inventoryGrid.MoveItem(testItems[1], new Vector2Int(4, 3)))
        {
            Debug.Log("Successfully moved Shield");
        }
        
        inventoryGrid.PrintGridState();
        
        // 3초 후 제거 테스트
        Invoke("TestRemoval", 3f);
    }
    
    void TestRemoval()
    {
        Debug.Log("=== Testing Removal ===");
        
        // Potion 제거
        if (inventoryGrid.RemoveItem(testItems[2]))
        {
            Debug.Log("Successfully removed Potion");
        }
        
        // 빈 공간에 Bow 배치
        if (inventoryGrid.PlaceItem(testItems[5], new Vector2Int(3, 0)))
        {
            Debug.Log("Successfully placed Bow in freed space");
        }
        
        inventoryGrid.PrintGridState();
    }
    
    void Update()
    {
        // 키보드 단축키
        if (Input.GetKeyDown(KeyCode.R))
        {
            // 첫 번째 배치된 아이템 회전
            var items = inventoryGrid.GetPlacedItems();
            if (items.Count > 0)
            {
                inventoryGrid.RotateItem(items[0]);
                Debug.Log("Rotated first item");
            }
        }
        
        if (Input.GetKeyDown(KeyCode.P))
        {
            // 그리드 상태 출력
            inventoryGrid.PrintGridState();
        }
        
        if (Input.GetKeyDown(KeyCode.C))
        {
            // 모든 아이템 제거
            var items = new List<InventoryItem>(inventoryGrid.GetPlacedItems());
            foreach (var item in items)
            {
                inventoryGrid.RemoveItem(item);
            }
            Debug.Log("Cleared all items");
        }
        
        if (Input.GetKeyDown(KeyCode.T))
        {
            // 배치 가능 여부 테스트
            Vector2Int testPos = new Vector2Int(2, 2);
            bool canPlace = inventoryGrid.CanPlaceItem(testItems[0], testPos);
            Debug.Log($"Can place {testItems[0].itemName} at {testPos}: {canPlace}");
        }
    }
}
