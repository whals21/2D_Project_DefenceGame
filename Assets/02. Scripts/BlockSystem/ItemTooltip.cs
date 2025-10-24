using UnityEngine;
using TMPro;

/// <summary>
/// 아이템 마우스 오버 시 정보 표시 (백팩 히어로 스타일)
/// </summary>
public class ItemTooltip : MonoBehaviour
{
    [Header("Tooltip Settings")]
    public GameObject tooltipPrefab;
    public Vector3 tooltipOffset = new Vector3(1f, 1f, 0);
    public float showDelay = 0.3f;

    [Header("Tooltip Content")]
    public string itemName;
    [TextArea(2, 4)]
    public string itemDescription;
    public string itemStats;

    private GameObject currentTooltip;
    private float hoverTime = 0f;
    private bool isHovering = false;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnMouseEnter()
    {
        isHovering = true;
        hoverTime = 0f;
    }

    void OnMouseExit()
    {
        isHovering = false;
        hoverTime = 0f;
        HideTooltip();
    }

    void Update()
    {
        if (isHovering)
        {
            hoverTime += Time.deltaTime;

            if (hoverTime >= showDelay && currentTooltip == null)
            {
                ShowTooltip();
            }

            // 툴팁을 마우스 위치에 따라 이동
            if (currentTooltip != null)
            {
                UpdateTooltipPosition();
            }
        }
    }

    void ShowTooltip()
    {
        if (tooltipPrefab == null) return;

        // 툴팁 생성
        currentTooltip = Instantiate(tooltipPrefab);
        currentTooltip.name = "Tooltip_" + itemName;

        UpdateTooltipPosition();
        UpdateTooltipContent();
    }

    void UpdateTooltipPosition()
    {
        if (currentTooltip == null || mainCamera == null) return;

        // 마우스 위치 기준
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
        worldPos.z = -1f;  // 맨 앞에 표시

        currentTooltip.transform.position = worldPos + tooltipOffset;
    }

    void UpdateTooltipContent()
    {
        if (currentTooltip == null) return;

        // TextMeshPro 텍스트 업데이트
        TextMeshPro[] texts = currentTooltip.GetComponentsInChildren<TextMeshPro>();
        if (texts.Length > 0)
        {
            string content = $"<b>{itemName}</b>\n";
            if (!string.IsNullOrEmpty(itemDescription))
                content += $"{itemDescription}\n";
            if (!string.IsNullOrEmpty(itemStats))
                content += $"<color=yellow>{itemStats}</color>";

            texts[0].text = content;
        }
    }

    void HideTooltip()
    {
        if (currentTooltip != null)
        {
            Destroy(currentTooltip);
            currentTooltip = null;
        }
    }

    void OnDestroy()
    {
        HideTooltip();
    }
}
