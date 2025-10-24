using UnityEngine;

/// <summary>
/// 백팩 시스템 테스트 - 다양한 크기의 아이템 생성
/// </summary>
public class BackpackInventoryTest : MonoBehaviour
{
    [Header("References")]
    public SpriteInventoryGrid grid;

    [Header("Test Items")]
    public Sprite potionSprite;
    public Sprite swordSprite;
    public Sprite shieldSprite;
    public Sprite bowSprite;

    void Start()
    {
        if (grid == null)
        {
            Debug.LogError("❌ Grid reference is null! Please assign SpriteInventoryGrid.");
            return;
        }

        // 백팩 히어로 스타일 아이템들 생성
        Invoke("CreateBackpackItems", 1f);
        //CreateBackpackItems();
    }

    void CreateBackpackItems()
    {
        Debug.Log("🎒 Creating Backpack Hero style items...");

        // 4x4 그리드에 맞는 작은 아이템들 (그리드는 (3,3)에서 시작)

        // 1. 작은 물약 (1x1)
        GridItem smallPotion = new GridItem("Small Potion", 1, 1, potionSprite);
        smallPotion.color = new Color(1f, 0.3f, 0.3f); // 빨강
        grid.PlaceItem(smallPotion, new Vector2Int(3, 3));

        // 2. 중형 물약 (1x2)
        GridItem mediumPotion = new GridItem("Medium Potion", 1, 2, potionSprite);
        mediumPotion.color = new Color(0.3f, 0.3f, 1f); // 파랑
        grid.PlaceItem(mediumPotion, new Vector2Int(4, 3));

        // 3. 큰 물약 (2x2)
        GridItem largePotion = new GridItem("Large Potion", 2, 2, potionSprite);
        largePotion.color = new Color(0.3f, 1f, 0.3f); // 초록
        grid.PlaceItem(largePotion, new Vector2Int(5, 3));

        // 4. 짧은 검 (1x2)
        GridItem sword = new GridItem("Short Sword", 1, 2, swordSprite);
        sword.color = new Color(0.8f, 0.8f, 0.8f); // 회색 (금속)
        grid.PlaceItem(sword, new Vector2Int(3, 4));

        // 5. 작은 방패 (2x2)
        GridItem shield = new GridItem("Small Shield", 2, 2, shieldSprite);
        shield.color = new Color(0.5f, 0.5f, 0.3f); // 갈색
        grid.PlaceItem(shield, new Vector2Int(3, 5));

        // 6. 반지 (1x1)
        GridItem ring = new GridItem("Gold Ring", 1, 1);
        ring.color = new Color(1f, 0.84f, 0f); // 금색
        grid.PlaceItem(ring, new Vector2Int(5, 5));

        Debug.Log($"✅ Created {grid.GetPlacedItems().Count} items!");
    }

    void Update()
    {
        // 키보드 단축키
        if (Input.GetKeyDown(KeyCode.P))
        {
            grid.PrintGridState();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllItems();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            CreateBackpackItems();
        }

        // 그리드 정보 표시
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log($"ℹ️ {grid.GetGridInfo()}");
        }

        // 구매 모드 토글 (B키)
        if (Input.GetKeyDown(KeyCode.B))
        {
            grid.TogglePurchaseMode();
        }
    }

    void ClearAllItems()
    {
        var items = grid.GetPlacedItems();
        for (int i = items.Count - 1; i >= 0; i--)
        {
            grid.RemoveItem(items[i]);
        }
        Debug.Log("🧹 Cleared all items!");
    }

    /// <summary>
    /// 특정 위치에 아이템 추가 (외부에서 호출 가능)
    /// </summary>
    public bool AddItemAtPosition(string itemName, int width, int height, Vector2Int position, Color color)
    {
        GridItem newItem = new GridItem(itemName, width, height);
        newItem.color = color;

        if (grid.CanPlaceItem(newItem, position))
        {
            grid.PlaceItem(newItem, position);
            Debug.Log($"✅ Added {itemName} at {position}");
            return true;
        }
        else
        {
            Debug.LogWarning($"❌ Cannot add {itemName} at {position}");
            return false;
        }
    }

    /// <summary>
    /// 랜덤 아이템 생성 (테스트용)
    /// </summary>
    public void AddRandomItem()
    {
        int width = Random.Range(1, 3);
        int height = Random.Range(1, 4);
        Color randomColor = new Color(Random.value, Random.value, Random.value);

        GridItem randomItem = new GridItem($"Random_{Random.Range(0, 999)}", width, height);
        randomItem.color = randomColor;

        // 랜덤 위치 찾기
        for (int attempts = 0; attempts < 20; attempts++)
        {
            Vector2Int randomPos = new Vector2Int(
                Random.Range(0, grid.gridWidth - width),
                Random.Range(0, grid.gridHeight - height)
            );

            if (grid.CanPlaceItem(randomItem, randomPos))
            {
                grid.PlaceItem(randomItem, randomPos);
                Debug.Log($"✅ Added random item at {randomPos}");
                return;
            }
        }

        Debug.LogWarning("❌ Could not find space for random item!");
    }
}
