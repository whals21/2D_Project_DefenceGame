using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템 타워 스폰 시스템
/// - 키를 누르면 랜덤으로 아이템 타워 블록이 스폰됨
/// - Minus(-) 키: 아이템 타워 3개 스폰
/// - Equals(=) 키: 생성된 아이템 타워 모두 제거
/// </summary>
public class ItemTowerSpawner : MonoBehaviour
{
    [Header("Item Tower Data")]
    [SerializeField] private ItemTowerData[] itemDataList; // 아이템 타워 데이터 리스트

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints; // 스폰 지점들
    [SerializeField] private int itemsToSpawn = 3; // 한 번에 생성할 아이템 개수
    [SerializeField] private KeyCode spawnKey = KeyCode.Minus; // 스폰 키 (-)
    [SerializeField] private KeyCode clearKey = KeyCode.Equals; // 제거 키 (=)

    [Header("Spawn Layout")]
    [SerializeField] private float spacing = 2f; // 아이템 간 간격
    [SerializeField] private bool useHorizontalLayout = true; // 가로 배치

    [Header("Block System")]
    [SerializeField] private BlockFactory blockFactory; // 블록 생성 팩토리

    private List<Block> spawnedItemBlocks = new List<Block>(); // 생성된 아이템 블록들
    private List<BlockData> availableItemShapes; // 사용 가능한 아이템 블록 모양 리스트

    void Start()
    {
        // 아이템 블록 모양 리스트 초기화
        InitializeItemShapes();

        // BlockFactory 자동 찾기
        if (blockFactory == null)
        {
            blockFactory = FindObjectOfType<BlockFactory>();
            if (blockFactory != null)
            {
                Debug.Log("✅ ItemTowerSpawner: BlockFactory 자동 찾기 성공");
            }
            else
            {
                Debug.LogError("❌ ItemTowerSpawner: BlockFactory를 찾을 수 없습니다!");
            }
        }
    }

    void Update()
    {
        // KeyBindingManager에서 설정한 키로 아이템 타워 스폰
        if (KeyBindingManager.Instance != null && KeyBindingManager.Instance.GetSpawnItemTowerKeyDown())
        {
            SpawnRandomItemTowers();
        }

        // KeyBindingManager에서 설정한 키로 생성된 아이템 타워 모두 제거
        if (KeyBindingManager.Instance != null && KeyBindingManager.Instance.GetClearItemTowerKeyDown())
        {
            ClearSpawnedItems();
        }
    }

    /// <summary>
    /// 사용 가능한 아이템 블록 모양들을 초기화 (1-3칸)
    /// </summary>
    void InitializeItemShapes()
    {
        availableItemShapes = new List<BlockData>
        {
            BlockFactory.CreateItemSingle(),      // 1칸
            BlockFactory.CreateItemLine2H(),      // 2칸 가로
            BlockFactory.CreateItemLine2V(),      // 2칸 세로
            BlockFactory.CreateItemDiagonal2(),   // 2칸 대각선
            BlockFactory.CreateItemLine3H(),      // 3칸 가로
            BlockFactory.CreateItemLine3V(),      // 3칸 세로
            BlockFactory.CreateItemL3(),          // 3칸 L자
            BlockFactory.CreateItemT3()           // 3칸 T자 (4칸이지만 작은 편)
        };

        Debug.Log($"✅ ItemTowerSpawner: {availableItemShapes.Count}개의 아이템 블록 모양 초기화 완료");
    }

    /// <summary>
    /// 랜덤으로 아이템 타워 스폰
    /// </summary>
    public void SpawnRandomItemTowers()
    {
        if (itemDataList == null || itemDataList.Length == 0)
        {
            Debug.LogError("❌ ItemTowerSpawner: Item Data List is empty!");
            return;
        }

        // 스폰 위치 계산
        Vector3 basePosition = GetBaseSpawnPosition();

        Debug.Log($"✨ Spawning {itemsToSpawn} random item towers...");

        for (int i = 0; i < itemsToSpawn; i++)
        {
            // 가중치 기반 랜덤 선택
            ItemTowerData randomItemData = GetRandomItemData();

            // 스폰 위치 계산
            Vector3 spawnPosition = CalculateSpawnPosition(basePosition, i);

            // 아이템 타워 생성
            SpawnItemTower(randomItemData, spawnPosition);
        }

        Debug.Log($"✅ Spawned {spawnedItemBlocks.Count} item tower blocks!");
    }

