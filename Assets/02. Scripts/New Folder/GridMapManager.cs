using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class GridMapManager : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    private GridMap map;
    public GameObject cellPrefab;
    public GameObject ghostCellPrefab;
    public CellVisualizer cellVisualizer;
    private List<GameObject> ghostCells = new List<GameObject>();

    // ✨ Cell GameObject 추적을 위한 Dictionary
    private Dictionary<Vector2Int, GameObject> cellGameObjects = new Dictionary<Vector2Int, GameObject>();


    void Start()
    {
        map = new GridMap(width, height);
        Debug.Log("맵 생성 완료: " + width + "x" + height);
        VisualizeGrid();

        // CellVisualizer 초기화
        if (cellVisualizer == null)
        {
            cellVisualizer = GetComponent<CellVisualizer>();
        }
        if (cellVisualizer != null)
        {
            cellVisualizer.cellPrefab = cellPrefab;
            cellVisualizer.parentTransform = transform;
        }
    }

    void VisualizeGrid()
    {
        // map의 실제 데이터를 기반으로 셀 생성
        foreach (var kvp in map.cells)
        {
            Vector2Int pos = kvp.Key;

            // 이미 생성된 Cell GameObject가 있는지 확인
            if (!cellGameObjects.ContainsKey(pos))
            {
                CreateCellGameObject(pos);
            }
        }
    }

    /// <summary>
    /// Cell GameObject 생성 및 CellCollider 컴포넌트 추가
    /// </summary>
    GameObject CreateCellGameObject(Vector2Int pos)
    {
        Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
        GameObject cellObj = Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);

        // ✨ CellCollider 컴포넌트 추가
        CellCollider cellCollider = cellObj.GetComponent<CellCollider>();
        if (cellCollider == null)
        {
            cellCollider = cellObj.AddComponent<CellCollider>();
        }
        cellCollider.Initialize(pos);

        // Dictionary에 추가
        cellGameObjects[pos] = cellObj;

        return cellObj;
    }

    public void OnExpandCellClicked(Vector2Int pos)
    {
        map.AddCell(pos);
        if (cellVisualizer != null)
        {
            cellVisualizer.VisualizeCell(pos); // 셀 오브젝트 생성
        }
        else
        {
            Debug.LogWarning("GridMapManager: CellVisualizer가 설정되지 않았습니다.");
        }
    }


    public void ShowExpandableCells()
    {
        // ✨ NEW: 토글 기능 - 고스트 셀이 이미 있으면 제거
        if (ghostCells.Count > 0)
        {
            ClearGhostCells();
            return;
        }

        // 기존 고스트 셀 제거 (혹시 모를 경우 대비)
        ClearGhostCells();

        List<Vector2Int> expandablePositions = GetExpandablePositions();

        foreach (Vector2Int pos in expandablePositions)
        {
            GameObject ghostCell = Instantiate(ghostCellPrefab, new Vector3(pos.x, pos.y, 0), Quaternion.identity);

            // GhostCellClickHandler 컴포넌트 추가 및 설정
            GhostCellClickHandler clickHandler = ghostCell.GetComponent<GhostCellClickHandler>();
            if (clickHandler == null)
            {
                clickHandler = ghostCell.AddComponent<GhostCellClickHandler>();
            }
            clickHandler.Initialize(this, pos);

            // Button 컴포넌트가 있으면 onClick 이벤트도 설정
            Button button = ghostCell.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => ExpandAt(pos));
            }

            // Collider가 없으면 추가 (클릭 감지를 위해)
            if (ghostCell.GetComponent<Collider>() == null && ghostCell.GetComponent<Collider2D>() == null)
            {
                // 2D 게임인 경우 BoxCollider2D 추가
                BoxCollider2D collider = ghostCell.AddComponent<BoxCollider2D>();
                collider.isTrigger = true;
            }

            ghostCells.Add(ghostCell);
        }
    }

    void ClearGhostCells()
    {
        foreach (GameObject ghostCell in ghostCells)
        {
            if (ghostCell != null)
            {
                if (Application.isPlaying)
                {
                    Destroy(ghostCell);
                }
                else
                {
                    DestroyImmediate(ghostCell);
                }
            }
        }
        ghostCells.Clear();
    }

    public List<Vector2Int> GetExpandablePositions()
    {
        HashSet<Vector2Int> expandablePositions = new HashSet<Vector2Int>();
        HashSet<Vector2Int> existingCells = new HashSet<Vector2Int>(map.cells.Keys);

        // 모든 기존 셀의 주변(상하좌우) 위치를 확인
        foreach (Vector2Int cellPos in existingCells)
        {
            // 상하좌우 네 방향 체크
            Vector2Int[] directions = new Vector2Int[]
            {
                new Vector2Int(0, 1),   // 위
                new Vector2Int(0, -1),  // 아래
                new Vector2Int(-1, 0),  // 왼쪽
                new Vector2Int(1, 0)    // 오른쪽
            };

            foreach (Vector2Int dir in directions)
            {
                Vector2Int neighborPos = cellPos + dir;
                
                // 인접한 위치에 셀이 없으면 확장 가능한 위치로 추가
                if (!existingCells.Contains(neighborPos))
                {
                    expandablePositions.Add(neighborPos);
                }
            }
        }

        return new List<Vector2Int>(expandablePositions);
    }

    public void ExpandAt(Vector2Int pos)
    {
        // 이미 셀이 있는 위치면 추가하지 않음
        if (map.HasCell(pos))
        {
            Debug.LogWarning($"이미 셀이 존재하는 위치입니다: {pos}");
            return;
        }

        // 고스트 셀 제거
        ClearGhostCells();

        // 셀 추가 (방향에 관계없이 단순히 셀 추가)
        map.AddCell(pos);
        map.UpdateMapBounds();

        // width와 height 업데이트
        width = map.width;
        height = map.height;

        // 새로 추가된 셀만 시각화 (중복 체크)
        bool cellExists = false;
        foreach (Transform child in transform)
        {
            Vector2Int childPos = new Vector2Int(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y));
            if (childPos == pos)
            {
                cellExists = true;
                break;
            }
        }

        if (!cellExists)
        {
            // ✨ CreateCellGameObject 사용하여 CellCollider와 함께 생성
            CreateCellGameObject(pos);
        }
    }

    void RedrawMap()
    {
        // 기존 셀 오브젝트들 제거
        foreach (Transform child in transform)
        {
            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        // 새로운 맵 크기로 셀 재생성
        VisualizeGrid();
    }

    // 블록 시스템 관련 메서드들
    public GridMap GetGridMap()
    {
        return map;
    }

    public void OnBlockPlaced(Block block)
    {
        // ✨ 블록이 차지하는 Cell들의 Collider 활성화
        List<Vector2Int> cellPositions = block.GetLastPlacedPositions();
        if (cellPositions.Count == 0)
        {
            cellPositions = block.GetWorldCellPositions();
        }

        foreach (Vector2Int pos in cellPositions)
        {
            if (cellGameObjects.ContainsKey(pos))
            {
                CellCollider cellCollider = cellGameObjects[pos].GetComponent<CellCollider>();
                if (cellCollider != null)
                {
                    cellCollider.SetOccupied(true, block);
                }
            }
        }

        Debug.Log($"Block '{block.blockData.blockName}' placed. {cellPositions.Count} cells occupied.");

        // ✨ 아이템 타워 블록이면 자동으로 활성화 (타워 블록보다 우선 체크)
        ItemTowerBlock itemTowerBlock = block.GetComponent<ItemTowerBlock>();
        if (itemTowerBlock != null)
        {
            itemTowerBlock.ActivateItemTower();
            // 아이템 타워는 공격하지 않으므로 TowerBlock 활성화 건너뛰기
        }
        else
        {
            // ✨ 일반 타워 블록이면 자동으로 활성화
            TowerBlock towerBlock = block.GetComponent<TowerBlock>();
            if (towerBlock != null)
            {
                towerBlock.ActivateTower();
            }
        }

        // ✨ BlockTowerManager에 알림 (있는 경우)
        BlockTowerManager towerManager = FindObjectOfType<BlockTowerManager>();
        if (towerManager != null)
        {
            towerManager.OnBlockPlaced(block);
        }
    }

    public void OnBlockRemoved(Block block)
    {
        // ✨ 타워 블록이면 비활성화
        TowerBlock towerBlock = block.GetComponent<TowerBlock>();
        if (towerBlock != null)
        {
            towerBlock.DeactivateTower();
        }

        // ✨ 아이템 타워 블록이면 비활성화
        ItemTowerBlock itemTowerBlock = block.GetComponent<ItemTowerBlock>();
        if (itemTowerBlock != null)
        {
            itemTowerBlock.DeactivateItemTower();
        }

        // ✨ BlockTowerManager에 알림 (있는 경우)
        BlockTowerManager towerManager = FindObjectOfType<BlockTowerManager>();
        if (towerManager != null)
        {
            towerManager.OnBlockRemoved(block);
        }

        // ✨ 블록이 차지했던 Cell들의 Collider 비활성화
        List<Vector2Int> cellPositions = block.GetLastPlacedPositions();
        if (cellPositions.Count == 0)
        {
            cellPositions = block.GetWorldCellPositions();
        }

        foreach (Vector2Int pos in cellPositions)
        {
            if (cellGameObjects.ContainsKey(pos))
            {
                CellCollider cellCollider = cellGameObjects[pos].GetComponent<CellCollider>();
                if (cellCollider != null)
                {
                    cellCollider.SetOccupied(false);
                }
            }
        }

        Debug.Log($"Block '{block.blockData.blockName}' removed. {cellPositions.Count} cells freed.");
    }

    /// <summary>
    /// 특정 위치의 Cell이 점유되어 있는지 확인
    /// </summary>
    public bool IsCellOccupied(Vector2Int pos)
    {
        if (cellGameObjects.ContainsKey(pos))
        {
            CellCollider cellCollider = cellGameObjects[pos].GetComponent<CellCollider>();
            if (cellCollider != null)
            {
                return cellCollider.IsOccupied();
            }
        }
        return false;
    }

    /// <summary>
    /// Cell GameObject 가져오기
    /// </summary>
    public GameObject GetCellGameObject(Vector2Int pos)
    {
        if (cellGameObjects.ContainsKey(pos))
        {
            return cellGameObjects[pos];
        }
        return null;
    }

}