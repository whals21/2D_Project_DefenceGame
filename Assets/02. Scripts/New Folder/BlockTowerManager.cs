using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 블록 타워 시스템 전체 관리
/// 블록이 그리드에 배치/제거될 때 타워 활성화/비활성화 자동 처리
/// </summary>
public class BlockTowerManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GridMapManager gridMapManager;
    [SerializeField] private BlockPlacer blockPlacer;

    [Header("Tower Settings")]
    [SerializeField] private bool autoActivateTowers = true; // 블록 배치 시 자동으로 타워 활성화

    // 관리 중인 타워 블록들
    private Dictionary<Block, TowerBlock> activeTowerBlocks = new Dictionary<Block, TowerBlock>();

    void Start()
    {
        // GridMapManager 자동 탐색
        if (gridMapManager == null)
        {
            gridMapManager = FindObjectOfType<GridMapManager>();
        }

        // BlockPlacer 자동 탐색
        if (blockPlacer == null)
        {
            blockPlacer = FindObjectOfType<BlockPlacer>();
        }

        // BlockPlacer 이벤트 구독 (블록 배치/제거 감지)
        if (blockPlacer != null)
        {
            // BlockPlacer에 이벤트가 있다면 구독
            // 예: blockPlacer.OnBlockPlaced += OnBlockPlaced;
            // 예: blockPlacer.OnBlockRemoved += OnBlockRemoved;
        }
    }

    void Update()
    {
        // 수동으로 배치된 블록들을 검색하여 타워 활성화 (테스트용)
        if (Input.GetKeyDown(KeyCode.T))
        {
            ScanAndActivateAllTowers();
        }

        // 모든 타워 비활성화 (테스트용)
        if (Input.GetKeyDown(KeyCode.Y))
        {
            DeactivateAllTowers();
        }
    }

    /// <summary>
    /// 블록이 그리드에 배치될 때 호출
    /// </summary>
    public void OnBlockPlaced(Block block)
    {
        if (block == null || !autoActivateTowers)
            return;

        // TowerBlock 컴포넌트 확인
        TowerBlock towerBlock = block.GetComponent<TowerBlock>();

        if (towerBlock == null)
        {
            // TowerBlock이 없으면 추가 (선택사항)
            // towerBlock = block.gameObject.AddComponent<TowerBlock>();
            Debug.LogWarning($"⚠️ {block.gameObject.name}에 TowerBlock 컴포넌트가 없습니다.");
            return;
        }

        // 타워 활성화
        towerBlock.ActivateTower();

        // 딕셔너리에 추가
        if (!activeTowerBlocks.ContainsKey(block))
        {
            activeTowerBlocks.Add(block, towerBlock);
        }

        Debug.Log($"🏰 {block.gameObject.name} 타워 배치 완료 (타입: {towerBlock.GetTowerType()})");
    }

    /// <summary>
    /// 블록이 그리드에서 제거될 때 호출
    /// </summary>
    public void OnBlockRemoved(Block block)
    {
        if (block == null)
            return;

        // 딕셔너리에서 제거
        if (activeTowerBlocks.TryGetValue(block, out TowerBlock towerBlock))
        {
            towerBlock.DeactivateTower();
            activeTowerBlocks.Remove(block);
            Debug.Log($"🚫 {block.gameObject.name} 타워 제거됨");
        }
    }

    /// <summary>
    /// 씬에 있는 모든 배치된 블록을 스캔하여 타워 활성화
    /// </summary>
    public void ScanAndActivateAllTowers()
    {
        Block[] allBlocks = FindObjectsOfType<Block>();
        int activatedCount = 0;

        foreach (Block block in allBlocks)
        {
            if (block.isPlacedOnGrid)
            {
                TowerBlock towerBlock = block.GetComponent<TowerBlock>();

                if (towerBlock != null && !towerBlock.IsTowerActive())
                {
                    towerBlock.ActivateTower();

                    if (!activeTowerBlocks.ContainsKey(block))
                    {
                        activeTowerBlocks.Add(block, towerBlock);
                    }

                    activatedCount++;
                }
            }
        }

        Debug.Log($"✅ {activatedCount}개의 타워 활성화 완료! (총 {activeTowerBlocks.Count}개 타워 활성 중)");
    }

    /// <summary>
    /// 모든 타워 비활성화
    /// </summary>
    public void DeactivateAllTowers()
    {
        foreach (var kvp in activeTowerBlocks)
        {
            if (kvp.Value != null)
            {
                kvp.Value.DeactivateTower();
            }
        }

        activeTowerBlocks.Clear();
        Debug.Log("🛑 모든 타워 비활성화됨");
    }

    /// <summary>
    /// 특정 블록의 타워 타입 변경
    /// </summary>
    public void ChangeTowerType(Block block, TowerBlock.TowerType newType)
    {
        if (block == null)
            return;

        TowerBlock towerBlock = block.GetComponent<TowerBlock>();

        if (towerBlock != null)
        {
            towerBlock.SetTowerType(newType);
            Debug.Log($"🔄 {block.gameObject.name}의 타워 타입을 {newType}으로 변경");
        }
    }

    /// <summary>
    /// 현재 활성화된 타워 개수 반환
    /// </summary>
    public int GetActiveTowerCount()
    {
        return activeTowerBlocks.Count;
    }

    /// <summary>
    /// 타워 타입별 개수 반환
    /// </summary>
    public Dictionary<TowerBlock.TowerType, int> GetTowerCountByType()
    {
        Dictionary<TowerBlock.TowerType, int> counts = new Dictionary<TowerBlock.TowerType, int>();

        foreach (var kvp in activeTowerBlocks)
        {
            if (kvp.Value != null)
            {
                TowerBlock.TowerType type = kvp.Value.GetTowerType();

                if (counts.ContainsKey(type))
                {
                    counts[type]++;
                }
                else
                {
                    counts[type] = 1;
                }
            }
        }

        return counts;
    }

    /// <summary>
    /// 특정 타입의 타워 개수 반환
    /// </summary>
    public int GetTowerCountByType(TowerBlock.TowerType type)
    {
        int count = 0;

        foreach (var kvp in activeTowerBlocks)
        {
            if (kvp.Value != null && kvp.Value.GetTowerType() == type)
            {
                count++;
            }
        }

        return count;
    }

    /// <summary>
    /// 모든 타워 정보 출력 (디버그용)
    /// </summary>
    public void PrintTowerInfo()
    {
        Debug.Log($"📊 === 타워 정보 ===");
        Debug.Log($"총 타워 개수: {activeTowerBlocks.Count}");

        Dictionary<TowerBlock.TowerType, int> typeCounts = GetTowerCountByType();

        foreach (var kvp in typeCounts)
        {
            Debug.Log($"  - {kvp.Key}: {kvp.Value}개");
        }
    }

    /// <summary>
    /// OnGUI로 타워 상태 표시
    /// </summary>
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, 100, 300, 200));
        GUILayout.Label("=== Tower System ===");
        GUILayout.Label($"Active Towers: {activeTowerBlocks.Count}");

        Dictionary<TowerBlock.TowerType, int> typeCounts = GetTowerCountByType();
        foreach (var kvp in typeCounts)
        {
            GUILayout.Label($"  {kvp.Key}: {kvp.Value}");
        }

        GUILayout.Space(10);
        GUILayout.Label("Controls:");
        GUILayout.Label("  T: Activate All Towers");
        GUILayout.Label("  Y: Deactivate All Towers");

        GUILayout.EndArea();
    }

    void OnDestroy()
    {
        // 모든 타워 비활성화
        DeactivateAllTowers();
    }
}
