using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeTower : TowerBase
{
    [Header("Visual Effects")]
    [SerializeField] private float attackEffectDuration = 0.2f;  // 공격 효과 지속 시간
    [SerializeField] private GameObject slashEffectPrefab;       // 베기 효과 프리팹
    [SerializeField] private Color attackEffectColor = Color.red; // 공격 효과 색상

    protected override void Update()
    {
        fireTimer += Time.deltaTime;
        
        if(fireTimer >= 1f / fireRate)
        {
            List<MonsterBase> monstersToAttack = new List<MonsterBase>(monsterInRange);
            
            foreach(MonsterBase monster in monstersToAttack)
            {
                if(monster == null) continue;
                Attack(monster);
            }
            
            // 공격할 때 시각효과 표시
            if(monstersToAttack.Count > 0)
            {
                StartCoroutine(ShowAttackEffect());
            }
            
            fireTimer = 0f;
        }
    }       

    protected virtual void Attack(MonsterBase target)
    {
        if (target == null) return;
        target.TakeDamage(damage);
        
        // 각 타겟에 대한 개별 효과
        ShowHitEffect(target.transform.position);
    }

    private IEnumerator ShowAttackEffect()
    {
        // 타워 주변에 원형 이펙트 표시
        SpriteRenderer towerSprite = GetComponent<SpriteRenderer>();
        Color originalColor = towerSprite.color;
        
        // 타워 색상 변경
        towerSprite.color = attackEffectColor;
        
        // 공격 범위 표시
        GameObject effectObj = new GameObject("AttackEffect");
        effectObj.transform.position = transform.position;
        
        // 원형 라인 렌더러 생성
        LineRenderer circleEffect = effectObj.AddComponent<LineRenderer>();
        circleEffect.useWorldSpace = false;
        circleEffect.startWidth = 0.1f;
        circleEffect.endWidth = 0.1f;
        circleEffect.startColor = attackEffectColor;
        circleEffect.endColor = attackEffectColor;
        
        // 원형 그리기
        int segments = 36;
        circleEffect.positionCount = segments + 1;
        for(int i = 0; i <= segments; i++)
        {
            float angle = ((float)i / segments) * 360f * Mathf.Deg2Rad;
            float x = Mathf.Cos(angle) * Range;
            float y = Mathf.Sin(angle) * Range;
            circleEffect.SetPosition(i, new Vector3(x, y, 0));
        }
        
        yield return new WaitForSeconds(attackEffectDuration);
        
        // 원래 색상으로 복구
        towerSprite.color = originalColor;
        Destroy(effectObj);
    }

    private void ShowHitEffect(Vector3 targetPosition)
    {
        if(slashEffectPrefab != null)
        {
            // 타겟 위치에 베기 효과 생성
            GameObject slashEffect = Instantiate(slashEffectPrefab, targetPosition, Quaternion.identity);
            Destroy(slashEffect, attackEffectDuration);
        }
    }
}
