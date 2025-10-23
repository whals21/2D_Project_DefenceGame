using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [HideInInspector] public InventoryItem itemData;
    [HideInInspector] public InventoryGrid grid;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 originalPosition;
    private Transform originalParent;
    private Canvas canvas;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        canvas = GetComponentInParent<Canvas>();
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"Started dragging {itemData.itemName}");
        
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        
        // 드래그 중 투명도 조절
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        
        // 최상위로 이동 (다른 아이템 위에 그리기)
        transform.SetAsLastSibling();
        
        // 그리드에서 제거 (임시)
        if (grid != null)
        {
            grid.RemoveItem(itemData);
        }
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // 마우스 위치로 이동
        if (canvas != null)
        {
            rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log($"Stopped dragging {itemData.itemName}");
        
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
        
        if (grid == null)
        {
            RestorePosition();
            return;
        }
        
        // 마우스 위치를 그리드 좌표로 변환
        Vector2Int gridPos = GetGridPositionFromMouse(eventData.position);
        
        // 그리드에 배치 시도
        if (grid.CanPlaceItem(itemData, gridPos))
        {
            grid.PlaceItem(itemData, gridPos);
            Debug.Log($"Placed {itemData.itemName} at {gridPos}");
        }
        else
        {
            // 배치 불가능하면 원래 위치로
            Debug.Log($"Cannot place {itemData.itemName} at {gridPos}, restoring position");
            RestorePosition();
            
            // 원래 위치에 다시 배치
            if (itemData.gridPosition.x >= 0 && itemData.gridPosition.y >= 0)
            {
                grid.PlaceItem(itemData, itemData.gridPosition);
            }
        }
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        // 우클릭으로 회전
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            if (grid != null)
            {
                grid.RotateItem(itemData);
                Debug.Log($"Rotated {itemData.itemName} to {itemData.rotation} degrees");
            }
        }
    }
    
    void RestorePosition()
    {
        rectTransform.anchoredPosition = originalPosition;
        transform.SetParent(originalParent);
    }
    
    Vector2Int GetGridPositionFromMouse(Vector2 mousePosition)
    {
        if (grid == null) return Vector2Int.zero;
        
        // 화면 좌표를 그리드 로컬 좌표로 변환
        RectTransform gridRect = grid.GetComponent<RectTransform>();
        if (gridRect == null) return Vector2Int.zero;
        
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            gridRect, 
            mousePosition, 
            canvas.worldCamera, 
            out localPoint
        );
        
        // 로컬 좌표를 그리드 인덱스로 변환
        int gridX = Mathf.FloorToInt(localPoint.x / grid.cellSize);
        int gridY = Mathf.FloorToInt(localPoint.y / grid.cellSize);
        
        return new Vector2Int(gridX, gridY);
    }
}
