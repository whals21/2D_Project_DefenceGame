using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// 검은 경로 타일을 통해 적의 순회 경로를 계산
/// </summary>
public class PathFinder : MonoBehaviour
{
    private List<Vector3> pathWaypoints = new List<Vector3>();
    private List<GameObject> pathTiles = new List<GameObject>();

    /// <summary>
    /// 현재 계산된 경로를 반환
    /// </summary>
    public List<Vector3> GetPath()
    {
        return pathWaypoints;
    }

    /// <summary>
    /// 경로를 새로 계산 (구매 모드 종료 시 호출)
    /// </summary>
    public void CalculatePath()
    {
        pathWaypoints.Clear();
        pathTiles.Clear();

        // 씬에서 모든 PathTile 찾기
        GameObject[] allPathTiles = GameObject.FindGameObjectsWithTag("PathTile");

        if (allPathTiles.Length == 0)
        {
            Debug.LogWarning("⚠️ No PathTiles found in scene!");
            return;
        }

        // PathTile_0_0 찾기 (시작 지점)
        GameObject startTile = GameObject.Find("PathTile_0_0");

        if (startTile == null)
        {
            // PathTile_0_0이 없으면 원점에 가장 가까운 타일을 시작점으로
            startTile = allPathTiles.OrderBy(tile => Vector3.Distance(tile.transform.position, Vector3.zero)).First();
            Debug.LogWarning($"⚠️ PathTile_0_0 not found! Using closest tile: {startTile.name}");
        }

        // 경로 구성: 시계방향 또는 반시계방향으로 정렬
        pathTiles = SortPathTilesClockwise(allPathTiles.ToList(), startTile);

        // 월드 좌표로 변환
        foreach (GameObject tile in pathTiles)
        {
            pathWaypoints.Add(tile.transform.position);
        }

        // 순환 경로를 위해 시작점을 마지막에 다시 추가
        if (pathWaypoints.Count > 0)
        {
            pathWaypoints.Add(pathWaypoints[0]);
        }

        Debug.Log($"✅ Path calculated with {pathWaypoints.Count} waypoints");
    }

    /// <summary>
    /// 경로 타일을 시계방향으로 정렬 (그리드 외곽을 따라)
    /// 상하좌우로만 이동 (대각선 이동 금지)
    /// </summary>
    private List<GameObject> SortPathTilesClockwise(List<GameObject> tiles, GameObject startTile)
    {
        List<GameObject> sortedPath = new List<GameObject>();
        HashSet<GameObject> visited = new HashSet<GameObject>();

        GameObject currentTile = startTile;
        sortedPath.Add(currentTile);
        visited.Add(currentTile);

        // 인접한 타일을 찾아가며 경로 생성 (최대 반복 제한)
        int maxIterations = tiles.Count * 2;
        int iterations = 0;

        while (sortedPath.Count < tiles.Count && iterations < maxIterations)
        {
            iterations++;

            // 현재 타일에서 상하좌우 방향으로만 인접한 미방문 타일 찾기
            GameObject nextTile = null;
            float minDistance = float.MaxValue;

            foreach (GameObject tile in tiles)
            {
                if (visited.Contains(tile)) continue;

                Vector3 currentPos = currentTile.transform.position;
                Vector3 tilePos = tile.transform.position;

                float distance = Vector3.Distance(currentPos, tilePos);

                // 상하좌우로만 인접한 타일 선택 (대각선 제외)
                // X축 또는 Y축 중 하나만 차이가 있어야 함 (둘 다 차이나면 대각선)
                float deltaX = Mathf.Abs(currentPos.x - tilePos.x);
                float deltaY = Mathf.Abs(currentPos.y - tilePos.y);

                // 상하좌우 조건: 한 축은 같고(0.1 이하), 다른 축은 1.1 이하의 차이
                bool isOrthogonal = (deltaX < 0.1f && deltaY > 0.1f && deltaY < 1.1f) ||
                                   (deltaY < 0.1f && deltaX > 0.1f && deltaX < 1.1f);

                // Cell을 통과하지 않는지 검증
                bool pathIsClear = !IsPathBlockedByCell(currentPos, tilePos);

                if (isOrthogonal && pathIsClear && distance < minDistance)
                {
                    minDistance = distance;
                    nextTile = tile;
                }
            }

            // 인접 타일이 없으면 상하좌우로 가장 가까운 미방문 타일 선택
            if (nextTile == null)
            {
                foreach (GameObject tile in tiles)
                {
                    if (visited.Contains(tile)) continue;

                    Vector3 currentPos = currentTile.transform.position;
                    Vector3 tilePos = tile.transform.position;

                    float distance = Vector3.Distance(currentPos, tilePos);

                    float deltaX = Mathf.Abs(currentPos.x - tilePos.x);
                    float deltaY = Mathf.Abs(currentPos.y - tilePos.y);

                    // 여전히 상하좌우 우선이지만 거리 제한 완화
                    bool isOrthogonal = (deltaX < 0.1f || deltaY < 0.1f);

                    // Cell을 통과하지 않는지 검증
                    bool pathIsClear = !IsPathBlockedByCell(currentPos, tilePos);

                    if (isOrthogonal && pathIsClear && distance < minDistance)
                    {
                        minDistance = distance;
                        nextTile = tile;
                    }
                }
            }

            if (nextTile != null)
            {
                sortedPath.Add(nextTile);
                visited.Add(nextTile);
                currentTile = nextTile;
            }
            else
            {
                break; // 더 이상 방문할 타일이 없음
            }
        }

        return sortedPath;
    }

    /// <summary>
    /// 두 지점 사이 경로에 Cell이 있는지 검사
    /// </summary>
    private bool IsPathBlockedByCell(Vector3 from, Vector3 to)
    {
        // Raycast로 경로상에 Cell이 있는지 체크
        Vector3 direction = to - from;
        float distance = direction.magnitude;

        // 거리가 너무 짧으면 체크 안 함
        if (distance < 0.1f) return false;

        // Raycast 수행 (Cell 레이어 체크)
        RaycastHit2D[] hits = Physics2D.RaycastAll(from, direction.normalized, distance);

        foreach (RaycastHit2D hit in hits)
        {
            // Cell 태그를 가진 오브젝트가 있으면 차단됨
            if (hit.collider != null && hit.collider.CompareTag("Cell"))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// 경로 시각화 (디버그용)
    /// </summary>
    private void OnDrawGizmos()
    {
        if (pathWaypoints == null || pathWaypoints.Count < 2) return;

        Gizmos.color = Color.cyan;
        for (int i = 0; i < pathWaypoints.Count - 1; i++)
        {
            Gizmos.DrawLine(pathWaypoints[i], pathWaypoints[i + 1]);
            Gizmos.DrawSphere(pathWaypoints[i], 0.1f);
        }
    }
}
