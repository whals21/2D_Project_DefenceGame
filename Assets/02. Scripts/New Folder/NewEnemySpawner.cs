using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 새로운 적 생성 시스템 - GridMap 및 MonsterPathManager 기반
/// 오브젝트 풀링을 사용하여 적을 효율적으로 생성/재사용
/// </summary>
public class NewEnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 20;
    [SerializeField] private float spawnInterval = 2f;

    [Header("References")]
    [SerializeField] private NewPathFinder pathFinder;

    [Header("Spawn Position")]
    [SerializeField] private Vector2Int spawnGridPosition; // 스폰할 그리드 좌표 (경로상의 특정 위치)
    [SerializeField] private bool useFirstWaypointAsSpawn = true; // true면 경로의 첫 지점에서 스폰

    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;

    void Start()
    {
        // PathFinder 자동 탐색
        if (pathFinder == null)
        {
            pathFinder = FindObjectOfType<NewPathFinder>();
            if (pathFinder == null)
            {
                Debug.LogError("❌ NewPathFinder not found! Please add NewPathFinder to the scene.");
                return;
            }
        }

        InitializePool();
    }

    /// <summary>
    /// 오브젝트 풀 초기화
    /// </summary>
    void InitializePool()
    {
        if (enemyPrefab == null)
        {
            Debug.LogError("❌ Enemy prefab is not assigned!");
            return;
        }

        for (int i = 0; i < poolSize; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab);
            enemy.name = $"Enemy_{i}";
            enemy.SetActive(false);
            enemy.transform.SetParent(transform);

            // NewEnemy 컴포넌트 초기화
            NewEnemy enemyScript = enemy.GetComponent<NewEnemy>();
            if (enemyScript != null)
            {
                enemyScript.SetSpawner(this);
            }

            enemyPool.Enqueue(enemy);
        }

        Debug.Log($"✅ Enemy pool initialized with {poolSize} enemies");
    }

    /// <summary>
    /// 적 생성 시작
    /// </summary>
    public void StartSpawning()
    {
        if (!isSpawning)
        {
            isSpawning = true;
            StartCoroutine(SpawnRoutine());
            Debug.Log("▶️ Enemy spawning started");
        }
    }

    /// <summary>
    /// 적 생성 중지
    /// </summary>
    public void StopSpawning()
    {
        isSpawning = false;
        Debug.Log("⏸️ Enemy spawning stopped");
    }

    /// <summary>
    /// 스폰 상태 토글
    /// </summary>
    public void ToggleSpawning()
    {
        if (isSpawning)
            StopSpawning();
        else
            StartSpawning();
    }

    /// <summary>
    /// 적 생성 루틴 (코루틴)
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            // 경로 확인
            List<Vector2Int> path = pathFinder.GetPath();

            // 경로가 있고 풀에 사용 가능한 적이 있을 때만 생성
            if (path != null && path.Count > 0 && enemyPool.Count > 0)
            {
                SpawnEnemy();
            }
            else if (path == null || path.Count == 0)
            {
                Debug.LogWarning("⚠️ No path available for spawning!");
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 적 1마리 생성
    /// </summary>
    void SpawnEnemy()
    {
        if (enemyPool.Count == 0)
        {
            Debug.LogWarning("⚠️ Enemy pool is empty! Consider increasing pool size.");
            return;
        }

        List<Vector2Int> path = pathFinder.GetPath();
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("⚠️ No path available for enemy!");
            return;
        }

        // 풀에서 적 가져오기
        GameObject enemy = enemyPool.Dequeue();
        enemy.SetActive(true);

        // 스폰 위치 결정
        Vector2Int spawnPos;
        if (useFirstWaypointAsSpawn)
        {
            spawnPos = path[0]; // 경로의 첫 번째 지점
        }
        else
        {
            spawnPos = spawnGridPosition; // 수동 설정한 그리드 좌표
        }

        // 월드 좌표로 변환하여 배치
        Vector3 spawnWorldPos = new Vector3(spawnPos.x, spawnPos.y, 0);
        enemy.transform.position = spawnWorldPos;

        // NewEnemy 컴포넌트에 경로 설정
        NewEnemy enemyScript = enemy.GetComponent<NewEnemy>();
        if (enemyScript != null)
        {
            enemyScript.SetPath(path);
            enemyScript.ResetEnemy();
        }

        activeEnemies.Add(enemy);
        Debug.Log($"✅ Spawned {enemy.name} at grid position {spawnPos} (world: {spawnWorldPos})");
    }

    /// <summary>
    /// 특정 위치에 적 즉시 생성 (테스트용)
    /// </summary>
    public void SpawnEnemyAtPosition(Vector2Int gridPos)
    {
        if (enemyPool.Count == 0)
        {
            Debug.LogWarning("⚠️ Enemy pool is empty!");
            return;
        }

        List<Vector2Int> path = pathFinder.GetPath();
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("⚠️ No path available!");
            return;
        }

        GameObject enemy = enemyPool.Dequeue();
        enemy.SetActive(true);

        Vector3 worldPos = new Vector3(gridPos.x, gridPos.y, 0);
        enemy.transform.position = worldPos;

        NewEnemy enemyScript = enemy.GetComponent<NewEnemy>();
        if (enemyScript != null)
        {
            enemyScript.SetPath(path);
            enemyScript.ResetEnemy();
        }

        activeEnemies.Add(enemy);
        Debug.Log($"✅ Manually spawned {enemy.name} at {gridPos}");
    }

    /// <summary>
    /// 적을 풀로 반환
    /// </summary>
    public void ReturnToPool(GameObject enemy)
    {
        if (enemy == null) return;

        enemy.SetActive(false);
        activeEnemies.Remove(enemy);
        enemyPool.Enqueue(enemy);

        Debug.Log($"♻️ {enemy.name} returned to pool");
    }

    /// <summary>
    /// 모든 활성 적 제거
    /// </summary>
    public void ClearAllEnemies()
    {
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            ReturnToPool(activeEnemies[i]);
        }

        Debug.Log("🧹 Cleared all active enemies");
    }

    /// <summary>
    /// 현재 활성 적 수 반환
    /// </summary>
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    /// <summary>
    /// 풀에 남은 적 수 반환
    /// </summary>
    public int GetPoolCount()
    {
        return enemyPool.Count;
    }

    /// <summary>
    /// 스폰 간격 변경
    /// </summary>
    public void SetSpawnInterval(float interval)
    {
        spawnInterval = Mathf.Max(0.1f, interval); // 최소 0.1초
        Debug.Log($"⏱️ Spawn interval set to {spawnInterval}s");
    }

    void Update()
    {
        // 테스트용 키보드 단축키
        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleSpawning();
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            SpawnEnemy();
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            ClearAllEnemies();
        }
    }

    /// <summary>
    /// Inspector에서 상태 확인용
    /// </summary>
    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 300, 20), $"Active Enemies: {activeEnemies.Count}");
        GUI.Label(new Rect(10, 30, 300, 20), $"Pool Available: {enemyPool.Count}");
        GUI.Label(new Rect(10, 50, 300, 20), $"Spawning: {isSpawning}");
    }
}
