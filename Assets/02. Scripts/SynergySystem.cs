using UnityEngine;
using System.Collections.Generic;

public class SynergySystem : MonoBehaviour
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
    public InventoryGrid inventoryGrid;
    
    [Header("Synergy Rules")]
    public List<SynergyRule> synergyRules = new List<SynergyRule>();
    
    // 활성화된 시너지 목록
    private List<string> activeSynergies = new List<string>();
    
    void Start()
    {
        // 기본 시너지 규칙 설정
        SetupDefaultSynergies();
    }
    
    void SetupDefaultSynergies()
    {
        synergyRules.Add(new SynergyRule
        {
            name = "화염 폭발",
            item1Type = "Cannon",
            item2Type = "Slow",
            description = "대포 + 슬로우 타워 인접 시 데미지 2배",
            bonusMultiplier = 2.0f
        });
        
        synergyRules.Add(new SynergyRule
        {
            name = "집중 포화",
            item1Type = "Arrow",
            item2Type = "Buff",
            description = "화살 + 버프 타워 인접 시 공격속도 +50%",
            bonusMultiplier = 1.5f
        });
        
        synergyRules.Add(new SynergyRule
        {
            name = "마법 증폭",
            item1Type = "Magic",
            item2Type = "Magic",
            description = "마법 타워 2개 인접 시 관통력 증가",
            bonusMultiplier = 1.3f
        });
    }
    
    // 모든 시너지 체크 (그리드 변경 시 호출)
    public void CheckAllSynergies()
    {
        activeSynergies.Clear();
        
        List<InventoryItem> items = inventoryGrid.GetPlacedItems();
        
        foreach (var item in items)
        {
            CheckItemSynergies(item);
        }
        
        DisplayActiveSynergies();
    }
    
    // 특정 아이템의 시너지 체크
    void CheckItemSynergies(InventoryItem item)
    {
        List<InventoryItem> neighbors = GetAdjacentItems(item);
        
        foreach (var neighbor in neighbors)
        {
            CheckSynergyBetweenItems(item, neighbor);
        }
    }
    
    // 두 아이템 간 시너지 체크
    void CheckSynergyBetweenItems(InventoryItem item1, InventoryItem item2)
    {
        foreach (var rule in synergyRules)
        {
            // 양방향 체크 (item1-item2 또는 item2-item1)
            if ((item1.itemName.Contains(rule.item1Type) && item2.itemName.Contains(rule.item2Type)) ||
                (item1.itemName.Contains(rule.item2Type) && item2.itemName.Contains(rule.item1Type)))
            {
                string synergyKey = $"{rule.name}:{item1.gridPosition}-{item2.gridPosition}";
                
                if (!activeSynergies.Contains(synergyKey))
                {
                    activeSynergies.Add(synergyKey);
                    Debug.Log($"⚡ 시너지 발동! {rule.name}: {item1.itemName} + {item2.itemName}");
                    
                    // 시너지 효과 적용 (실제 게임에서는 스탯 증가 등)
                    ApplySynergyEffect(item1, item2, rule);
                }
            }
        }
    }
    
    // 인접한 아이템 가져오기
    List<InventoryItem> GetAdjacentItems(InventoryItem item)
    {
        List<InventoryItem> neighbors = new List<InventoryItem>();
        Vector2Int pos = item.gridPosition;
        Vector2Int size = item.GetRotatedSize();
        
        // 8방향 + 상하좌우 인접 체크
        HashSet<Vector2Int> checkPositions = new HashSet<Vector2Int>();
        
        // 아이템이 차지하는 모든 셀의 인접 위치 체크
        for (int x = -1; x <= size.x; x++)
        {
            for (int y = -1; y <= size.y; y++)
            {
                Vector2Int checkPos = pos + new Vector2Int(x, y);
                
                // 아이템 자신의 영역은 제외
                bool isInsideItem = (x >= 0 && x < size.x && y >= 0 && y < size.y);
                if (!isInsideItem)
                {
                    checkPositions.Add(checkPos);
                }
            }
        }
        
        // 각 위치에서 아이템 찾기
        foreach (var checkPos in checkPositions)
        {
            InventoryItem neighborItem = inventoryGrid.GetItemAtPosition(checkPos);
            if (neighborItem != null && neighborItem != item && !neighbors.Contains(neighborItem))
            {
                neighbors.Add(neighborItem);
            }
        }
        
        return neighbors;
    }
    
    // 시너지 효과 적용 (예시)
    void ApplySynergyEffect(InventoryItem item1, InventoryItem item2, SynergyRule rule)
    {
        // 실제 게임에서는 여기서 스탯 증가, 이펙트 생성 등을 처리
        // 예: item1의 데미지 증가, 공격속도 증가 등
        
        Debug.Log($"  → {item1.itemName}에 {rule.description} 효과 적용");
        Debug.Log($"  → 보너스 배율: {rule.bonusMultiplier}x");
    }
    
    // 활성화된 시너지 표시
    void DisplayActiveSynergies()
    {
        if (activeSynergies.Count > 0)
        {
            Debug.Log($"=== 활성 시너지: {activeSynergies.Count}개 ===");
            foreach (var synergy in activeSynergies)
            {
                Debug.Log($"  • {synergy}");
            }
        }
        else
        {
            Debug.Log("활성화된 시너지가 없습니다.");
        }
    }
    
    // 특정 패턴 체크 (예: 정사각형 배치)
    public bool CheckSquarePattern(Vector2Int topLeft, int size)
    {
        for (int x = 0; x < size; x++)
        {
            for (int y = 0; y < size; y++)
            {
                Vector2Int pos = topLeft + new Vector2Int(x, y);
                if (inventoryGrid.GetItemAtPosition(pos) == null)
                {
                    return false;
                }
            }
        }
        
        Debug.Log($"⭐ 정사각형 패턴 발견! ({size}x{size})");
        return true;
    }
    
    // 일직선 패턴 체크
    public bool CheckLinePattern(Vector2Int start, Vector2Int direction, int length)
    {
        string firstItemType = null;
        
        for (int i = 0; i < length; i++)
        {
            Vector2Int pos = start + direction * i;
            InventoryItem item = inventoryGrid.GetItemAtPosition(pos);
            
            if (item == null)
                return false;
            
            if (firstItemType == null)
            {
                firstItemType = item.itemName;
            }
            else if (item.itemName != firstItemType)
            {
                return false;
            }
        }
        
        Debug.Log($"⭐ 일직선 패턴 발견! {firstItemType} x {length}");
        return true;
    }
    
    // 같은 타입 카운트
    public int CountItemType(string itemType)
    {
        int count = 0;
        foreach (var item in inventoryGrid.GetPlacedItems())
        {
            if (item.itemName.Contains(itemType))
            {
                count++;
            }
        }
        return count;
    }
    
    // 타입별 시너지 보너스 계산
    public float GetSynergyBonus(string itemType)
    {
        float bonus = 0f;
        
        // 같은 타입이 많을수록 보너스
        int count = CountItemType(itemType);
        if (count >= 3)
        {
            bonus += 0.2f; // 20% 보너스
        }
        if (count >= 5)
        {
            bonus += 0.3f; // 추가 30% 보너스
        }
        
        return bonus;
    }
    
    void Update()
    {
        // S키로 시너지 체크
        if (Input.GetKeyDown(KeyCode.S))
        {
            CheckAllSynergies();
        }
        
        // Q키로 정사각형 패턴 체크 (예시: 0,0 위치에서 2x2)
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CheckSquarePattern(new Vector2Int(0, 0), 2);
        }
        
        // W키로 일직선 패턴 체크 (예시: 0,0에서 오른쪽으로 3칸)
        if (Input.GetKeyDown(KeyCode.W))
        {
            CheckLinePattern(new Vector2Int(0, 0), Vector2Int.right, 3);
        }
    }
}
