using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 몬스터 경로 관리 시스템
/// 그리드 외곽을 둘러싼 고스트 셀을 생성하여 몬스터가 순회할 경로를 정의
/// ShowExpandableCells()와 달리 대각선 모서리 부분도 포함
/// 고스트 셀 생성 시 자동으로 경로 재계산 및 적 스폰 시작
/// </summary>
public class MonsterPathManager : MonoBehaviour
{
    [Header("Grid References")]
    public GridMapManager gridMapManager;
    public GameObject monsterPathCellPrefab; // 몬스터 경로용 고스트 셀 프리팹

    [Header("Enemy System References")]
    public NewPathFinder pathFinder; // 경로 계산 시스템
    public NewEnemySpawner enemySpawner; // 적 생성 시스템

    private List<GameObject> pathCells = new List<GameObject>();
    private List<Vector2Int> pathPositions = new List<Vector2Int>(); // 경로 위치 순서대로 저장

    void Start()
    {
        if (gridMapManager == null)
        {
            gridMapManager = FindObjectOfType<GridMapManager>();
        }

        if (gridMapManager == null)
        {
            Debug.LogError("MonsterPathManager: GridMapManager를 찾을 수 없습니다.");
        }

        // PathFinder와 EnemySpawner 자동 탐색
        if (pathFinder == null)
        {
            pathFinder = FindObjectOfType<NewPathFinder>();
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindObjectOfType<NewEnemySpawner>();
        }
    }

    /// <summary>
    /// 몬스터 경로 고스트 셀 표시
    /// 그리드 외곽을 시계방향으로 순회하는 경로 생성 (대각선 모서리 포함)
    /// 고스트 셀 생성 시 자동으로 경로 재계산 및 적 스폰 시작
    /// </summary>
    public void ShowMonsterPath()
    {
        // ✨ NEW: 토글 기능 - 경로 셀이 이미 있으면 제거하고 적 스폰 중지
        if (pathCells.Count > 0)
        {
            ClearPathCells();

            // 적 스폰 중지
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
                enemySpawner.ClearAllEnemies();
                Debug.Log("🛑 몬스터 경로 제거 - 적 스폰 중지 및 모든 적 제거");
            }

            return;
        }

        // 기존 경로 셀 제거 (혹시 모를 경우 대비)
        ClearPathCells();

        if (gridMapManager == null)
        {
            Debug.LogError("MonsterPathManager: GridMapManager가 설정되지 않았습니다.");
            return;
        }

        GridMap gridMap = gridMapManager.GetGridMap();
        if (gridMap == null)
        {
            Debug.LogError("MonsterPathManager: GridMap을 가져올 수 없습니다.");
            return;
        }

        // ✨ FIX: GetExpandablePositions()와 동일한 방식으로 외곽 셀 찾기
        List<Vector2Int> perimeterPositions = GetMonsterPathPositions(gridMap);

        // 고스트 셀 생성
        foreach (Vector2Int pos in perimeterPositions)
        {
            CreatePathCell(pos);
        }

        // 경로 위치 저장 (순서대로)
        pathPositions = new List<Vector2Int>(perimeterPositions);

        Debug.Log($"✅ 몬스터 경로 생성 완료: {pathPositions.Count}개의 경로 포인트");

        // ✨ NEW: 경로 생성 후 자동으로 PathFinder 경로 재계산
        if (pathFinder != null)
        {
            pathFinder.RecalculatePath();
            Debug.Log("🔄 PathFinder 경로 재계산 완료");
        }
        else
        {
            Debug.LogWarning("⚠️ PathFinder가 설정되지 않아 경로 재계산을 건너뜁니다.");
        }

