using UnityEngine;
using System.Collections.Generic;

public class SpriteSynergySystem : MonoBehaviour
{
    [System.Serializable]
    public class SynergyRule
    {
        public string name;
        public string item1Type;
        public string item2Type;
        public string description;
        public float bonusMultiplier = 1.5f;
    }
    
    [Header("References")]
    public SpriteInventoryGrid inventoryGrid;
    
    [Header("Synergy Rules")]
    public List<SynergyRule> synergyRules = new List<SynergyRule>();
    
    [Header("Visual Settings")]
    public Color synergyLineColor = Color.yellow;
    public float lineWidth = 0.1f;
    
    private List<string> activeSynergies = new List<string>();
    private List<LineRenderer> synergyLines = new List<LineRenderer>();
    
    void Start()
    {
        SetupDefaultSynergies();
    }
    
    void SetupDefaultSynergies()
    {
        synergyRules.Add(new SynergyRule
        {
            name = "화염 폭발",
            item1Type = "Sword",
            item2Type = "Shield",
            description = "검 + 방패 인접 시 데미지 2배",
            bonusMultiplier = 2.0f
        });
        
        synergyRules.Add(new SynergyRule
        {
            name = "포션 강화",
            item1Type = "Potion",
            item2Type = "Armor",
            description = "포션 + 갑옷 인접 시 회복량 증가",
            bonusMultiplier = 1.5f
        });
    }
    
    // 모든 시너지 체크
    public void CheckAllSynergies()
    {
        ClearSynergyLines();
        activeSynergies.Clear();
        
        List<GridItem> items = inventoryGrid.GetPlacedItems();
        
        foreach (var item in items)
        {
            CheckItemSynergies(item);
        }
        
        DisplayActiveSynergies();
    }
    
    // 특정 아이템의 시너지 체크
    void CheckItemSynergies(GridItem item)
    {
        List<GridItem> neighbors = GetAdjacentItems(item);
        
        foreach (var neighbor in neighbors)
        {
            CheckSynergyBetweenItems(item, neighbor);
        }
    }
    
    // 두 아이템 간 시너지 체크
    void CheckSynergyBetweenItems(GridItem item1, GridItem item2)
    {
        foreach (var rule in synergyRules)
        {
            if ((item1.itemName.Contains(rule.item1Type) && item2.itemName.Contains(rule.item2Type)) ||
                (item1.itemName.Contains(rule.item2Type) && item2.itemName.Contains(rule.item1Type)))
            {
                string synergyKey = $"{rule.name}:{item1.gridPosition}-{item2.gridPosition}";
                
                if (!activeSynergies.Contains(synergyKey))
                {
                    activeSynergies.Add(synergyKey);
                    Debug.Log($"⚡ 시너지 발동! {rule.name}: {item1.itemName} + {item2.itemName}");
                    
                    // 시너지 연결선 그리기
                    DrawSynergyLine(item1, item2);
                }
            }
        }
    }
    
    // 인접 아이템 찾기
    List<GridItem> GetAdjacentItems(GridItem item)
    {
        List<GridItem> neighbors = new List<GridItem>();
        Vector2Int pos = item.gridPosition;
        Vector2Int size = item.GetRotatedSize();
        
        HashSet<Vector2Int> checkPositions = new HashSet<Vector2Int>();
        
        for (int x = -1; x <= size.x; x++)
        {
            for (int y = -1; y <= size.y; y++)
            {
                Vector2Int checkPos = pos + new Vector2Int(x, y);
                bool isInsideItem = (x >= 0 && x < size.x && y >= 0 && y < size.y);
                
                if (!isInsideItem)
                {
                    checkPositions.Add(checkPos);
                }
            }
        }
        
        foreach (var checkPos in checkPositions)
        {
            GridItem neighborItem = inventoryGrid.GetItemAtPosition(checkPos);
            if (neighborItem != null && neighborItem != item && !neighbors.Contains(neighborItem))
            {
                neighbors.Add(neighborItem);
            }
        }
        
        return neighbors;
    }
    
    // 시너지 연결선 그리기
    void DrawSynergyLine(GridItem item1, GridItem item2)
    {
        GameObject lineObj = new GameObject($"SynergyLine_{item1.itemName}_{item2.itemName}");
        lineObj.transform.SetParent(transform);
        
        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = synergyLineColor;
        lr.endColor = synergyLineColor;
        lr.sortingOrder = 5;
        lr.positionCount = 2;
        
        Vector3 pos1 = inventoryGrid.GridToWorldPosition(item1.gridPosition);
        Vector3 pos2 = inventoryGrid.GridToWorldPosition(item2.gridPosition);
        
        Vector2Int size1 = item1.GetRotatedSize();
        Vector2Int size2 = item2.GetRotatedSize();
        
        pos1 += new Vector3(size1.x * inventoryGrid.cellSize / 2f, size1.y * inventoryGrid.cellSize / 2f, -0.5f);
        pos2 += new Vector3(size2.x * inventoryGrid.cellSize / 2f, size2.y * inventoryGrid.cellSize / 2f, -0.5f);
        
        lr.SetPosition(0, pos1);
        lr.SetPosition(1, pos2);
        
        synergyLines.Add(lr);
    }
    
    // 시너지 라인 제거
    void ClearSynergyLines()
    {
        foreach (var line in synergyLines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        synergyLines.Clear();
    }
    
    void DisplayActiveSynergies()
    {
        if (activeSynergies.Count > 0)
        {
            Debug.Log($"=== 활성 시너지: {activeSynergies.Count}개 ===");
        }
        else
        {
            Debug.Log("활성화된 시너지가 없습니다.");
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            CheckAllSynergies();
        }
    }
}
