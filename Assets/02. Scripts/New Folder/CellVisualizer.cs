using UnityEngine;

public class CellVisualizer : MonoBehaviour
{
    public GameObject cellPrefab;
    public Transform parentTransform;

    public void VisualizeCell(Vector2Int pos)
    {
        if (cellPrefab == null)
        {
            Debug.LogWarning("CellVisualizer: cellPrefab이 설정되지 않았습니다.");
            return;
        }

        Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
        Instantiate(cellPrefab, worldPos, Quaternion.identity, parentTransform);
    }
}

