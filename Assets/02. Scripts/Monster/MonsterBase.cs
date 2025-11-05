using UnityEngine;
using UnityEngine.UI; // UI 컴포넌트 사용을 위해 추가
using TMPro; // TextMeshPro 사용을 위해 추가

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
    [SerializeField] private float waypointReachThreshold = 0.05f; // 웨이포인트 도달 판정 거리
    [SerializeField] private float heightOffset = 0.5f;

    [Header("Reward setting")]
    [SerializeField] private int rewardMoney = 10;
    [SerializeField] private int rewardScore = 10;

    [Header("HP Display")]
    [SerializeField] private Image hpFillImage; // 체력 표시용 Image (월드 스페이스)
    [SerializeField] private Transform hpBarTransform; // HP 바의 Transform (스케일 방식 사용 시)
    [SerializeField] private TMP_Text hpText; // TextMeshPro 체력 텍스트 (HPBar의 자식)
    [SerializeField] private Vector3 hpBarOffset = new Vector3(0, 1.5f, 0); // 몬스터 위에 표시할 위치 오프셋
    [SerializeField] private bool useScaleForHP = true; // true: 스케일 방식, false: fillAmount 방식

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

        // HP 바 Transform 자동 찾기
        if (hpBarTransform == null && hpFillImage != null)
        {
            hpBarTransform = hpFillImage.transform;
        }

        // TextMeshPro 자동 찾기 (설정되지 않은 경우)
        if (hpText == null)
        {
            hpText = GetComponentInChildren<TMP_Text>();
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
        if (path == null || targetIndex >= path.Length)
        {
            // 경로 끝에 도달하면 목표 도달 처리
            if (targetIndex >= path.Length)
            {
                ReachGoal();
            }
            return;
        }

        // 현재 목표 웨이포인트
        Transform target = path[targetIndex];
        Vector3 currentPosition = transform.position;
        Vector3 targetPos = target.position;

        // Y축 오프셋 적용
        targetPos.y += heightOffset;

        // 목표 지점으로 이동 (NewEnemy 방식과 동일)
        Vector3 newPosition = Vector3.MoveTowards(currentPosition, targetPos, moveSpeed * Time.deltaTime);
        transform.position = newPosition;

        // 웨이포인트 도달 확인
        float distanceToTarget = Vector3.Distance(transform.position, targetPos);
        if (distanceToTarget < waypointReachThreshold)
        {
            targetIndex++;
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
        if (maxHP <= 0) return;

        float hpRatio = (float)currentHP / maxHP;
        hpRatio = Mathf.Clamp01(hpRatio);

        // ✨ TextMeshPro 텍스트 업데이트 (체력 수치 표시)
        if (hpText != null)
        {
            hpText.text = $"{currentHP}";
            // 또는 최대 체력도 함께 표시하려면: hpText.text = $"{currentHP}/{maxHP}";
        }

        // 방식 1: 스케일 방식 (기본값, 더 안정적)
        if (useScaleForHP && hpBarTransform != null)
        {
            // HP 바의 X 스케일을 체력 비율에 맞게 조정
            Vector3 scale = hpBarTransform.localScale;
            scale.x = hpRatio;
            hpBarTransform.localScale = scale;

            // 체력에 따라 색상 변경
            if (hpFillImage != null)
            {
                if (hpRatio > 0.5f)
                    hpFillImage.color = Color.green; // 초록색
                else if (hpRatio > 0.2f)
                    hpFillImage.color = Color.yellow; // 노란색
                else
                    hpFillImage.color = Color.red; // 빨간색
            }

            Debug.Log($"🩺 {gameObject.name} HP: {currentHP}/{maxHP} ({hpRatio * 100:F0}%) - Scale: {scale.x}");
        }
        // 방식 2: fillAmount 방식 (Image Type이 Filled일 때만 작동)
        else if (hpFillImage != null)
        {
            // Image의 fillAmount 설정 (0~1 범위)
            hpFillImage.fillAmount = hpRatio;

            // 체력에 따라 색상 변경
            if (hpRatio > 0.5f)
                hpFillImage.color = Color.green; // 초록색
            else if (hpRatio > 0.2f)
                hpFillImage.color = Color.yellow; // 노란색
            else
                hpFillImage.color = Color.red; // 빨간색

            Debug.Log($"🩺 {gameObject.name} HP: {currentHP}/{maxHP} ({hpRatio * 100:F0}%) - FillAmount: {hpFillImage.fillAmount}");
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
