using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내 모든 키 바인딩을 중앙에서 관리하는 싱글톤 매니저
/// Unity Inspector에서 키를 자유롭게 설정 가능
/// </summary>
public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance { get; private set; }

    [Header("=== 블록 조작 키 ===")]
    [SerializeField] private KeyCode rotateBlockKey = KeyCode.R;
    [Tooltip("블록을 시계방향으로 90도 회전")]
    public string rotateBlockDescription = "블록 회전";

    [Header("=== 타워 스폰 키 ===")]
    [SerializeField] private KeyCode spawnTowerKey = KeyCode.T;
    [Tooltip("랜덤 타워 스폰 (테스트용)")]
    public string spawnTowerDescription = "타워 스폰";

    [Header("=== 아이템 타워 키 ===")]
    [SerializeField] private KeyCode spawnItemTowerKey = KeyCode.Minus;
    [Tooltip("아이템 타워 3개 스폰")]
    public string spawnItemTowerDescription = "아이템 타워 스폰";

    [SerializeField] private KeyCode clearItemTowerKey = KeyCode.Equals;
    [Tooltip("모든 아이템 타워 제거")]
    public string clearItemTowerDescription = "아이템 타워 제거";

    [Header("=== 그리드 조작 키 ===")]
    [SerializeField] private KeyCode showExpandableCellsKey = KeyCode.E;
    [Tooltip("확장 가능한 셀 표시/숨김")]
    public string showExpandableCellsDescription = "확장 가능 셀 토글";

    [SerializeField] private KeyCode showMonsterPathKey = KeyCode.P;
    [Tooltip("몬스터 경로 표시/숨김 및 스폰 토글")]
    public string showMonsterPathDescription = "몬스터 경로 토글";

    [Header("=== 몬스터 스폰 키 ===")]
    [SerializeField] private KeyCode toggleMonsterSpawnKey = KeyCode.M;
    [Tooltip("몬스터 스폰 시작/중지")]
    public string toggleMonsterSpawnDescription = "몬스터 스폰 토글";

    [Header("=== 테스트 키 ===")]
    [SerializeField] private KeyCode testPathfindingKey = KeyCode.F;
    [Tooltip("경로 찾기 테스트")]
    public string testPathfindingDescription = "경로 찾기 테스트";

    [SerializeField] private KeyCode clearAllKey = KeyCode.C;
    [Tooltip("모든 오브젝트 제거 (Reset)")]
    public string clearAllDescription = "모두 제거";

    void Awake()
    {
        // 싱글톤 패턴
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // ========== 블록 조작 ==========
    public KeyCode RotateBlockKey => rotateBlockKey;
    public bool GetRotateBlockKeyDown() => Input.GetKeyDown(rotateBlockKey);

    // ========== 타워 스폰 ==========
    public KeyCode SpawnTowerKey => spawnTowerKey;
    public bool GetSpawnTowerKeyDown() => Input.GetKeyDown(spawnTowerKey);

    // ========== 아이템 타워 ==========
    public KeyCode SpawnItemTowerKey => spawnItemTowerKey;
    public bool GetSpawnItemTowerKeyDown() => Input.GetKeyDown(spawnItemTowerKey);

    public KeyCode ClearItemTowerKey => clearItemTowerKey;
    public bool GetClearItemTowerKeyDown() => Input.GetKeyDown(clearItemTowerKey);

    // ========== 그리드 조작 ==========
    public KeyCode ShowExpandableCellsKey => showExpandableCellsKey;
    public bool GetShowExpandableCellsKeyDown() => Input.GetKeyDown(showExpandableCellsKey);

    public KeyCode ShowMonsterPathKey => showMonsterPathKey;
    public bool GetShowMonsterPathKeyDown() => Input.GetKeyDown(showMonsterPathKey);

    // ========== 몬스터 스폰 ==========
    public KeyCode ToggleMonsterSpawnKey => toggleMonsterSpawnKey;
    public bool GetToggleMonsterSpawnKeyDown() => Input.GetKeyDown(toggleMonsterSpawnKey);

    // ========== 테스트 ==========
    public KeyCode TestPathfindingKey => testPathfindingKey;
    public bool GetTestPathfindingKeyDown() => Input.GetKeyDown(testPathfindingKey);

    public KeyCode ClearAllKey => clearAllKey;
    public bool GetClearAllKeyDown() => Input.GetKeyDown(clearAllKey);

    /// <summary>
    /// 모든 키 바인딩 정보를 리스트로 반환 (UI 표시용)
    /// </summary>
    public List<KeyBindingInfo> GetAllKeyBindings()
    {
        List<KeyBindingInfo> bindings = new List<KeyBindingInfo>
        {
            // 블록 조작
            new KeyBindingInfo("블록 조작", rotateBlockKey, rotateBlockDescription),

            // 타워
            new KeyBindingInfo("타워", spawnTowerKey, spawnTowerDescription),

            // 아이템 타워
            new KeyBindingInfo("아이템 타워", spawnItemTowerKey, spawnItemTowerDescription),
            new KeyBindingInfo("아이템 타워", clearItemTowerKey, clearItemTowerDescription),

            // 그리드
            new KeyBindingInfo("그리드", showExpandableCellsKey, showExpandableCellsDescription),
            new KeyBindingInfo("몬스터", showMonsterPathKey, showMonsterPathDescription),

            // 몬스터 스폰
            new KeyBindingInfo("몬스터", toggleMonsterSpawnKey, toggleMonsterSpawnDescription),

            // 테스트
            new KeyBindingInfo("테스트", testPathfindingKey, testPathfindingDescription),
            new KeyBindingInfo("테스트", clearAllKey, clearAllDescription)
        };

        return bindings;
    }

    /// <summary>
    /// 특정 카테고리의 키 바인딩만 반환
    /// </summary>
    public List<KeyBindingInfo> GetKeyBindingsByCategory(string category)
    {
        List<KeyBindingInfo> allBindings = GetAllKeyBindings();
        return allBindings.FindAll(binding => binding.category == category);
    }
}

/// <summary>
/// 키 바인딩 정보를 담는 데이터 클래스
/// </summary>
[System.Serializable]
public class KeyBindingInfo
{
    public string category;      // 카테고리 (예: "블록 조작", "타워", "몬스터")
    public KeyCode key;          // 키 코드
    public string description;   // 설명

    public KeyBindingInfo(string category, KeyCode key, string description)
    {
        this.category = category;
        this.key = key;
        this.description = description;
    }

    /// <summary>
    /// UI 표시용 포맷팅된 문자열 반환
    /// </summary>
    public string GetFormattedString()
    {
        return $"[{key}] {description}";
    }
}
