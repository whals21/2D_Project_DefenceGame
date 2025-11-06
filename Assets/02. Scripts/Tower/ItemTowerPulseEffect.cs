using UnityEngine;

/// <summary>
/// 아이템 타워의 시각적 효과 (펄스, 스케일 애니메이션)
/// 일반 타워와 구별하기 위한 시각적 피드백 제공
/// </summary>
public class ItemTowerPulseEffect : MonoBehaviour
{
    [Header("Pulse Settings")]
    [SerializeField] private float pulseSpeed = 1.5f; // 펄스 속도
    [SerializeField] private float scaleAmplitude = 0.05f; // 크기 변화 폭 (5%)
    [SerializeField] private float brightnessAmplitude = 0.15f; // 밝기 변화 폭

    [Header("Glow Settings")]
    [SerializeField] private bool enableGlow = true; // 외곽 빛 효과
    [SerializeField] private Color glowColor = new Color(1f, 1f, 0.5f, 0.5f); // 노란빛

    private SpriteRenderer[] cellRenderers;
    private Color[] originalColors;
    private Vector3 originalScale;
    private float timeOffset;

    /// <summary>
    /// 초기화 (ItemTowerSpawner에서 호출)
    /// </summary>
    public void Initialize(SpriteRenderer[] renderers)
    {
        cellRenderers = renderers;
        originalScale = transform.localScale;
        timeOffset = Random.Range(0f, 2f * Mathf.PI); // 랜덤 시작 위상

        // 원래 색상 저장
        if (cellRenderers != null && cellRenderers.Length > 0)
        {
            originalColors = new Color[cellRenderers.Length];
            for (int i = 0; i < cellRenderers.Length; i++)
            {
                if (cellRenderers[i] != null)
                {
                    originalColors[i] = cellRenderers[i].color;
                }
            }
        }
    }

    void Update()
    {
        if (cellRenderers == null || cellRenderers.Length == 0)
            return;

        // 1. 스케일 펄스 효과 (크기가 부드럽게 커졌다 작아졌다)
        float scaleMultiplier = 1f + Mathf.Sin((Time.time + timeOffset) * pulseSpeed) * scaleAmplitude;
        transform.localScale = originalScale * scaleMultiplier;

        // 2. 밝기 펄스 효과 (색상이 밝아졌다 어두워졌다)
        float brightness = 1f + Mathf.Sin((Time.time + timeOffset) * pulseSpeed * 1.5f) * brightnessAmplitude;

        for (int i = 0; i < cellRenderers.Length; i++)
        {
            if (cellRenderers[i] != null && originalColors != null && i < originalColors.Length)
            {
                Color newColor = originalColors[i] * brightness;
                newColor.a = originalColors[i].a; // 알파값은 유지
                cellRenderers[i].color = newColor;
            }
        }

        // 3. 회전 효과 (선택 사항, 매우 느리게)
        // transform.Rotate(Vector3.forward, Time.deltaTime * 5f);
    }

    void OnDestroy()
    {
        // 원래 스케일로 복원
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }

        // 원래 색상으로 복원
        if (cellRenderers != null && originalColors != null)
        {
            for (int i = 0; i < cellRenderers.Length; i++)
            {
                if (cellRenderers[i] != null && i < originalColors.Length)
                {
                    cellRenderers[i].color = originalColors[i];
                }
            }
        }
    }

    /// <summary>
    /// 효과 일시정지/재개
    /// </summary>
    public void SetEnabled(bool enabled)
    {
        this.enabled = enabled;

        if (!enabled)
        {
            // 비활성화 시 원래 상태로 복원
            transform.localScale = originalScale;

            if (cellRenderers != null && originalColors != null)
            {
                for (int i = 0; i < cellRenderers.Length; i++)
                {
                    if (cellRenderers[i] != null && i < originalColors.Length)
                    {
                        cellRenderers[i].color = originalColors[i];
                    }
                }
            }
        }
    }
}
