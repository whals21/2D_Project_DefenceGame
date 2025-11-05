using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 타워 블록 컴포넌트
/// 인접한 타워들에게 버프를 제공하는 보조 타워
/// </summary>
[RequireComponent(typeof(Block))]
public class ItemTowerBlock : MonoBehaviour
{
    [Header("Item Tower Data")]
    [SerializeField] private ItemTowerData itemData;

    [Header("Glow Effect Settings")]
    [SerializeField] private float glowSpeed = 2f; // 반짝임 속도
    [SerializeField] private float minAlpha = 0.3f; // 최소 밝기
    [SerializeField] private float maxAlpha = 1f; // 최대 밝기

    [Header("References")]
    private Block block;
    private GridMapManager gridMapManager;
    private bool isActive = false;

    // 버프를 받고 있는 타워들
    private List<TowerBlock> buffedTowers = new List<TowerBlock>();
    private List<GameObject> buffEffects = new List<GameObject>(); // 버프 이펙트들

    // 버프 효과를 받는 셀들의 SpriteRenderer (반짝임 효과용)
    private Dictionary<TowerBlock, List<SpriteRenderer>> buffedCellRenderers = new Dictionary<TowerBlock, List<SpriteRenderer>>();
    private Coroutine glowCoroutine;

    void Awake()
    {
        block = GetComponent<Block>();
    }

    void Start()
    {
        // GridMapManager 찾기
        if (gridMapManager == null)
        {
            gridMapManager = FindObjectOfType<GridMapManager>();
        }

        // 블록이 이미 배치되어 있으면 활성화
        if (block != null && block.isPlacedOnGrid && !isActive)
        {
            ActivateItemTower();
        }
    }

    /// <summary>
    /// 아이템 타워 활성화 (블록이 그리드에 배치될 때 호출)
    /// </summary>
    public void ActivateItemTower()
    {
        if (isActive || itemData == null)
            return;

        isActive = true;

        // 인접한 타워들 찾아서 버프 적용
        ApplyBuffsToNearbyTowers();

        Debug.Log($"✨ {gameObject.name} 아이템 타워 활성화! (버프 범위: {itemData.buffRange}칸)");
    }

    /// <summary>
    /// 아이템 타워 비활성화 (블록이 그리드에서 제거될 때 호출)
    /// </summary>
    public void DeactivateItemTower()
    {
        if (!isActive)
            return;

        // 버프 제거
        RemoveBuffsFromAllTowers();

        // 버프 이펙트 제거
        ClearBuffEffects();

        isActive = false;
        Debug.Log($"🚫 {gameObject.name} 아이템 타워 비활성화");
    }

    /// <summary>
    /// 인접한 타워들에게 버프 적용
    /// </summary>
    void ApplyBuffsToNearbyTowers()
    {
        if (!block.isPlacedOnGrid || itemData == null)
            return;

        // 버프를 받을 타워들 찾기
        List<TowerBlock> nearbyTowers = FindNearbyTowers();

        foreach (TowerBlock tower in nearbyTowers)
        {
            if (tower == null || buffedTowers.Contains(tower))
                continue;

            // 버프 적용
            ApplyBuffToTower(tower);
            buffedTowers.Add(tower);

            // ✨ 접촉하는 셀들 찾아서 반짝이게 하기
            List<SpriteRenderer> contactCells = FindContactCells(tower);
            if (contactCells.Count > 0)
            {
                buffedCellRenderers[tower] = contactCells;
            }

            // 버프 이펙트 생성 (선택 사항)
            if (itemData.buffEffectPrefab != null)
            {
                GameObject effect = Instantiate(itemData.buffEffectPrefab, tower.transform.position, Quaternion.identity, tower.transform);
                buffEffects.Add(effect);
            }
        }

        Debug.Log($"✅ {buffedTowers.Count}개의 타워에 버프 적용!");

        // ✨ 반짝임 효과 시작
        if (buffedCellRenderers.Count > 0)
        {
            if (glowCoroutine != null)
            {
                StopCoroutine(glowCoroutine);
            }
            glowCoroutine = StartCoroutine(GlowEffect());
        }
    }

