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
        // 중복 방지를 위해 이미 생성된 셀 체크
        HashSet<Vector2Int> visualizedCells = new HashSet<Vector2Int>();
        
        foreach (Transform child in transform)
        {
            // 기존 셀 위치 추적 (이름이나 태그로 추적 가능)
            visualizedCells.Add(new Vector2Int(Mathf.RoundToInt(child.position.x), Mathf.RoundToInt(child.position.y)));
        }

        foreach (var kvp in map.cells)
        {
            Vector2Int pos = kvp.Key;
            if (!visualizedCells.Contains(pos))
            {
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
                Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
            }
        }
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
        // 기존 고스트 셀 제거
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
            if (cellVisualizer != null)
            {
                cellVisualizer.VisualizeCell(pos);
            }
            else
            {
                // CellVisualizer가 없으면 해당 위치에 직접 생성
                Vector3 worldPos = new Vector3(pos.x, pos.y, 0);
                Instantiate(cellPrefab, worldPos, Quaternion.identity, transform);
            }
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
        // 블록 배치 시 호출
        BlockCollisionChecker checker = FindObjectOfType<BlockCollisionChecker>();
        if (checker != null)
        {
            checker.RegisterBlock(block);
        }
    }

    public void OnBlockRemoved(Block block)
    {
        // 블록 제거 시 호출
        BlockCollisionChecker checker = FindObjectOfType<BlockCollisionChecker>();
        if (checker != null)
        {
            checker.UnregisterBlock(block);
        }
    }

}