    /// <summary>
    /// 가중치 기반 랜덤 아이템 데이터 선택
    /// </summary>
    ItemTowerData GetRandomItemData()
    {
        // 가중치 배열 생성
        float[] weights = new float[itemDataList.Length];
        for (int i = 0; i < itemDataList.Length; i++)
        {
            weights[i] = itemDataList[i].spawnWeight;
        }

        // 가중치 누적합 계산
        float cumulativeWeight = 0f;
        float[] cumulativeWeights = new float[weights.Length];
        for (int i = 0; i < weights.Length; i++)
        {
            cumulativeWeight += weights[i];
            cumulativeWeights[i] = cumulativeWeight;
        }

        // 누적합 기반 랜덤 선택
        float randomValue = Random.value * cumulativeWeight;
        for (int i = 0; i < weights.Length; i++)
        {
            if (randomValue <= cumulativeWeights[i])
            {
                return itemDataList[i];
            }
        }

        return itemDataList[0]; // 기본값
    }

    /// <summary>
    /// 아이템 타워 블록 생성 (BlockFactory 사용)
    /// </summary>
    void SpawnItemTower(ItemTowerData itemData, Vector3 position)
    {
        if (blockFactory == null)
        {
            Debug.LogError("❌ ItemTowerSpawner: BlockFactory가 없습니다!");
            return;
        }

        // 랜덤 블록 모양 선택 (1-3칸)
        BlockData randomBlockShape = GetRandomItemShape();

        // BlockFactory를 사용하여 블록 생성
        Block block = blockFactory.CreateBlock(randomBlockShape, position);

        if (block == null)
        {
            Debug.LogError($"❌ Failed to create item tower block for {itemData.itemName}");
            return;
        }

        // 블록 이름 변경
        block.gameObject.name = $"{itemData.itemName}_{randomBlockShape.blockName}";

        // ✨ CRITICAL: TowerBlock과 TowerBase 컴포넌트 제거 (아이템 타워는 공격하면 안 됨!)
        TowerBlock existingTowerBlock = block.GetComponent<TowerBlock>();
        if (existingTowerBlock != null)
        {
            DestroyImmediate(existingTowerBlock);
            Debug.Log($"⚠️ Removed TowerBlock component from {block.gameObject.name}");
        }

        // TowerBase 상속 컴포넌트들도 모두 제거 (RangeTower_1 등)
        TowerBase[] towerBases = block.GetComponents<TowerBase>();
        foreach (TowerBase towerBase in towerBases)
        {
            if (towerBase != null)
            {
                DestroyImmediate(towerBase);
                Debug.Log($"⚠️ Removed {towerBase.GetType().Name} component from {block.gameObject.name}");
            }
        }

        // CircleCollider2D도 제거 (타워 사거리용)
        CircleCollider2D circleCollider = block.GetComponent<CircleCollider2D>();
        if (circleCollider != null)
        {
            DestroyImmediate(circleCollider);
            Debug.Log($"⚠️ Removed CircleCollider2D from {block.gameObject.name}");
        }

        // ✨ 자식 CellVisual들의 시각적 효과 적용 (아이템 타워 구별용)
        SpriteRenderer[] cellRenderers = block.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in cellRenderers)
        {
            // 1. 색상 적용
            renderer.color = itemData.itemColor;

            // 2. 약간 반투명하게 설정 (일반 타워와 구별)
            Color colorWithAlpha = itemData.itemColor;
            colorWithAlpha.a = 0.85f; // 85% 불투명도
            renderer.color = colorWithAlpha;

            // 3. 렌더링 순서 변경 (위에 보이도록)
            renderer.sortingOrder = 5;

            // 4. Material 속성 변경 (밝기 증가)
            if (renderer.material != null)
            {
                // Sprite-Default 머티리얼의 색상 속성 조정
                renderer.material.color = Color.white;
            }
        }

        // ItemTowerBlock 컴포넌트 추가 또는 가져오기
        ItemTowerBlock itemTowerBlock = block.GetComponent<ItemTowerBlock>();
        if (itemTowerBlock == null)
        {
            itemTowerBlock = block.gameObject.AddComponent<ItemTowerBlock>();
        }

        // ItemTowerData 적용
        itemTowerBlock.SetItemData(itemData);

        // ✨ 블록을 "배치된 상태"로 마킹 (아이템 타워 활성화를 위해 필요)
        block.isPlacedOnGrid = true;

        // 아이템 타워 활성화
        itemTowerBlock.ActivateItemTower();