    /// <summary>
    /// 인접한 타워 블록들 찾기
    /// </summary>
    List<TowerBlock> FindNearbyTowers()
    {
        List<TowerBlock> nearbyTowers = new List<TowerBlock>();

        if (!block.isPlacedOnGrid)
            return nearbyTowers;

        // 이 아이템 타워가 차지하는 모든 셀 위치
        List<Vector2Int> itemPositions = block.GetWorldCellPositions();

        // 체크할 범위 계산 (각 셀 주변 buffRange 범위)
        HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();

        foreach (Vector2Int itemPos in itemPositions)
        {
            // 각 방향으로 buffRange만큼 탐색
            for (int x = -itemData.buffRange; x <= itemData.buffRange; x++)
            {
                for (int y = -itemData.buffRange; y <= itemData.buffRange; y++)
                {
                    Vector2Int checkPos = itemPos + new Vector2Int(x, y);

                    // 이미 체크했거나 아이템 타워 자신의 위치면 스킵
                    if (checkedPositions.Contains(checkPos) || itemPositions.Contains(checkPos))
                        continue;

                    checkedPositions.Add(checkPos);

                    // 해당 위치의 타워 블록 찾기
                    TowerBlock towerBlock = FindTowerAtPosition(checkPos);
                    if (towerBlock != null && !nearbyTowers.Contains(towerBlock))
                    {
                        nearbyTowers.Add(towerBlock);
                    }
                }
            }
        }

        return nearbyTowers;
    }

