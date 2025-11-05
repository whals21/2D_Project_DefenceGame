using UnityEngine;
using System.Collections.Generic;

public class TowerBase : MonoBehaviour
{
    [Header("bullet setting")]
    [SerializeField] protected GameObject bulletPrefab;
    [SerializeField] protected Transform firePoint;

    [Header("attack setting")]
    [SerializeField] protected float Range = 4f;
    [SerializeField] protected float fireRate = 1f;
    [SerializeField] protected int damage = 1;  // 데미지 값도 추가

    protected float fireTimer;
    protected List<MonsterBase> monsterInRange = new List<MonsterBase>(); //공격 사거리 내의 몬스터 리스트

    protected virtual void Update()
    {
        fireTimer += Time.deltaTime;

        MonsterBase target = GetNearestMonster();
        if(target == null) return;

        Vector2 dir = (target.transform.position - transform.position).normalized;
        transform.right = dir;

        if(fireTimer >= 1f / fireRate)
        {
            Fire(target);
            fireTimer = 0f;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.TryGetComponent<MonsterBase>(out var monster))
        {
            //리스트에 존재하지 않으면 추가
            if(!monsterInRange.Contains(monster))
            {
                monsterInRange.Add(monster);
            }
        }   
    }

    //사거리에서 나가면 리스트에서 제거
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.TryGetComponent<MonsterBase>(out var monster))
        {
            monsterInRange.Remove(monster);
        }
    }

    //사거리안에 저장된 몬스터중에서 가장 가까운 몬스터를 선택
    protected MonsterBase GetNearestMonster()
    {
        if(monsterInRange.Count == 0) return null;

        MonsterBase nearestMonster = null;

        float nearestDist = Mathf.Infinity; //Mathf.Infinity : 무한대
        Vector2 towerPos = transform.position; //타워 위치

        //공격 사거리 내의 몬스터를 하나씩 체크
        foreach(MonsterBase monster in monsterInRange)
        {
            if(monster == null) continue;

            float dist = Vector2.Distance(towerPos, monster.transform.position);
            //if(현재거리 < 지금까지 가장 가까운 거리)
            //자금까지 가장 가까운거리 = 현재거리
            //가장 가까운 몬스터로 타겟 갱신
            if(dist < nearestDist)
            {
                nearestDist = dist;
                nearestMonster = monster;
            }
        }

        return nearestMonster;
    }

    // private void Fire(MonsterBase target) 를 protected virtual로 변경
    protected virtual void Fire(MonsterBase target)
    {
        // bulletPrefab이 없으면 에러 방지
        if (bulletPrefab == null)
        {
            Debug.LogWarning($"Bullet prefab is not assigned on {gameObject.name}!");
            return;
        }

        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        
        if (bullet != null)
        {
            bullet.Initialize(target);
        }
        else
        {
            Debug.LogWarning($"Bullet component not found on {bulletObj.name}!");
            Destroy(bulletObj);
        }
    }

    // private void OnDrawGizmosSelected() 를 protected virtual로 변경
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Range);
    }
}

