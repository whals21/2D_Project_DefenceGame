using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 적 이동, 체력, 죽음 처리
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float moveSpeed = 2f;

    [Header("Visual")]
    [SerializeField] private Color normalColor = Color.red;
    [SerializeField] private Color damageColor = Color.white;

    private float currentHealth;
    private List<Vector3> path;
    private int currentWaypointIndex = 0;
    private bool isMoving = false;
    private EnemySpawner spawner;
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
    public void SetSpawner(EnemySpawner spawnerRef)
    {
        spawner = spawnerRef;
    }

    /// <summary>
    /// 경로 설정
    /// </summary>
    public void SetPath(List<Vector3> newPath)
    {
        path = newPath;
        currentWaypointIndex = 0;
    }

    void Update()
    {
        if (isMoving && path != null && path.Count > 0)
        {
            MoveAlongPath();
        }
    }

    /// <summary>
    /// 경로를 따라 이동
    /// </summary>
    void MoveAlongPath()
    {
        if (currentWaypointIndex >= path.Count)
        {
            // 경로 끝에 도달 - 다시 처음부터 순회
            currentWaypointIndex = 0;
        }

        Vector3 targetPosition = path[currentWaypointIndex];
        Vector3 currentPosition = transform.position;

        // 목표 지점으로 이동
        transform.position = Vector3.MoveTowards(currentPosition, targetPosition, moveSpeed * Time.deltaTime);

        // 웨이포인트 도달 확인
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            currentWaypointIndex++;
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
        if (path == null || path.Count == 0) return;

        Gizmos.color = Color.yellow;
        for (int i = 0; i < path.Count - 1; i++)
        {
            Gizmos.DrawLine(path[i], path[i + 1]);
        }

        // 현재 목표 웨이포인트 표시
        if (currentWaypointIndex < path.Count)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(path[currentWaypointIndex], 0.2f);
        }
    }
}