    /// <summary>
    /// 특정 그리드 위치에 있는 타워 블록 찾기
    /// </summary>
    TowerBlock FindTowerAtPosition(Vector2Int gridPos)
    {
        // 모든 블록 탐색
        Block[] allBlocks = FindObjectsOfType<Block>();

        foreach (Block otherBlock in allBlocks)
        {
            if (!otherBlock.isPlacedOnGrid || otherBlock == block)
                continue;

            // 해당 블록이 이 위치를 차지하고 있는지 확인
            List<Vector2Int> blockPositions = otherBlock.GetWorldCellPositions();
            if (blockPositions.Contains(gridPos))
            {
                // TowerBlock 컴포넌트 확인
                TowerBlock towerBlock = otherBlock.GetComponent<TowerBlock>();
                if (towerBlock != null && towerBlock.IsTowerActive())
                {
                    return towerBlock;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// 특정 타워에 버프 적용
    /// </summary>
    void ApplyBuffToTower(TowerBlock tower)
    {
        // TowerBase 찾기
        TowerBase towerBase = tower.GetComponent<TowerBase>();
        if (towerBase == null)
            return;

        // 리플렉션으로 필드 접근
        var baseType = typeof(TowerBase);

        // 기존 값 가져오기
        var rangeField = baseType.GetField("Range", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fireRateField = baseType.GetField("fireRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var damageField = baseType.GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (rangeField != null && itemData.rangeMultiplier != 1f)
        {
            float currentRange = (float)rangeField.GetValue(towerBase);
            float newRange = currentRange * itemData.rangeMultiplier;
            rangeField.SetValue(towerBase, newRange);
            Debug.Log($"📏 {tower.gameObject.name} 사거리: {currentRange:F1} → {newRange:F1}");
        }

        if (fireRateField != null && itemData.fireRateMultiplier != 1f)
        {
            float currentFireRate = (float)fireRateField.GetValue(towerBase);
            float newFireRate = currentFireRate * itemData.fireRateMultiplier;
            fireRateField.SetValue(towerBase, newFireRate);
            Debug.Log($"⚡ {tower.gameObject.name} 공격속도: {currentFireRate:F2} → {newFireRate:F2}");
        }

        if (damageField != null && itemData.damageMultiplier != 1f)
        {
            int currentDamage = (int)damageField.GetValue(towerBase);
            int newDamage = Mathf.RoundToInt(currentDamage * itemData.damageMultiplier);
            damageField.SetValue(towerBase, newDamage);
            Debug.Log($"💥 {tower.gameObject.name} 공격력: {currentDamage} → {newDamage}");
        }

        // CircleCollider2D 범위도 업데이트
        CircleCollider2D rangeCollider = tower.GetComponent<CircleCollider2D>();
        if (rangeCollider != null && itemData.rangeMultiplier != 1f)
        {
            rangeCollider.radius *= itemData.rangeMultiplier;
        }
    }

    /// <summary>
    /// 모든 타워에서 버프 제거
    /// </summary>
    void RemoveBuffsFromAllTowers()
    {
        foreach (TowerBlock tower in buffedTowers)
        {
            if (tower == null)
                continue;

            RemoveBuffFromTower(tower);
        }

        buffedTowers.Clear();
    }

    /// <summary>
    /// 특정 타워에서 버프 제거
    /// </summary>
    void RemoveBuffFromTower(TowerBlock tower)
    {
        TowerBase towerBase = tower.GetComponent<TowerBase>();
        if (towerBase == null)
            return;

        // 리플렉션으로 필드 접근
        var baseType = typeof(TowerBase);

        var rangeField = baseType.GetField("Range", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fireRateField = baseType.GetField("fireRate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var damageField = baseType.GetField("damage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        // 버프 효과를 역으로 제거 (나눗셈)
        if (rangeField != null && itemData.rangeMultiplier != 1f)
        {
            float currentRange = (float)rangeField.GetValue(towerBase);
            float originalRange = currentRange / itemData.rangeMultiplier;
            rangeField.SetValue(towerBase, originalRange);
        }

        if (fireRateField != null && itemData.fireRateMultiplier != 1f)
        {
            float currentFireRate = (float)fireRateField.GetValue(towerBase);
            float originalFireRate = currentFireRate / itemData.fireRateMultiplier;
            fireRateField.SetValue(towerBase, originalFireRate);
        }

        if (damageField != null && itemData.damageMultiplier != 1f)
        {
            int currentDamage = (int)damageField.GetValue(towerBase);
            int originalDamage = Mathf.RoundToInt(currentDamage / itemData.damageMultiplier);
            damageField.SetValue(towerBase, originalDamage);
        }

        // CircleCollider2D 범위도 복원
        CircleCollider2D rangeCollider = tower.GetComponent<CircleCollider2D>();
        if (rangeCollider != null && itemData.rangeMultiplier != 1f)
        {
            rangeCollider.radius /= itemData.rangeMultiplier;
        }
    }

    /// <summary>
    /// 아이템 타워와 접촉하는 타워 블록의 셀들 찾기
    /// </summary>
    List<SpriteRenderer> FindContactCells(TowerBlock tower)
    {
        List<SpriteRenderer> contactCells = new List<SpriteRenderer>();

        if (!block.isPlacedOnGrid)
            return contactCells;

        // 아이템 타워가 차지하는 셀 위치들
        List<Vector2Int> itemPositions = block.GetWorldCellPositions();
        HashSet<Vector2Int> itemPosSet = new HashSet<Vector2Int>(itemPositions);

        // 타워 블록이 차지하는 셀 위치들
        Block towerBlock = tower.GetComponent<Block>();
        if (towerBlock == null || !towerBlock.isPlacedOnGrid)
            return contactCells;

        List<Vector2Int> towerPositions = towerBlock.GetWorldCellPositions();

        // 타워 블록의 모든 자식 SpriteRenderer 가져오기
        SpriteRenderer[] allRenderers = towerBlock.GetComponentsInChildren<SpriteRenderer>();

        // 각 타워 셀 위치에 대해 인접성 체크
        for (int i = 0; i < towerPositions.Count && i < allRenderers.Length; i++)
        {
            Vector2Int towerPos = towerPositions[i];

            // 8방향 인접 체크 (상하좌우 + 대각선)
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // 위
                new Vector2Int(0, -1),  // 아래
                new Vector2Int(-1, 0),  // 왼쪽
                new Vector2Int(1, 0),   // 오른쪽
                new Vector2Int(-1, 1),  // 좌상단
                new Vector2Int(1, 1),   // 우상단
                new Vector2Int(-1, -1), // 좌하단
                new Vector2Int(1, -1)   // 우하단
            };

            // 인접한 위치에 아이템 타워 셀이 있는지 확인
            bool isContact = false;
            foreach (Vector2Int dir in directions)
            {
                Vector2Int checkPos = towerPos + dir;
                if (itemPosSet.Contains(checkPos))
                {
                    isContact = true;
                    break;
                }
            }

            // 접촉하는 셀이면 SpriteRenderer 추가
            if (isContact && allRenderers[i] != null)
            {
                contactCells.Add(allRenderers[i]);
            }
        }

        Debug.Log($"✨ {tower.gameObject.name}의 {contactCells.Count}개 셀이 아이템 타워와 접촉!");
        return contactCells;
    }

    /// <summary>
    /// 반짝임 효과 코루틴
    /// </summary>
    IEnumerator GlowEffect()
    {
        // 원래 색상 저장
        Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
        foreach (var kvp in buffedCellRenderers)
        {
            foreach (SpriteRenderer renderer in kvp.Value)
            {
                if (renderer != null)
                {
                    originalColors[renderer] = renderer.color;
                }
            }
        }

        while (isActive && buffedCellRenderers.Count > 0)
        {
            // Ping-Pong으로 알파값 변화 (minAlpha <-> maxAlpha)
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f);

            // 모든 접촉 셀의 알파값 업데이트
            foreach (var kvp in buffedCellRenderers)
            {
                foreach (SpriteRenderer renderer in kvp.Value)
                {
                    if (renderer != null && originalColors.ContainsKey(renderer))
                    {
                        Color color = originalColors[renderer];
                        color.a = alpha;
                        renderer.color = color;
                    }
                }
            }

            yield return null;
        }

        // 원래 색상으로 복원
        foreach (var kvp in originalColors)
        {
            if (kvp.Key != null)
            {
                kvp.Key.color = kvp.Value;
            }
        }
    }

    /// <summary>
    /// 버프 이펙트 모두 제거
    /// </summary>
    void ClearBuffEffects()
    {
        // 반짝임 효과 중지
        if (glowCoroutine != null)
        {
            StopCoroutine(glowCoroutine);
            glowCoroutine = null;
        }

        // 원래 색상 복원
        foreach (var kvp in buffedCellRenderers)
        {
            foreach (SpriteRenderer renderer in kvp.Value)
            {
                if (renderer != null)
                {
                    Color color = renderer.color;
                    color.a = 1f; // 알파값 1로 복원
                    renderer.color = color;
                }
            }
        }
        buffedCellRenderers.Clear();

        // 버프 이펙트 오브젝트 제거
        foreach (GameObject effect in buffEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        buffEffects.Clear();
    }

    /// <summary>
    /// ItemTowerData 설정 (외부에서 호출)
    /// </summary>
    public void SetItemData(ItemTowerData data)
    {
        itemData = data;
    }

    /// <summary>
    /// 현재 활성화 상태 반환
    /// </summary>
    public bool IsActive()
    {
        return isActive;
    }

    /// <summary>
    /// Gizmos로 버프 범위 시각화
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!isActive || itemData == null || !block.isPlacedOnGrid)
            return;

        Gizmos.color = new Color(1f, 1f, 0f, 0.3f); // 노란색 반투명

        // 아이템 타워가 차지하는 각 셀에서 버프 범위 그리기
        List<Vector2Int> itemPositions = block.GetWorldCellPositions();
        foreach (Vector2Int pos in itemPositions)
        {
            Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
            Gizmos.DrawWireCube(worldPos, Vector3.one * itemData.buffRange * 2);
        }
    }

    void OnDestroy()
    {
        DeactivateItemTower();
    }
}
