using UnityEngine;
using UnityEngine.EventSystems;

public class GhostCellClickHandler : MonoBehaviour, IPointerClickHandler
{
    private GridMapManager gridMapManager;
    private Vector2Int position;

    public void Initialize(GridMapManager manager, Vector2Int pos)
    {
        gridMapManager = manager;
        position = pos;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (gridMapManager != null)
        {
            gridMapManager.ExpandAt(position);
        }
    }

    // 일반 GameObject 클릭 감지 (OnMouseDown)
    void OnMouseDown()
    {
        if (gridMapManager != null)
        {
            gridMapManager.ExpandAt(position);
        }
    }
}

