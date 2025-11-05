using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//타워 사거리내에 일정 범위의 글로우 지대(초록색 영역)을 잠시 생성하는 타워. 글로우 지대를 지나가는 몬스터는 일정 시간동안 속도가 감소된다.
//이 타워는 공격타워가 아니라 데미지를 주지는 않지만 몬스터의 속도를 감소시키는 효과를 준다.
public class GlowTower : TowerBase
{
    [Header("Glow Settings")]
    [SerializeField] private float glowDuration = 1f;
    [SerializeField] private Color glowColor = Color.green;
    [SerializeField] private float slowAmount = 0.5f; // 50% 감속
    [SerializeField] private float glowRadius = 2f; // 글로우 효과 범위
    [SerializeField] private GameObject glowEffectPrefab; // 글로우 이펙트 프리팹

    private List<GameObject> activeGlowEffects = new List<GameObject>();
    private Dictionary<MonsterBase, Coroutine> slowedMonsters = new Dictionary<MonsterBase, Coroutine>();

    protected override void Fire(MonsterBase target)
    {
        if (target == null) return;
        StartCoroutine(CreateGlowEffect(target.transform.position));
    }

    private IEnumerator CreateGlowEffect(Vector3 position)
    {
        // 글로우 이펙트 생성
        GameObject glowEffect = Instantiate(glowEffectPrefab, position, Quaternion.identity);
        activeGlowEffects.Add(glowEffect);

        // 이펙트 설정
        SpriteRenderer glowSprite = glowEffect.GetComponent<SpriteRenderer>();
        if (glowSprite != null)
        {
            glowSprite.color = new Color(glowColor.r, glowColor.g, glowColor.b, 0.5f);
            glowSprite.transform.localScale = Vector3.one * glowRadius * 2;
        }

        float elapsedTime = 0f;
        while (elapsedTime < glowDuration)
        {
            // 글로우 범위 내의 몬스터 감지
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, glowRadius);
            foreach (Collider2D col in colliders)
            {
                if (col.TryGetComponent<MonsterBase>(out MonsterBase monster))
                {
                    ApplySlowEffect(monster);
                }
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 이펙트 제거
        activeGlowEffects.Remove(glowEffect);
        Destroy(glowEffect);
    }

    private void ApplySlowEffect(MonsterBase monster)
    {
        // 이미 감속 중인 몬스터는 무시
        if (slowedMonsters.ContainsKey(monster)) return;

        // 새로운 감속 효과 적용
        Coroutine slowRoutine = StartCoroutine(SlowMonster(monster));
        slowedMonsters[monster] = slowRoutine;
    }

    private IEnumerator SlowMonster(MonsterBase monster)
    {
        // 몬스터의 원래 속도 저장
        float originalSpeed = monster.MoveSpeed;
        
        // 감속 적용
        monster.MoveSpeed = originalSpeed * (1 - slowAmount);

        // 지속 시간만큼 대기
        yield return new WaitForSeconds(glowDuration);

        // 몬스터가 아직 존재하면 원래 속도로 복구
        if (monster != null)
        {
            monster.MoveSpeed = originalSpeed;
        }

        // 감속 목록에서 제거
        slowedMonsters.Remove(monster);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected(); // 기본 공격 범위 표시 (빨간색)

        // 글로우 범위 표시 (초록색)
        Gizmos.color = glowColor;
        Gizmos.DrawWireSphere(transform.position, glowRadius);
    }

    private void OnDestroy()
    {
        // 활성화된 모든 글로우 이펙트 제거
        foreach (GameObject effect in activeGlowEffects)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
    }
}