        // ✨ NEW: 경로 생성 후 자동으로 적 스폰 시작
        if (enemySpawner != null)
        {
            enemySpawner.StartSpawning();
            Debug.Log("▶️ 적 스폰 시작");
        }
        else
        {
            Debug.LogWarning("⚠️ EnemySpawner가 설정되지 않아 적 스폰을 건너뜁니다.");
        }
    }

    /// <summary>
    /// 그리드 외곽 경로 계산 (ShowExpandableCells 방식 + 대각선 모서리 포함)
    /// 실제 그리드 셀의 인접 위치만 찾아서 경로 생성
    /// </summary>
    List<Vector2Int> GetMonsterPathPositions(GridMap gridMap)
    {
        HashSet<Vector2Int> existingCells = new HashSet<Vector2Int>(gridMap.cells.Keys);
        HashSet<Vector2Int> pathPositions = new HashSet<Vector2Int>();

        // 8방향 (상하좌우 + 대각선 4방향)
        Vector2Int[] directions = new Vector2Int[]
        {
            new Vector2Int(0, 1),   // 위
            new Vector2Int(0, -1),  // 아래
            new Vector2Int(-1, 0),  // 왼쪽
            new Vector2Int(1, 0),   // 오른쪽
            new Vector2Int(-1, 1),  // 좌상단 대각선
            new Vector2Int(1, 1),   // 우상단 대각선
            new Vector2Int(-1, -1), // 좌하단 대각선
            new Vector2Int(1, -1)   // 우하단 대각선
        };

        // 모든 기존 셀의 주변(8방향)을 확인
        foreach (Vector2Int cellPos in existingCells)
        {
            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = cellPos + dir;

                // 인접한 위치에 셀이 없으면 경로 위치로 추가
                if (!existingCells.Contains(neighborPos))
                {
                    pathPositions.Add(neighborPos);
                }
            }
        }

        // HashSet을 List로 변환하여 반환
        return new List<Vector2Int>(pathPositions);
    }

    /// <summary>
    /// 경로 셀 GameObject 생성
    /// </summary>
    void CreatePathCell(Vector2Int pos)
    {
        if (monsterPathCellPrefab == null)
        {
            Debug.LogWarning("MonsterPathManager: monsterPathCellPrefab이 설정되지 않았습니다.");
            return;
        }

        Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
        GameObject pathCell = Instantiate(monsterPathCellPrefab, worldPos, Quaternion.identity, transform);

        // 경로 셀임을 표시하기 위한 시각적 차별화 (예: 색상 변경)
        SpriteRenderer spriteRenderer = pathCell.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = new Color(1f, 0.5f, 0f, 0.5f); // 주황색 반투명
        }

        pathCells.Add(pathCell);
    }

    /// <summary>
    /// 모든 경로 셀 제거
    /// </summary>
    void ClearPathCells()
    {
        foreach (GameObject pathCell in pathCells)
        {
            if (pathCell != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(pathCell);
                }
                else
                {
                    DestroyImmediate(pathCell);
                }
            }
        }
        pathCells.Clear();
        pathPositions.Clear();
    }

    /// <summary>
    /// 경로 위치 리스트 반환 (순서대로)
    /// </summary>
    public List<Vector2Int> GetPathPositions()
    {
        return new List<Vector2Int>(pathPositions);
    }

    /// <summary>
    /// 경로가 생성되어 있는지 확인
    /// </summary>
    public bool HasPath()
    {
        return pathPositions.Count > 0;
    }

    /// <summary>
    /// Gizmo로 경로 시각화 (Scene 뷰에서만 보임)
    /// </summary>
    void OnDrawGizmos()
    {
        if (pathPositions.Count > 0)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.8f); // 주황색

            // 경로 라인 그리기
            for (int i = 0; i < pathPositions.Count - 1; i++)
            {
                Vector3 start = new Vector3(pathPositions[i].x, pathPositions[i].y, 0);
                Vector3 end = new Vector3(pathPositions[i + 1].x, pathPositions[i + 1].y, 0);
                Gizmos.DrawLine(start, end);
            }

            // 마지막 → 첫 번째 연결 (순환 경로)
            if (pathPositions.Count > 1)
            {
                Vector3 lastPos = new Vector3(pathPositions[pathPositions.Count - 1].x, pathPositions[pathPositions.Count - 1].y, 0);
                Vector3 firstPos = new Vector3(pathPositions[0].x, pathPositions[0].y, 0);
                Gizmos.DrawLine(lastPos, firstPos);
            }
        }
    }

    void OnDestroy()
    {
        ClearPathCells();
    }
}
