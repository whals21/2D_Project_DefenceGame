using UnityEngine;

/// <summary>
/// 타워 데이터 ScriptableObject
/// MonsterData와 유사한 구조로 타워 설정 저장
/// </summary>
[CreateAssetMenu(fileName = "New Tower Data", menuName = "SO/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Basic Info")]
    public string towerName = "Basic Tower";
    public GameObject towerPrefab; // 타워 프리팹
    public Color towerColor = Color.white; // 타워 색상

    [Header("Tower Type")]
    public TowerType towerType = TowerType.RangeTower;

    [Header("Attack Stats")]
    public float attackRange = 4f;
    public float fireRate = 1f; // 초당 공격 횟수
    public int damage = 10;

    [Header("Bullet Settings")]
    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

    [Header("Melee Tower Settings")]
    public float attackEffectDuration = 0.2f;
    public GameObject slashEffectPrefab;
    public Color attackEffectColor = Color.red;

    [Header("Canon Tower Settings")]
    public float bulletLifeTime = 2f;
    public float explosionRadius = 2f;
    public GameObject explosionEffectPrefab;

    [Header("Glow Tower Settings")]
    public float glowDuration = 1f;
    public Color glowColor = Color.green;
    public float slowAmount = 0.5f; // 50% 감속
    public float glowRadius = 2f;
    public GameObject glowEffectPrefab;

    [Header("Spawn Settings")]
    public float spawnWeight = 10f; // 생성 가중치 (높을수록 자주 생성)

    public enum TowerType
    {
        RangeTower,     // 원거리 타워
        MeleeTower,     // 근접 타워
        CanonTower,     // 캐논 타워
        GlowTower       // 글로우 타워
    }
}