        // ✨ NEW: 시각적 효과 컴포넌트 추가
        AddVisualEffects(block.gameObject, cellRenderers);

        // 생성된 아이템 블록 리스트에 추가
        spawnedItemBlocks.Add(block);

        Debug.Log($"✨ Spawned {itemData.itemName} with shape {randomBlockShape.blockName} at {position}");
    }

    /// <summary>
    /// 아이템 타워에 시각적 효과 추가 (펄스, 외곽선 등)
    /// </summary>
    void AddVisualEffects(GameObject itemTowerObj, SpriteRenderer[] cellRenderers)
    {
        // 1. 펄스 효과 컴포넌트 추가
        ItemTowerPulseEffect pulseEffect = itemTowerObj.AddComponent<ItemTowerPulseEffect>();
        pulseEffect.Initialize(cellRenderers);

        // 2. 각 셀에 외곽선 효과 추가
        foreach (SpriteRenderer cellRenderer in cellRenderers)
        {
            if (cellRenderer != null)
            {
                AddOutlineToCell(cellRenderer);
            }
        }
    }

    /// <summary>
    /// 개별 셀에 외곽선 효과 추가 (그림자 스프라이트 생성)
    /// </summary>
    void AddOutlineToCell(SpriteRenderer cellRenderer)
    {
        // 외곽선용 GameObject 생성
        GameObject outlineObj = new GameObject("Outline");
        outlineObj.transform.SetParent(cellRenderer.transform);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localRotation = Quaternion.identity;
        outlineObj.transform.localScale = Vector3.one * 1.08f; // 8% 크게

        // SpriteRenderer 복사
        SpriteRenderer outlineRenderer = outlineObj.AddComponent<SpriteRenderer>();
        outlineRenderer.sprite = cellRenderer.sprite;
        outlineRenderer.sortingLayerName = cellRenderer.sortingLayerName;
        outlineRenderer.sortingOrder = cellRenderer.sortingOrder - 1; // 뒤에 렌더링

        // 외곽선 색상 (밝은 노란색 또는 흰색)
        outlineRenderer.color = new Color(1f, 1f, 0.5f, 0.4f); // 반투명 노란빛
    }

    /// <summary>
    /// 랜덤 아이템 블록 모양 선택
    /// </summary>
    BlockData GetRandomItemShape()
    {
        if (availableItemShapes == null || availableItemShapes.Count == 0)
        {
            Debug.LogWarning("⚠️ No item block shapes available! Initializing...");
            InitializeItemShapes();
        }

        int randomIndex = Random.Range(0, availableItemShapes.Count);
        return availableItemShapes[randomIndex];
    }

    /// <summary>
    /// 기본 스폰 위치 계산
    /// </summary>
    Vector3 GetBaseSpawnPosition()
    {
        // 스폰 포인트가 설정되어 있으면 첫 번째 포인트 사용
        if (spawnPoints != null && spawnPoints.Length > 0 && spawnPoints[0] != null)
        {
            return spawnPoints[0].position;
        }

        // 없으면 이 오브젝트 위치 사용
        return transform.position;
    }

    /// <summary>
    /// 인덱스에 따른 스폰 위치 계산
    /// </summary>
    Vector3 CalculateSpawnPosition(Vector3 basePosition, int index)
    {
        // 개별 스폰 포인트가 있으면 사용
        if (spawnPoints != null && index < spawnPoints.Length && spawnPoints[index] != null)
        {
            return spawnPoints[index].position;
        }

        // 없으면 간격을 두고 배치
        if (useHorizontalLayout)
        {
            return basePosition + new Vector3(index * spacing, 0, 0);
        }
        else
        {
            return basePosition + new Vector3(0, -index * spacing, 0);
        }
    }

    /// <summary>
    /// 생성된 모든 아이템 타워 블록 제거
    /// </summary>
    public void ClearSpawnedItems()
    {
        foreach (Block block in spawnedItemBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }

        spawnedItemBlocks.Clear();
        Debug.Log("🧹 Cleared all spawned item tower blocks!");
    }

    /// <summary>
    /// 스폰된 아이템 타워 개수 반환 (GameUIManager용)
    /// </summary>
    public int GetSpawnedItemCount()
    {
        return spawnedItemBlocks.Count;
    }

    /// <summary>
    /// 사용 가능한 아이템 데이터 개수 반환 (GameUIManager용)
    /// </summary>
    public int GetItemDataCount()
    {
        return itemDataList?.Length ?? 0;
    }
}
