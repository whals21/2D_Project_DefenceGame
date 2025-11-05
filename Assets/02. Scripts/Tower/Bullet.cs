using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private int damage = 1;

    private Rigidbody2D rb;
    private MonsterBase targetMonster;

    public void Initialize(MonsterBase target)
    {
        targetMonster = target;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (targetMonster == null)
        {
            Destroy(gameObject);
            return;
        }

        //현재 총알위치 -> 몬스터 위치로 향하는 방향 벡터
        Vector2 dir = (targetMonster.transform.position - transform.position).normalized;
        rb.MovePosition((Vector2)transform.position + dir * speed * Time.fixedDeltaTime);

        if (Vector2.Distance(transform.position, targetMonster.transform.position) < 0.1f)
        {
            targetMonster.TakeDamage(damage);
            Destroy(gameObject);
        }
    }
}
