using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public Transform[] waypoints;      // 웨이포인트 배열
    public float speed = 2f;           // 이동 속도

    private int currentWaypointIndex = 0;

    void Update()
    {
        if (waypoints.Length == 0) return;

        // 현재 웨이포인트 방향으로 이동
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, speed * Time.deltaTime);

        // 웨이포인트에 도달했는지 확인
        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            currentWaypointIndex++;

            // 마지막 웨이포인트에 도달하면 처음으로 되돌림
            if (currentWaypointIndex >= waypoints.Length)
            {
                currentWaypointIndex = 0;
            }

        }
    }
}