using UnityEngine;

/// <summary>
/// 아이템 배치 시 시각적 효과 (백팩 히어로 스타일)
/// </summary>
public class ItemVisualEffects : MonoBehaviour
{
    [Header("Placement Effect")]
    public bool useScaleAnimation = true;
    public float scaleAnimDuration = 0.2f;
    public AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Hover Effect")]
    public bool useHoverEffect = true;
    public float hoverBrightness = 1.2f;

    [Header("Shake Effect")]
    public bool useShakeOnInvalidPlacement = true;
    public float shakeDuration = 0.3f;
    public float shakeIntensity = 0.1f;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Vector3 originalScale;
    private Vector3 originalPosition;

    // 애니메이션 상태
    private bool isAnimating = false;
    private float animationTime = 0f;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        // 생성 시 스케일 애니메이션
        if (useScaleAnimation)
        {
            PlayPlacementAnimation();
        }
    }

    void Update()
    {
        if (isAnimating)
        {
            UpdateAnimation();
        }
    }

    /// <summary>
    /// 배치 시 스케일 애니메이션
    /// </summary>
    public void PlayPlacementAnimation()
    {
        isAnimating = true;
        animationTime = 0f;
        transform.localScale = Vector3.zero;
    }

    void UpdateAnimation()
    {
        animationTime += Time.deltaTime;
        float progress = Mathf.Clamp01(animationTime / scaleAnimDuration);
        float curveValue = scaleCurve.Evaluate(progress);

        transform.localScale = originalScale * curveValue;

        if (progress >= 1f)
        {
            isAnimating = false;
            transform.localScale = originalScale;
        }
    }

    /// <summary>
    /// 잘못된 배치 시 흔들기 효과
    /// </summary>
    public void PlayShakeEffect()
    {
        if (!useShakeOnInvalidPlacement) return;
        StartCoroutine(ShakeCoroutine());
    }

    System.Collections.IEnumerator ShakeCoroutine()
    {
        float elapsed = 0f;
        Vector3 startPos = transform.localPosition;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;

            // 감쇠하는 진동
            float strength = (1f - progress) * shakeIntensity;
            float x = Random.Range(-1f, 1f) * strength;
            float y = Random.Range(-1f, 1f) * strength;

            transform.localPosition = startPos + new Vector3(x, y, 0);

            yield return null;
        }

        transform.localPosition = startPos;
    }

    void OnMouseEnter()
    {
        if (!useHoverEffect || spriteRenderer == null) return;

        // 밝게
        Color brightColor = originalColor * hoverBrightness;
        brightColor.a = originalColor.a;
        spriteRenderer.color = brightColor;
    }

    void OnMouseExit()
    {
        if (!useHoverEffect || spriteRenderer == null) return;

        // 원래 색상
        spriteRenderer.color = originalColor;
    }

    /// <summary>
    /// 원래 색상 업데이트 (색상 변경 시 호출)
    /// </summary>
    public void UpdateOriginalColor(Color newColor)
    {
        originalColor = newColor;
        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
    }
}
