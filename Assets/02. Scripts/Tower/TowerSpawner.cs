using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타워 스폰 시스템
/// 숫자키 0을 누르면 랜덤으로 3개의 타워가 스폰 지점에 생성됨
/// </summary>
public class TowerSpawner : MonoBehaviour
{
    [Header("Tower Data")]
    [SerializeField] private TowerData[] towerDataList; // 타워 데이터 리스트

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints; // 스폰 지점들 (3개 권장)
    [SerializeField] private int towersToSpawn = 3; // 한 번에 생성할 타워 개수
    [SerializeField] private KeyCode spawnKey = KeyCode.Alpha0; // 스폰 키 (숫자 0)

    [Header("Spawn Layout")]
    [SerializeField] private float spacing = 2f; // 타워 간 간격
    [SerializeField] private bool useHorizontalLayout = true; // 가로 배치

    [Header("Block System")]
    [SerializeField] private BlockFactory blockFactory; // 블록 생성 팩토리

    private List<Block> spawnedTowerBlocks = new List<Block>(); // 생성된 타워 블록들
    private List<BlockData> availableBlockShapes; // 사용 가능한 블록 모양 리스트

    void Start()
    {
        // 블록 모양 리스트 초기화
        InitializeBlockShapes();

        // BlockFactory 자동 찾기
        if (blockFactory == null)
        {
            blockFactory = FindObjectOfType<BlockFactory>();
            if (blockFactory != null)
            {
                Debug.Log("✅ TowerSpawner: BlockFactory 자동 찾기 성공");
            }
            else
            {
                Debug.LogError("❌ TowerSpawner: BlockFactory를 찾을 수 없습니다!");
            }
        }
    }

    void Update()
    {
        // 숫자 0 키를 누르면 타워 스폰
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnRandomTowers();
        }

