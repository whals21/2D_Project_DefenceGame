using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 새로운 적 시스템 - GridMap 기반 경로 순회
/// MonsterPathManager가 생성한 경로를 따라 이동
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class NewEnemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Visual")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color damageColor = Color.white;

    [Header("Path Settings")]
    [SerializeField] private float waypointReachThreshold = 0.05f; // 웨이포인트 도달 판정 거리

    private float currentHealth;
    private List<Vector2Int> pathPositions; // 그리드 좌표 경로
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private NewEnemySpawner spawner;
    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }
    }

    /// <summary>
    /// 적 초기화
    /// </summary>
    public void ResetEnemy()
    {
        currentHealth = maxHealth;
        currentWaypointIndex = 0;
        isMoving = true;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = normalColor;
        }

        Debug.Log($"🔄 {gameObject.name} reset with {currentHealth} HP");
    }

    /// <summary>
    /// 스포너 설정
    /// </summary>
    public void SetSpawner(NewEnemySpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    /// <summary>
    /// 경로 설정 (그리드 좌표)
    /// </summary>
    public void SetPath(List<Vector2Int> newPath)
    {
        pathPositions = new List<Vector2Int>(newPath);
        currentWaypointIndex = 0;
        isMoving = true;

        Debug.Log($"📍 {gameObject.name} path set with {pathPositions.Count} waypoints");
    }

    void Update()
    {
        if (isMoving && pathPositions != null && pathPositions.Count > 0)
        {
            MoveAlongPath();
        }
    }

    /// <summary>
    /// 경로를 따라 이동
    /// </summary>
    void MoveAlongPath()
    {
        // 경로 끝에 도달하면 처음부터 다시 순회 (순환 경로)
        if (currentWaypointIndex >= pathPositions.Count)
        {
            currentWaypointIndex = 0;
            Debug.Log($"🔄 {gameObject.name} completed path loop, restarting");
        }

        // 현재 목표 웨이포인트 (월드 좌표로 변환)
        Vector2Int targetGridPos = pathPositions[currentWaypointIndex];
        Vector3 targetWorldPos = new Vector3(targetGridPos.x, targetGridPos.y, 0);
        Vector3 currentPosition = transform.position;

        // 목표 지점으로 이동
        Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetWorldPos, moveSpeed * Time.deltaTime);
        transform.position = newPosition;

        // 웨이포인트 도달 확인
        float distanceToTarget = Vector3.Distance(transform.position, targetWorldPos);
        if (distanceToTarget < waypointReachThreshold)
        {
            currentWaypointIndex++;

            // 다음 웨이포인트로 이동했음을 로그 (디버깅용, 필요시 주석 처리)
            // Debug.Log($"✅ {gameObject.name} reached waypoint {currentWaypointIndex - 1}");
        }
    }

    /// <summary>
    /// 데미지 받기
    /// </summary>
    public void TakeDamage(float damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"💥 {gameObject.name} took {damage} damage! HP: {currentHealth}/{maxHealth}");

        // 데미지 이펙트
        StartCoroutine(DamageFlash());

        // 죽음 처리
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 데미지 받았을 때 깜빡임 효과
    /// </summary>
    IEnumerator DamageFlash()
    {
        if (spriteRenderer == null) yield break;

        spriteRenderer.color = damageColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = normalColor;
    }

    /// <summary>
    /// 죽음 처리
    /// </summary>
    void Die()
    {
        Debug.Log($"💀 {gameObject.name} died!");

        isMoving = false;

        // 죽음 이펙트 (선택 사항)
        // TODO: 죽음 애니메이션, 파티클, 사운드 등

        // 오브젝트 풀로 반환
        if (spawner != null)
        {
            spawner.ReturnToPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 체력 회복 (테스트용)
    /// </summary>
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log($"💚 {gameObject.name} healed! HP: {currentHealth}/{maxHealth}");
    }

    /// <summary>
    /// 현재 체력 반환
    /// </summary>
    public float GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// 최대 체력 반환
    /// </summary>
    public float GetMaxHealth()
    {
        return maxHealth;
    }

    /// <summary>
    /// 이동 속도 설정
    /// </summary>
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }

    /// <summary>
    /// 클릭으로 데미지 테스트 (개발용)
    /// </summary>
    void OnMouseDown()
    {
        TakeDamage(20f);
    }

    /// <summary>
    /// 경로 시각화 (디버그용)
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (pathPositions == null || pathPositions.Count == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < pathPositions.Count - 1; i++)
        {
            Vector3 start = new Vector3(pathPositions[i].x, pathPositions[i].y, 0);
            Vector3 end = new Vector3(pathPositions[i + 1].x, pathPositions[i + 1].y, 0);
            Gizmos.DrawLine(start, end);
        }

        // 순환 경로: 마지막 -> 첫 번째 연결
        if (pathPositions.Count > 1)
        {
            Vector3 lastPos = new Vector3(pathPositions[pathPositions.Count - 1].x, pathPositions[pathPositions.Count - 1].y, 0);
            Vector3 firstPos = new Vector3(pathPositions[0].x, pathPositions[0].y, 0);
            Gizmos.DrawLine(lastPos, firstPos);
        }

        // 현재 목표 웨이포인트 표시
        if (currentWaypointIndex < pathPositions.Count)
        {
            Gizmos.color = Color.green;
            Vector3 targetPos = new Vector3(pathPositions[currentWaypointIndex].x, pathPositions[currentWaypointIndex].y, 0);
            Gizmos.DrawSphere(targetPos, 0.2f);
        }
    }
}
