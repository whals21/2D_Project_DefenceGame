using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 생성 및 오브젝트 풀 관리
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Settings")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private int poolSize = 10;
    [SerializeField] private float spawnInterval = 2f;

    [Header("References")]
    [SerializeField] private PathFinder pathFinder;

    private Queue<GameObject> enemyPool = new Queue<GameObject>();
    private List<GameObject> activeEnemies = new List<GameObject>();
    private bool isSpawning = false;

    void Start()
    {
        if (pathFinder == null)
        {
            pathFinder = FindObjectOfType<PathFinder>();
            if (pathFinder == null)
            {
                Debug.LogError("❌ PathFinder not found! Please add PathFinder to the scene.");
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

            // Enemy 컴포넌트 초기화
            Enemy enemyScript = enemy.GetComponent<Enemy>();
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
    /// 적 생성 루틴
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        while (isSpawning)
        {
            List<Vector3> path = pathFinder.GetPath();

            // 경로가 있고 풀에 사용 가능한 적이 있을 때만 생성
            if (path != null && path.Count > 0 && enemyPool.Count > 0)
            {
                SpawnEnemy();
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
            Debug.LogWarning("⚠️ Enemy pool is empty!");
            return;
        }

        List<Vector3> path = pathFinder.GetPath();
        if (path == null || path.Count == 0)
        {
            Debug.LogWarning("⚠️ No path available for enemy!");
            return;
        }

        // 풀에서 적 가져오기
        GameObject enemy = enemyPool.Dequeue();
        enemy.SetActive(true);

        // 시작 위치로 이동 (PathTile_0_0 또는 첫 번째 웨이포인트)
        enemy.transform.position = path[0];

        // Enemy 컴포넌트에 경로 설정
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.SetPath(path);
            enemyScript.ResetEnemy();
        }

        activeEnemies.Add(enemy);
        Debug.Log($"✅ Spawned {enemy.name} at {path[0]}");
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
    /// 현재 활성 적 수
    /// </summary>
    public int GetActiveEnemyCount()
    {
        return activeEnemies.Count;
    }

    void Update()
    {
        // 테스트용 키보드 단축키
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (isSpawning)
                StopSpawning();
            else
                StartSpawning();
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
}
