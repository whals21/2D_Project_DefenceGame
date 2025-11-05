using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//RangeTower와 유사하지만 범위공격을 하는 타워. 범위내 모든 적에게 데미지를 입히는 타워.
public class CanonTower : TowerBase
{
    [Header("Canon Settings")]
    [SerializeField] private float bulletSpeed = 10f;
    [SerializeField] private int bulletDamage = 5;
    [SerializeField] private float bulletLifeTime = 2f;
    [SerializeField] private float explosionRadius = 2f; // 폭발 범위
    [SerializeField] private GameObject explosionEffectPrefab; // 폭발 이펙트

    protected override void Fire(MonsterBase target)
    {
        if (target == null || bulletPrefab == null) return;

        // 포탄 생성
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        CanonBullet canonBullet = bulletObj.GetComponent<CanonBullet>();
        
        if (canonBullet != null)
        {
            canonBullet.Initialize(target.transform.position, bulletSpeed, bulletDamage, 
                                 bulletLifeTime, explosionRadius, OnBulletExplode);
        }
    }

    private void OnBulletExplode(Vector3 explosionPosition)
    {
        // 폭발 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            GameObject effect = Instantiate(explosionEffectPrefab, explosionPosition, Quaternion.identity);
            Destroy(effect, 1f);
        }

        // 폭발 범위 내의 모든 몬스터 검색
        Collider2D[] colliders = Physics2D.OverlapCircleAll(explosionPosition, explosionRadius);
        
        foreach (Collider2D col in colliders)
        {
            if (col.TryGetComponent<MonsterBase>(out MonsterBase monster))
            {
                monster.TakeDamage(bulletDamage);
            }
        }
    }

    // 에디터에서 폭발 범위와 공격 범위 시각화
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // 부모의 공격 범위 시각화 (빨간색)
        
        // 폭발 범위 시각화 (노란색)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
