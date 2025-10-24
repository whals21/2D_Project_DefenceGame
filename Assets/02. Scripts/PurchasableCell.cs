using UnityEngine;

/// <summary>
/// 구매 가능한 셀 - 클릭하면 그리드에 추가됨
/// </summary>
[RequireComponent(typeof(BoxCollider2D))]
public class PurchasableCell : MonoBehaviour
{
    public int gridX;
    public int gridY;
    public SpriteInventoryGrid grid;

    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private Color hoverColor;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // BoxCollider2D 설정
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider != null)
        {
            collider.size = Vector2.one;
        }

        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
            hoverColor = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a * 1.5f);
        }
    }

    void OnMouseEnter()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = hoverColor;
        }
    }

    void OnMouseExit()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }

    void OnMouseDown()
    {
        if (grid != null && grid.IsPurchaseMode())
        {
            Debug.Log($"🛒 Attempting to purchase cell at ({gridX}, {gridY})");

            if (grid.PurchaseCell(gridX, gridY))
            {
                // 구매 성공 - 이 오브젝트는 UpdatePurchasableCells()에서 자동으로 제거됨
                Debug.Log($"✅ Successfully purchased cell!");
            }
        }
    }
}
