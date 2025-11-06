using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 새로운 경로 탐색 시스템 - MonsterPathManager 기반
/// GridMap의 외곽을 순회하는 경로를 자동으로 생성 및 정렬
/// </summary>
public class NewPathFinder : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MonsterPathManager monsterPathManager;
    [SerializeField] private GridMapManager gridMapManager;

    [Header("Path Settings")]
    [SerializeField] private bool autoCalculateOnStart = true;
    [SerializeField] private bool sortClockwise = true; // true: 시계방향, false: 반시계방향

    private List<Vector2Int> pathPositions = new List<Vector2Int>();
    private bool isPathCalculated = false;

    void Start()
    {
        // 자동으로 MonsterPathManager 찾기
        if (monsterPathManager == null)
        {
            monsterPathManager = FindObjectOfType<MonsterPathManager>();
        }

        // 자동으로 GridMapManager 찾기
        if (gridMapManager == null)
        {
            gridMapManager = FindObjectOfType<GridMapManager>();
        }

        if (monsterPathManager == null)
        {
            Debug.LogError("❌ MonsterPathManager not found! Please add MonsterPathManager to the scene.");
        }

        if (gridMapManager == null)
        {
            Debug.LogError("❌ GridMapManager not found! Please add GridMapManager to the scene.");
        }

        // 시작 시 자동 계산
        if (autoCalculateOnStart)
        {
            Invoke("CalculatePath", 0.5f); // 0.5초 후 계산 (GridMap 초기화 대기)
        }
    }

    /// <summary>
    /// 경로 계산 - MonsterPathManager의 경로를 가져와서 정렬
    /// </summary>
    public void CalculatePath()
    {
        pathPositions.Clear();

        if (monsterPathManager == null)
        {
            Debug.LogError("❌ MonsterPathManager is not assigned!");
            return;
        }

        // MonsterPathManager가 경로를 생성했는지 확인
        if (!monsterPathManager.HasPath())
        {
            Debug.LogWarning("⚠️ MonsterPathManager has no path. Calling ShowMonsterPath()...");
            monsterPathManager.ShowMonsterPath();
        }

        // MonsterPathManager로부터 경로 가져오기
        List<Vector2Int> rawPath = monsterPathManager.GetPathPositions();

        if (rawPath == null || rawPath.Count == 0)
        {
            Debug.LogWarning("⚠️ No path positions available from MonsterPathManager!");
            return;
        }

        // 경로 정렬 (시계방향 또는 반시계방향)
        pathPositions = SortPathPositions(rawPath);

        isPathCalculated = true;
        Debug.Log($"✅ Path calculated with {pathPositions.Count} waypoints (sorted {(sortClockwise ? "clockwise" : "counter-clockwise")})");
    }

    /// <summary>
    /// 경로 위치 정렬 - 시계방향 또는 반시계방향으로 순회 경로 생성
    /// </summary>
    List<Vector2Int> SortPathPositions(List<Vector2Int> positions)
    {
        if (positions.Count == 0) return positions;

        // 시작점 찾기: 가장 왼쪽 아래 (또는 원하는 기준점)
        Vector2Int startPos = positions.OrderBy(p => p.x).ThenBy(p => p.y).First();

        List<Vector2Int> sortedPath = new List<Vector2Int>();
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();

        Vector2Int currentPos = startPos;
        sortedPath.Add(currentPos);
        visited.Add(currentPos);

        // 인접한 위치를 찾아가며 경로 생성
        while (sortedPath.Count < positions.Count)
        {
            Vector2Int nextPos = FindNearestUnvisited(currentPos, positions, visited);

            if (nextPos != Vector2Int.zero || positions.Contains(Vector2Int.zero))
            {
                sortedPath.Add(nextPos);
                visited.Add(nextPos);
                currentPos = nextPos;
            }
            else
            {
                // 더 이상 연결된 경로가 없으면 남은 위치 중 가장 가까운 것 선택
                var remaining = positions.Where(p => !visited.Contains(p)).ToList();
                if (remaining.Count > 0)
                {
                    nextPos = remaining.OrderBy(p => Vector2Int.Distance(currentPos, p)).First();
                    sortedPath.Add(nextPos);
                    visited.Add(nextPos);
                    currentPos = nextPos;
                }
                else
                {
                    break;
                }
            }
        }

        return sortedPath;
    }

    /// <summary>
    /// 현재 위치에서 가장 가까운 미방문 위치 찾기 (상하좌우 우선)
    /// </summary>
    Vector2Int FindNearestUnvisited(Vector2Int current, List<Vector2Int> allPositions, HashSet<Vector2Int> visited)
    {
        // 상하좌우 방향 (시계방향이면 우->하->좌->상, 반시계방향이면 좌->하->우->상)
        Vector2Int[] orthogonalDirections = sortClockwise
            ? new Vector2Int[] { new Vector2Int(1, 0), new Vector2Int(0, -1), new Vector2Int(-1, 0), new Vector2Int(0, 1) } // 우, 하, 좌, 상
            : new Vector2Int[] { new Vector2Int(-1, 0), new Vector2Int(0, -1), new Vector2Int(1, 0), new Vector2Int(0, 1) }; // 좌, 하, 우, 상

        // 1. 상하좌우 인접 위치 우선 검색
        foreach (Vector2Int dir in orthogonalDirections)
        {
            Vector2Int neighbor = current + dir;
            if (allPositions.Contains(neighbor) && !visited.Contains(neighbor))
            {
                return neighbor;
            }
        }

        // 2. 대각선 방향 검색
        Vector2Int[] diagonalDirections = new Vector2Int[]
        {
            new Vector2Int(1, 1), new Vector2Int(-1, 1),
            new Vector2Int(-1, -1), new Vector2Int(1, -1)
        };

        foreach (Vector2Int dir in diagonalDirections)
        {
            Vector2Int neighbor = current + dir;
            if (allPositions.Contains(neighbor) && !visited.Contains(neighbor))
            {
                return neighbor;
            }
        }

        // 3. 인접하지 않은 경우 가장 가까운 미방문 위치 반환
        var unvisited = allPositions.Where(p => !visited.Contains(p)).ToList();
        if (unvisited.Count > 0)
        {
            return unvisited.OrderBy(p => Vector2Int.Distance(current, p)).First();
        }

        return Vector2Int.zero; // 미방문 위치 없음
    }

    /// <summary>
    /// 현재 경로 반환
    /// </summary>
    public List<Vector2Int> GetPath()
    {
        if (!isPathCalculated)
        {
            CalculatePath();
        }

        return new List<Vector2Int>(pathPositions);
    }

    /// <summary>
    /// 경로의 월드 좌표 버전 반환
    /// </summary>
    public List<Vector3> GetPathWorldPositions()
    {
        List<Vector3> worldPath = new List<Vector3>();

        foreach (Vector2Int gridPos in pathPositions)
        {
            worldPath.Add(new Vector3(gridPos.x, gridPos.y, 0));
        }

        return worldPath;
    }

    /// <summary>
    /// 경로가 계산되었는지 확인
    /// </summary>
    public bool HasPath()
    {
        return isPathCalculated && pathPositions.Count > 0;
    }

    /// <summary>
    /// 경로 재계산
    /// </summary>
    public void RecalculatePath()
    {
        isPathCalculated = false;
        CalculatePath();
    }

    /// <summary>
    /// 경로 정렬 방향 변경
    /// </summary>
    public void SetClockwise(bool clockwise)
    {
        if (sortClockwise != clockwise)
        {
            sortClockwise = clockwise;
            RecalculatePath();
        }
    }

    /// <summary>
    /// 경로 시각화 (Gizmos)
    /// </summary>
    void OnDrawGizmos()
    {
        if (pathPositions == null || pathPositions.Count < 2) return;

        Gizmos.color = Color.cyan;

        // 경로 라인 그리기
        for (int i = 0; i < pathPositions.Count - 1; i++)
        {
            Vector3 start = new Vector3(pathPositions[i].x, pathPositions[i].y, 0);
            Vector3 end = new Vector3(pathPositions[i + 1].x, pathPositions[i + 1].y, 0);
            Gizmos.DrawLine(start, end);

            // 웨이포인트 번호 표시
            Gizmos.DrawSphere(start, 0.1f);
        }

        // 순환 경로: 마지막 -> 첫 번째 연결
        if (pathPositions.Count > 1)
        {
            Vector3 lastPos = new Vector3(pathPositions[pathPositions.Count - 1].x, pathPositions[pathPositions.Count - 1].y, 0);
            Vector3 firstPos = new Vector3(pathPositions[0].x, pathPositions[0].y, 0);
            Gizmos.DrawLine(lastPos, firstPos);

            // 마지막 웨이포인트
            Gizmos.DrawSphere(lastPos, 0.1f);
        }

        // 시작점 강조
        if (pathPositions.Count > 0)
        {
            Gizmos.color = Color.green;
            Vector3 startPos = new Vector3(pathPositions[0].x, pathPositions[0].y, 0);
            Gizmos.DrawSphere(startPos, 0.2f);
        }
    }

    /// <summary>
    /// 경로 디버그 정보 출력
    /// </summary>
    public void PrintPathInfo()
    {
        if (pathPositions.Count == 0)
        {
            Debug.Log("📍 No path calculated yet.");
            return;
        }

        Debug.Log($"📍 Path Info:");
        Debug.Log($"  - Total waypoints: {pathPositions.Count}");
        Debug.Log($"  - Start position: {pathPositions[0]}");
        Debug.Log($"  - End position: {pathPositions[pathPositions.Count - 1]}");
        Debug.Log($"  - Direction: {(sortClockwise ? "Clockwise" : "Counter-clockwise")}");
    }

    void Update()
    {
        // 테스트용 키보드 단축키 - KeyBindingManager 사용
        if (KeyBindingManager.Instance != null)
        {
            if (KeyBindingManager.Instance.GetShowMonsterPathKeyDown())
            {
                PrintPathInfo();
            }

            if (KeyBindingManager.Instance.GetTestPathfindingKeyDown())
            {
                RecalculatePath();
            }
        }
        else
        {
            // KeyBindingManager가 없으면 기본 키 사용 (fallback)
            if (Input.GetKeyDown(KeyCode.P))
            {
                PrintPathInfo();
            }

            if (Input.GetKeyDown(KeyCode.F))
            {
                RecalculatePath();
            }
        }
    }
}