        // C 키로 생성된 타워 모두 제거 (테스트용)
        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearSpawnedTowers();
        }
    }

    /// <summary>
    /// 사용 가능한 블록 모양들을 초기화
    /// </summary>
    void InitializeBlockShapes()
    {
        availableBlockShapes = new List<BlockData>
        {
            BlockFactory.CreateTetrisI(),
            BlockFactory.CreateTetrisO(),
            BlockFactory.CreateTetrisT(),
            BlockFactory.CreateTetrisL(),
            BlockFactory.CreateTetrisJ(),
            BlockFactory.CreateTetrisS(),
            BlockFactory.CreateTetrisZ(),
            BlockFactory.CreatePentominoF(),
            BlockFactory.CreatePentominoP()
        };

        Debug.Log($"✅ TowerSpawner: {availableBlockShapes.Count}개의 블록 모양 초기화 완료");
    }

    /// <summary>
    /// 랜덤으로 타워 스폰
    /// </summary>
    public void SpawnRandomTowers()
    {
        if (towerDataList == null || towerDataList.Length == 0)
        {
            Debug.LogError("❌ TowerSpawner: Tower Data List is empty!");
            return;
        }

        // 스폰 위치 계산
        Vector3 basePosition = GetBaseSpawnPosition();

        Debug.Log($"🏰 Spawning {towersToSpawn} random towers...");

        for (int i = 0; i < towersToSpawn; i++)
        {
            // 가중치 기반 랜덤 선택
            TowerData randomTowerData = GetRandomTowerData();

            // 스폰 위치 계산
            Vector3 spawnPosition = CalculateSpawnPosition(basePosition, i);

            // 타워 생성
            SpawnTower(randomTowerData, spawnPosition);
        }

        Debug.Log($"✅ Spawned {spawnedTowerBlocks.Count} tower blocks!");
    }

    /// <summary>
    /// 가중치 기반 랜덤 타워 선택
    /// </summary>
    TowerData GetRandomTowerData()
    {
        // 가중치 배열 생성
        float[] weights = new float[towerDataList.Length];
        for (int i = 0; i < towerDataList.Length; i++)
        {
            weights[i] = towerDataList[i].spawnWeight;
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
                return towerDataList[i];
            }
        }

        return towerDataList[0]; // 기본값
    }

    /// <summary>
    /// 타워 블록 생성 (BlockFactory 사용)
    /// </summary>
    void SpawnTower(TowerData towerData, Vector3 position)
    {
        if (blockFactory == null)
        {
            Debug.LogError("❌ TowerSpawner: BlockFactory가 없습니다!");
            return;
        }

        // 랜덤 블록 모양 선택
        BlockData randomBlockShape = GetRandomBlockShape();

        // BlockFactory를 사용하여 블록 생성
        Block block = blockFactory.CreateBlock(randomBlockShape, position);

        if (block == null)
        {
            Debug.LogError($"❌ Failed to create tower block for {towerData.towerName}");
            return;
        }

        // 블록 이름 변경
        block.gameObject.name = $"{towerData.towerName}_{randomBlockShape.blockName}";

        // 자식 CellVisual들의 색상을 타워 색상으로 변경
        SpriteRenderer[] cellRenderers = block.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in cellRenderers)
        {
            renderer.color = towerData.towerColor;
        }

        // TowerBlock 컴포넌트 추가 또는 가져오기
        TowerBlock towerBlock = block.GetComponent<TowerBlock>();
        if (towerBlock == null)
        {
            towerBlock = block.gameObject.AddComponent<TowerBlock>();
        }

        // TowerData 적용
        ApplyTowerData(towerBlock, towerData);

        // ✨ 블록을 "배치된 상태"로 마킹 (타워 활성화를 위해 필요)
        block.isPlacedOnGrid = true;

        // 타워 활성화
        towerBlock.ActivateTower();

        // 생성된 타워 블록 리스트에 추가
        spawnedTowerBlocks.Add(block);

        Debug.Log($"🏰 Spawned {towerData.towerName} ({towerData.towerType}) with shape {randomBlockShape.blockName} at {position}");
    }

    /// <summary>
    /// 랜덤 블록 모양 선택
    /// </summary>
    BlockData GetRandomBlockShape()
    {
        if (availableBlockShapes == null || availableBlockShapes.Count == 0)
        {
            Debug.LogWarning("⚠️ No block shapes available! Initializing...");
            InitializeBlockShapes();
        }

        int randomIndex = Random.Range(0, availableBlockShapes.Count);
        return availableBlockShapes[randomIndex];
    }

    /// <summary>
    /// TowerData를 TowerBlock에 적용
    /// </summary>
    void ApplyTowerData(TowerBlock towerBlock, TowerData data)
    {
        // 리플렉션으로 필드 설정
        var towerBlockType = typeof(TowerBlock);

        SetField(towerBlock, towerBlockType, "towerType", ConvertToTowerBlockType(data.towerType));
        SetField(towerBlock, towerBlockType, "attackRange", data.attackRange);
        SetField(towerBlock, towerBlockType, "fireRate", data.fireRate);
        SetField(towerBlock, towerBlockType, "damage", data.damage);
        SetField(towerBlock, towerBlockType, "bulletPrefab", data.bulletPrefab);

        // 타입별 설정
        if (data.towerType == TowerData.TowerType.MeleeTower)
        {
            SetField(towerBlock, towerBlockType, "attackEffectDuration", data.attackEffectDuration);
            SetField(towerBlock, towerBlockType, "slashEffectPrefab", data.slashEffectPrefab);
            SetField(towerBlock, towerBlockType, "attackEffectColor", data.attackEffectColor);
        }
        else if (data.towerType == TowerData.TowerType.CanonTower)
        {
            SetField(towerBlock, towerBlockType, "bulletSpeed", data.bulletSpeed);
            SetField(towerBlock, towerBlockType, "bulletLifeTime", data.bulletLifeTime);
            SetField(towerBlock, towerBlockType, "explosionRadius", data.explosionRadius);
            SetField(towerBlock, towerBlockType, "explosionEffectPrefab", data.explosionEffectPrefab);
        }
        else if (data.towerType == TowerData.TowerType.GlowTower)
        {
            SetField(towerBlock, towerBlockType, "glowDuration", data.glowDuration);
            SetField(towerBlock, towerBlockType, "glowColor", data.glowColor);
            SetField(towerBlock, towerBlockType, "slowAmount", data.slowAmount);
            SetField(towerBlock, towerBlockType, "glowRadius", data.glowRadius);
            SetField(towerBlock, towerBlockType, "glowEffectPrefab", data.glowEffectPrefab);
        }

        // 타워 활성화
        towerBlock.ActivateTower();
    }

    /// <summary>
    /// TowerData를 TowerBase에 직접 적용
    /// </summary>
    void ApplyTowerDataToBase(TowerBase towerBase, TowerData data)
    {
        var baseType = typeof(TowerBase);

        SetField(towerBase, baseType, "Range", data.attackRange);
        SetField(towerBase, baseType, "fireRate", data.fireRate);
        SetField(towerBase, baseType, "damage", data.damage);
        SetField(towerBase, baseType, "bulletPrefab", data.bulletPrefab);
    }

    /// <summary>
    /// TowerData.TowerType을 TowerBlock.TowerType으로 변환
    /// </summary>
    TowerBlock.TowerType ConvertToTowerBlockType(TowerData.TowerType dataType)
    {
        switch (dataType)
        {
            case TowerData.TowerType.RangeTower:
                return TowerBlock.TowerType.RangeTower;
            case TowerData.TowerType.MeleeTower:
                return TowerBlock.TowerType.MeleeTower;
            case TowerData.TowerType.CanonTower:
                return TowerBlock.TowerType.CanonTower;
            case TowerData.TowerType.GlowTower:
                return TowerBlock.TowerType.GlowTower;
            default:
                return TowerBlock.TowerType.RangeTower;
        }
    }

    /// <summary>
    /// 리플렉션으로 필드 설정
    /// </summary>
    void SetField(object obj, System.Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
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
    /// 생성된 모든 타워 블록 제거
    /// </summary>
    public void ClearSpawnedTowers()
    {
        foreach (Block block in spawnedTowerBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }

        spawnedTowerBlocks.Clear();
        Debug.Log("🧹 Cleared all spawned tower blocks!");
    }

    /// <summary>
    /// 스폰된 타워 개수 반환 (GameUIManager용)
    /// </summary>
    public int GetSpawnedTowerCount()
    {
        return spawnedTowerBlocks.Count;
    }

    /// <summary>
    /// 사용 가능한 타워 데이터 개수 반환 (GameUIManager용)
    /// </summary>
    public int GetTowerDataCount()
    {
        return towerDataList?.Length ?? 0;
    }
}
