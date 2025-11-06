using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임의 모든 OnGUI UI를 통합 관리하는 매니저
/// H키로 UI 표시/숨김 토글
/// </summary>
public class GameUIManager : MonoBehaviour
{
    public static GameUIManager Instance { get; private set; }

    [Header("UI Toggle Settings")]
    [SerializeField] private KeyCode toggleKey = KeyCode.H;
    [SerializeField] private bool showUI = true;

    [Header("UI Position Settings")]
    [SerializeField] private int padding = 10;
    [SerializeField] private int panelWidth = 300;

    [Header("References (Auto-Find)")]
    [SerializeField] private GameManager gameManager;
    [SerializeField] private BlockTowerManager blockTowerManager;
    [SerializeField] private NewEnemySpawner enemySpawner;
    [SerializeField] private TowerSpawner towerSpawner;
    [SerializeField] private CameraController cameraController;

    private GUIStyle headerStyle;
    private GUIStyle normalStyle;
    private bool stylesInitialized = false;

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
            return;
        }
    }

    void Start()
    {
        // 참조 자동 찾기
        FindReferences();
    }

    void FindReferences()
    {
        if (gameManager == null)
            gameManager = FindObjectOfType<GameManager>();

        if (blockTowerManager == null)
            blockTowerManager = FindObjectOfType<BlockTowerManager>();

        if (enemySpawner == null)
            enemySpawner = FindObjectOfType<NewEnemySpawner>();

        if (towerSpawner == null)
            towerSpawner = FindObjectOfType<TowerSpawner>();

        if (cameraController == null)
            cameraController = FindObjectOfType<CameraController>();

        Debug.Log($"✅ GameUIManager: References found - GM:{gameManager != null}, Tower:{blockTowerManager != null}, Enemy:{enemySpawner != null}, Spawner:{towerSpawner != null}, Cam:{cameraController != null}");
    }

    void Update()
    {
        // H키로 UI 토글
        if (Input.GetKeyDown(toggleKey))
        {
            showUI = !showUI;
            Debug.Log($"🔄 UI Display: {(showUI ? "ON" : "OFF")}");
        }
    }

    /// <summary>
    /// KeyBindingManager에서 키 바인딩 정보를 가져와서 UI 문자열 생성
    /// </summary>
    string GetKeyBindingString(KeyCode key, string description)
    {
        return $"  [{key}]: {description}";
    }

    void InitializeStyles()
    {
        if (stylesInitialized) return;

        headerStyle = new GUIStyle();
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.yellow;

        normalStyle = new GUIStyle();
        normalStyle.fontSize = 12;
        normalStyle.normal.textColor = Color.white;

        stylesInitialized = true;
    }

    void OnGUI()
    {
        InitializeStyles();

        // H키 안내는 항상 표시
        DrawToggleHint();

        // UI가 꺼져있으면 여기서 종료
        if (!showUI) return;

        // 각 패널 그리기
        DrawEnemySpawnerPanel();
        DrawTowerSystemPanel();
        DrawGameManagerPanel();
        DrawCameraControllerPanel();
        DrawTowerSpawnerPanel();
        DrawControlsPanel();
    }

    /// <summary>
    /// H키 안내 (항상 표시)
    /// </summary>
    void DrawToggleHint()
    {
        GUILayout.BeginArea(new Rect(padding, Screen.height - 30, 300, 30));
        GUILayout.Label($"Press [{toggleKey}] to toggle UI hints", normalStyle);
        GUILayout.EndArea();
    }

    /// <summary>
    /// 적 스폰 정보 (좌측 상단)
    /// </summary>
    void DrawEnemySpawnerPanel()
    {
        if (enemySpawner == null) return;

        int x = padding;
        int y = padding;

        GUILayout.BeginArea(new Rect(x, y, panelWidth, 80));
        GUILayout.Label("=== Enemy Spawner ===", headerStyle);
        GUILayout.Label($"Active Enemies: {enemySpawner.GetActiveEnemyCount()}", normalStyle);
        GUILayout.Label($"Pool Available: {enemySpawner.GetPoolCount()}", normalStyle);
        GUILayout.Label($"Spawning: {(enemySpawner != null ? "Active" : "Stopped")}", normalStyle);
        GUILayout.EndArea();
    }

    /// <summary>
    /// 타워 시스템 정보 (좌측 중앙)
    /// </summary>
    void DrawTowerSystemPanel()
    {
        if (blockTowerManager == null) return;

        int x = padding;
        int y = 100;

        GUILayout.BeginArea(new Rect(x, y, panelWidth, 200));
        GUILayout.Label("=== Tower System ===", headerStyle);
        GUILayout.Label($"Active Towers: {blockTowerManager.GetActiveTowerCount()}", normalStyle);

        Dictionary<TowerBlock.TowerType, int> typeCounts = blockTowerManager.GetTowerCountByType();
        foreach (var kvp in typeCounts)
        {
            GUILayout.Label($"  {kvp.Key}: {kvp.Value}", normalStyle);
        }

        GUILayout.Space(5);
        GUILayout.Label("  T: Activate All Towers", normalStyle);
        GUILayout.Label("  Y: Deactivate All Towers", normalStyle);
        GUILayout.EndArea();
    }

    /// <summary>
    /// 게임 매니저 정보 (좌측 하단)
    /// </summary>
    void DrawGameManagerPanel()
    {
        if (gameManager == null || gameManager.player == null) return;

        int x = padding;
        int y = 310;

        GUILayout.BeginArea(new Rect(x, y, panelWidth, 150));
        GUILayout.Label("=== Game Manager ===", headerStyle);
        GUILayout.Label($"Money: {gameManager.player.GetMoney()}", normalStyle);
        GUILayout.Label($"Lives: {gameManager.player.GetLives()}", normalStyle);
        GUILayout.Label($"Score: {gameManager.player.GetScore()}", normalStyle);
        GUILayout.Label($"Game Started: {gameManager.isGameStarted}", normalStyle);
        GUILayout.Label($"Game Over: {gameManager.isGameOver}", normalStyle);
        GUILayout.Label($"Paused: {gameManager.isPaused}", normalStyle);
        GUILayout.EndArea();
    }

    /// <summary>
    /// 카메라 컨트롤러 정보 (우측 상단)
    /// </summary>
    void DrawCameraControllerPanel()
    {
        if (cameraController == null) return;

        int x = Screen.width - panelWidth - padding;
        int y = padding;

        Camera cam = cameraController.GetComponent<Camera>();
        if (cam == null) cam = Camera.main;

        GUILayout.BeginArea(new Rect(x, y, panelWidth, 200));
        GUILayout.Label("=== Camera Controller ===", headerStyle);
        GUILayout.Label($"Position: ({cameraController.transform.position.x:F1}, {cameraController.transform.position.y:F1})", normalStyle);

        if (cam != null)
        {
            if (cam.orthographic)
            {
                GUILayout.Label($"Zoom (Ortho Size): {cam.orthographicSize:F1}", normalStyle);
            }
            else
            {
                GUILayout.Label($"Zoom (Z Pos): {cameraController.transform.position.z:F1}", normalStyle);
            }
        }

        GUILayout.Space(5);
        GUILayout.Label("Controls:", normalStyle);
        GUILayout.Label("  Move: WASD or Arrow Keys", normalStyle);
        GUILayout.Label("  Edge Scroll: Move mouse to edges", normalStyle);
        GUILayout.Label("  Drag: Middle Mouse Button", normalStyle);
        GUILayout.Label("  Zoom: Mouse Wheel", normalStyle);
        GUILayout.EndArea();
    }

    /// <summary>
    /// 타워 스포너 정보 (우측 중앙)
    /// </summary>
    void DrawTowerSpawnerPanel()
    {
        if (towerSpawner == null) return;

        int x = Screen.width - panelWidth - padding;
        int y = 220;

        GUILayout.BeginArea(new Rect(x, y, panelWidth, 150));
        GUILayout.Label("=== Tower Spawner ===", headerStyle);
        GUILayout.Label($"Spawned Towers: {towerSpawner.GetSpawnedTowerCount()}", normalStyle);
        GUILayout.Label($"Available Tower Types: {towerSpawner.GetTowerDataCount()}", normalStyle);
        GUILayout.Space(5);
        GUILayout.Label("Controls:", normalStyle);
        GUILayout.Label("  0: Spawn 3 Random Towers", normalStyle);
        GUILayout.Label("  C: Clear All Spawned Towers", normalStyle);
        GUILayout.EndArea();
    }

    /// <summary>
    /// 전체 컨트롤 안내 (우측 하단) - KeyBindingManager 연동
    /// </summary>
    void DrawControlsPanel()
    {
        int x = Screen.width - panelWidth - padding;
        int y = 380;

        GUILayout.BeginArea(new Rect(x, y, panelWidth, 300));
        GUILayout.Label("=== Game Controls ===", headerStyle);

        // KeyBindingManager가 있으면 자동으로 키 바인딩 표시
        if (KeyBindingManager.Instance != null)
        {
            // 블록 조작
            GUILayout.Label("Block Controls:", normalStyle);
            GUILayout.Label(GetKeyBindingString(
                KeyBindingManager.Instance.RotateBlockKey,
                KeyBindingManager.Instance.rotateBlockDescription), normalStyle);

            GUILayout.Space(5);

            // 타워
            GUILayout.Label("Tower Controls:", normalStyle);
            GUILayout.Label(GetKeyBindingString(
                KeyBindingManager.Instance.SpawnTowerKey,
                KeyBindingManager.Instance.spawnTowerDescription), normalStyle);

            GUILayout.Space(5);

            // 아이템 타워
            GUILayout.Label("Item Tower Controls:", normalStyle);
            GUILayout.Label(GetKeyBindingString(
                KeyBindingManager.Instance.SpawnItemTowerKey,
                KeyBindingManager.Instance.spawnItemTowerDescription), normalStyle);
            GUILayout.Label(GetKeyBindingString(
                KeyBindingManager.Instance.ClearItemTowerKey,
                KeyBindingManager.Instance.clearItemTowerDescription), normalStyle);

            GUILayout.Space(5);

            // 그리드 & 몬스터
            GUILayout.Label("Grid & Monster Controls:", normalStyle);
            GUILayout.Label(GetKeyBindingString(
                KeyBindingManager.Instance.ShowExpandableCellsKey,
                KeyBindingManager.Instance.showExpandableCellsDescription), normalStyle);
            GUILayout.Label(GetKeyBindingString(
                KeyBindingManager.Instance.ShowMonsterPathKey,
                KeyBindingManager.Instance.showMonsterPathDescription), normalStyle);
            GUILayout.Label(GetKeyBindingString(
                KeyBindingManager.Instance.ToggleMonsterSpawnKey,
                KeyBindingManager.Instance.toggleMonsterSpawnDescription), normalStyle);
        }
        else
        {
            // KeyBindingManager가 없으면 기본 안내
            GUILayout.Label("Block Controls:", normalStyle);
            GUILayout.Label("  [R]: Rotate Block", normalStyle);
            GUILayout.Label("Tower Controls:", normalStyle);
            GUILayout.Label("  [T]: Spawn Tower", normalStyle);
            GUILayout.Label("Item Tower Controls:", normalStyle);
            GUILayout.Label("  [-]: Spawn Item Towers", normalStyle);
            GUILayout.Label("  [=]: Clear Item Towers", normalStyle);
        }

        GUILayout.EndArea();
    }

    /// <summary>
    /// UI 표시 여부 확인 (다른 스크립트에서 사용)
    /// </summary>
    public static bool ShouldShowUI()
    {
        return Instance != null && Instance.showUI;
    }

    /// <summary>
    /// UI 강제 토글
    /// </summary>
    public void ToggleUI()
    {
        showUI = !showUI;
    }

    /// <summary>
    /// UI 표시 설정
    /// </summary>
    public void SetUIVisibility(bool visible)
    {
        showUI = visible;
    }
}
