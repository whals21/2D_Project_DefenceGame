using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용을 위해 추가

public abstract class MonsterBase : MonoBehaviour
{
    // moveSpeed를 property로 변경
    public float MoveSpeed
    {
        get { return moveSpeed; }
        set { moveSpeed = value; }
    }

    [Header("Monster stat")]
    protected int currentHP;
    protected int maxHP;
    protected float moveSpeed;

    [Header("Path setting")]
    protected Transform[] path;
    protected int targetIndex;
    private float arriveRange = 0.1f;
    [SerializeField] private float heightOffset = 0.5f;

    [Header("Reward setting")]
    [SerializeField] private int rewardMoney = 10;
    [SerializeField] private int rewardScore = 10;

    [Header("HP Display")]
    [SerializeField] private Image hpFillImage; // 체력 표시용 Image (월드 스페이스)
    [SerializeField] private Vector3 hpBarOffset = new Vector3(0, 1.5f, 0); // 몬스터 위에 표시할 위치 오프셋

    private GameManager gameManager;

    public virtual void Initialize(MonsterData data, Transform[] pathPoints)
    {
        currentHP = data.maxHP;
        maxHP = data.maxHP;
        moveSpeed = data.moveSpeed;
        path = pathPoints;
        targetIndex = 0;
        gameManager = FindObjectOfType<GameManager>();

        // HP Fill Image 자동 찾기 (설정되지 않은 경우)
        if (hpFillImage == null)
        {
            hpFillImage = GetComponentInChildren<Image>();
        }

        // 초기 체력 표시
        UpdateHPDisplay();
    }

    protected virtual void Update()
    {
        MovePath();
        UpdateHPBarPosition(); // HP 바 위치 업데이트
    }

    protected virtual void MovePath()
    {
        if (path == null || targetIndex >= path.Length) return;

        Transform target = path[targetIndex];
        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.position;

        // Y축 오프셋 적용
        targetPos.y += heightOffset;

        transform.position = Vector3.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(currentPos, targetPos) < arriveRange)
        {
            targetIndex++;
        }
        if (targetIndex >= path.Length)
        {
            ReachGoal();
        }
    }

    public virtual void TakeDamage(int damage)
    {
        currentHP -= damage;
        
        // 체력 표시 업데이트
        UpdateHPDisplay();

        if (currentHP <= 0)
        {
            Die();
        }
    }

    protected virtual void Die()
    {
        RewardMoney();
        RewardScore();
        Destroy(gameObject);
    }

    public virtual void RewardMoney()
    {
        if (gameManager != null && gameManager.player != null)
        {
            gameManager.player.AddMoney(rewardMoney);
        }
    }

    public virtual void RewardScore()
    {
        if (gameManager != null && gameManager.player != null)
        {
            gameManager.player.AddScore(rewardScore);
        }
    }

    public virtual void ReachGoal()
    {
        if (gameManager != null && gameManager.player != null)
        {
            gameManager.player.SubtractLife(1);
        }
        Destroy(gameObject);
    }

    // 체력 표시를 업데이트하는 메서드
    private void UpdateHPDisplay()
    {
        if (hpFillImage != null && maxHP > 0)
        {
            float hpRatio = (float)currentHP / maxHP;
            hpRatio = Mathf.Clamp01(hpRatio);
            
            // Image의 fillAmount 설정 (0~1 범위)
            hpFillImage.fillAmount = hpRatio;

            // 체력에 따라 색상 변경
            if (hpRatio > 0.5f)
                hpFillImage.color = Color.green; // 초록색
            else if (hpRatio > 0.2f)
                hpFillImage.color = Color.yellow; // 노란색
            else
                hpFillImage.color = Color.red; // 빨간색
        }
    }

    // HP 바의 위치를 몬스터를 따라가도록 업데이트
    private void UpdateHPBarPosition()
    {
        if (hpFillImage != null)
        {
            // 몬스터의 위치 + 오프셋
            hpFillImage.transform.position = transform.position + hpBarOffset;
        }
    }
}
