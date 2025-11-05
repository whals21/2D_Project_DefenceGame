using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 블록에 타워 기능을 추가하는 컴포넌트
/// 블록이 그리드에 배치되면 자동으로 타워로 활성화됨
/// </summary>
[RequireComponent(typeof(Block))]
public class TowerBlock : MonoBehaviour
{
    [Header("Tower Type")]
    [SerializeField] private TowerType towerType = TowerType.RangeTower;

    [Header("Tower Settings")]
    [SerializeField] private float attackRange = 4f;
    [SerializeField] private float fireRate = 1f;
    [SerializeField] private int damage = 1;

    [Header("Prefab References")]
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Melee Tower Settings")]
    [SerializeField] private float attackEffectDuration = 0.2f;
    [SerializeField] private GameObject slashEffectPrefab;
    [SerializeField] private Color attackEffectColor = Color.red;

    [Header("Canon Tower Settings")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private float bulletLifeTime = 2f;
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private GameObject explosionEffectPrefab;

    [Header("Glow Tower Settings")]
    [SerializeField] private float glowDuration = 1f;
    [SerializeField] private Color glowColor = Color.green;
    [SerializeField] private float slowAmount = 0.5f;
    [SerializeField] private float glowRadius = 2f;
    [SerializeField] private GameObject glowEffectPrefab;

    // 내부 상태
    private Block block;
    private TowerBase activeTower;
    private CircleCollider2D rangeCollider;
    private bool isTowerActive = false;

    public enum TowerType
    {
        None,           // 타워 기능 없음
        RangeTower,     // 원거리 타워
        MeleeTower,     // 근접 타워 (범위 내 모든 적 공격)
        CanonTower,     // 캐논 타워 (폭발 범위 공격)
        GlowTower       // 글로우 타워 (감속 효과)
    }

    void Awake()
    {
        block = GetComponent<Block>();
    }

    void Start()
    {
        // 블록이 이미 그리드에 배치되어 있으면 타워 활성화 (백업용 - GridMapManager가 이미 호출했을 가능성이 높음)
        if (block != null && block.isPlacedOnGrid && !isTowerActive)
        {
            ActivateTower();
        }
    }

    /// <summary>
    /// 타워 활성화 (블록이 그리드에 배치될 때 호출)
    /// </summary>
    public void ActivateTower()
    {
        if (towerType == TowerType.None || isTowerActive)
            return;

        // 타워 컴포넌트 추가
        switch (towerType)
        {
            case TowerType.RangeTower:
                activeTower = gameObject.AddComponent<RangeTower_1>();
                break;

            case TowerType.MeleeTower:
                activeTower = gameObject.AddComponent<MeleeTower>();
                break;

            case TowerType.CanonTower:
                activeTower = gameObject.AddComponent<CanonTower>();
                break;

            case TowerType.GlowTower:
                activeTower = gameObject.AddComponent<GlowTower>();
                break;
        }

        if (activeTower != null)
        {
            // TowerBase의 필드는 protected이므로 리플렉션으로 설정
            SetTowerFields(activeTower);

            // 타워 감지 범위 Collider 추가
            rangeCollider = gameObject.AddComponent<CircleCollider2D>();
            rangeCollider.radius = attackRange;
            rangeCollider.isTrigger = true;

            isTowerActive = true;
            Debug.Log($"✅ {gameObject.name}에 {towerType} 타워 활성화!");
        }
    }

    /// <summary>
    /// 타워 비활성화 (블록이 그리드에서 제거될 때 호출)
    /// </summary>
    public void DeactivateTower()
    {
        if (!isTowerActive)
            return;

        // 타워 컴포넌트 제거
        if (activeTower != null)
        {
            Destroy(activeTower);
            activeTower = null;
        }

        // 범위 Collider 제거
        if (rangeCollider != null)
        {
            Destroy(rangeCollider);
            rangeCollider = null;
        }

        isTowerActive = false;
        Debug.Log($"🛑 {gameObject.name}의 타워 비활성화");
    }

    /// <summary>
    /// 리플렉션을 사용하여 TowerBase의 protected 필드 설정
    /// </summary>
    private void SetTowerFields(TowerBase tower)
    {
        var towerType = tower.GetType();
        var baseType = typeof(TowerBase);

        // TowerBase 필드 설정
        SetField(tower, baseType, "bulletPrefab", bulletPrefab);
        SetField(tower, baseType, "firePoint", firePoint != null ? firePoint : transform);
        SetField(tower, baseType, "Range", attackRange);
        SetField(tower, baseType, "fireRate", fireRate);
        SetField(tower, baseType, "damage", damage);

        // 타입별 추가 필드 설정
        if (tower is MeleeTower)
        {
            SetField(tower, towerType, "attackEffectDuration", attackEffectDuration);
            SetField(tower, towerType, "slashEffectPrefab", slashEffectPrefab);
            SetField(tower, towerType, "attackEffectColor", attackEffectColor);
        }
        else if (tower is CanonTower)
        {
            SetField(tower, towerType, "bulletSpeed", bulletSpeed);
            SetField(tower, towerType, "bulletDamage", damage);
            SetField(tower, towerType, "bulletLifeTime", bulletLifeTime);
            SetField(tower, towerType, "explosionRadius", explosionRadius);
            SetField(tower, towerType, "explosionEffectPrefab", explosionEffectPrefab);
        }
        else if (tower is GlowTower)
        {
            SetField(tower, towerType, "glowDuration", glowDuration);
            SetField(tower, towerType, "glowColor", glowColor);
            SetField(tower, towerType, "slowAmount", slowAmount);
            SetField(tower, towerType, "glowRadius", glowRadius);
            SetField(tower, towerType, "glowEffectPrefab", glowEffectPrefab);
        }
    }

    /// <summary>
    /// 리플렉션으로 필드 값 설정
    /// </summary>
    private void SetField(object obj, System.Type type, string fieldName, object value)
    {
        var field = type.GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(obj, value);
        }
        else
        {
            Debug.LogWarning($"⚠️ Field '{fieldName}' not found in {type.Name}");
        }
    }

    /// <summary>
    /// 타워 타입 변경 (런타임)
    /// </summary>
    public void SetTowerType(TowerType newType)
    {
        if (towerType == newType)
            return;

        // 기존 타워 비활성화
        if (isTowerActive)
        {
            DeactivateTower();
        }

        towerType = newType;

        // 그리드에 배치되어 있으면 새 타워 활성화
        if (block != null && block.isPlacedOnGrid)
        {
            ActivateTower();
        }
    }

    /// <summary>
    /// 현재 타워가 활성화되어 있는지 확인
    /// </summary>
    public bool IsTowerActive()
    {
        return isTowerActive;
    }

    /// <summary>
    /// 타워 타입 반환
    /// </summary>
    public TowerType GetTowerType()
    {
        return towerType;
    }

    /// <summary>
    /// Gizmos로 공격 범위 시각화
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (!isTowerActive || towerType == TowerType.None)
            return;

        // 공격 범위 (빨간색)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 타입별 추가 범위
        if (towerType == TowerType.CanonTower)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
        else if (towerType == TowerType.GlowTower)
        {
            Gizmos.color = glowColor;
            Gizmos.DrawWireSphere(transform.position, glowRadius);
        }
    }

    void OnDestroy()
    {
        // 타워 정리
        DeactivateTower();
    }
}
