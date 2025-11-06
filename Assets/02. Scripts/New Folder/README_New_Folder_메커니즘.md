# New Folder 스크립트 메커니즘 정리

## 📋 목차
1. [그리드 시스템](#1-그리드-시스템)
2. [블록 시스템](#2-블록-시스템)
3. [타워 배치 시스템](#3-타워-배치-시스템)
4. [경로 찾기 시스템](#4-경로-찾기-시스템)
5. [게임 관리 시스템](#5-게임-관리-시스템)

---

## 1. 그리드 시스템

### 📝 개요
2D 타워 디펜스 게임의 기반이 되는 그리드 맵 시스템입니다. 셀 기반 좌표 시스템으로 블록 배치와 몬스터 이동을 관리합니다.

### 🔑 주요 스크립트

#### **GridMap.cs**
그리드 데이터 구조를 관리하는 핵심 클래스

```csharp
// 주요 데이터 구조
public Dictionary<Vector2Int, Cell> cells; // 모든 셀 정보
```

**핵심 메서드:**
- `AddCell(Vector2Int position, Cell cell)` - 특정 위치에 셀 추가
- `GetCell(Vector2Int position)` - 특정 위치의 셀 가져오기
- `IsCellOccupied(Vector2Int position)` - 셀이 차지되었는지 확인
- `GetExpandablePositions()` - 확장 가능한 위치들 반환 (그리드 외곽)

**작동 원리:**
1. Dictionary를 사용하여 Vector2Int 좌표로 빠른 셀 접근
2. Cell 클래스에 isOccupied 플래그로 점유 상태 관리
3. 외곽 확장 계산 시 8방향 인접 체크

---

#### **GridMapManager.cs**
GridMap의 시각화와 게임 로직을 담당하는 매니저

```csharp
[SerializeField] private GridMap gridMap;
[SerializeField] private GameObject cellPrefab;
[SerializeField] private GameObject ghostCellPrefab;
```

**핵심 메서드:**
- `InitializeGrid(int width, int height)` - 초기 그리드 생성
- `ShowExpandableCells()` - 확장 가능한 셀 시각화 (고스트 셀)
- `HideExpandableCells()` - 고스트 셀 숨기기
- `OnBlockPlaced(Block block)` - 블록 배치 시 호출되는 이벤트 핸들러

**작동 원리:**
1. **그리드 초기화:**
   - cellPrefab을 width × height만큼 Instantiate
   - 각 셀의 worldPosition 설정 (Vector3)
   - GridMap에 Cell 데이터 등록

2. **확장 시스템:**
   - GetExpandablePositions()로 외곽 위치 계산
   - ghostCellPrefab 생성 → 반투명 표시
   - 클릭 시 실제 셀로 변환

3. **블록 배치 처리:**
   ```csharp
   // 우선순위: ItemTowerBlock > TowerBlock
   if (itemTowerBlock != null)
       itemTowerBlock.ActivateItemTower();
   else if (towerBlock != null)
       towerBlock.ActivateTower();
   ```

---

#### **Cell.cs**
개별 셀의 데이터와 상태를 저장

```csharp
public Vector2Int gridPosition;  // 그리드 좌표
public Vector3 worldPosition;    // 월드 좌표
public bool isOccupied;          // 점유 여부
public Block occupyingBlock;     // 점유 중인 블록
```

**용도:**
- 그리드 좌표 ↔ 월드 좌표 변환
- 셀 점유 상태 추적
- 블록 참조 저장

---

#### **CellVisualizer.cs**
셀의 시각적 상태를 관리 (색상, 하이라이트)

```csharp
public void SetColor(Color color);
public void Highlight();      // 마우스 오버 시
public void Unhighlight();    // 마우스 나갈 때
```

---

#### **GhostCellClickHandler.cs**
고스트 셀 클릭 시 실제 셀로 변환

```csharp
void OnMouseDown()
{
    gridMapManager.ExpandGrid(gridPosition);
    Destroy(gameObject); // 고스트 셀 제거
}
```

---

## 2. 블록 시스템

### 📝 개요
테트리스/펜토미노 스타일의 블록을 생성, 회전, 배치하는 시스템입니다.

### 🔑 주요 스크립트

#### **BlockData.cs**
블록의 모양 정의 (ScriptableObject 대신 순수 데이터 클래스)

```csharp
public class BlockData
{
    public string blockName;
    public List<Vector2Int> cellPositions;  // 블록을 구성하는 셀들의 상대 좌표
    public Color color;
}
```

**예시:**
```csharp
// L자 블록
cellPositions = {
    (0, 0), (0, 1), (0, 2), (1, 2)
}
```

---

#### **Block.cs**
실제 블록 GameObject의 동작을 관리

```csharp
public BlockData blockData;
public Vector2Int gridPosition;      // 블록의 기준 위치
public List<Vector2Int> currentShape; // 현재 회전된 모양
public bool isPlacedOnGrid;          // 그리드에 배치되었는지
```

**핵심 메서드:**

1. **회전 시스템:**
```csharp
public void Rotate()
{
    // 90도 시계방향 회전 공식
    // (x, y) → (y, -x)
    foreach (Vector2Int cell in currentShape)
    {
        rotatedShape.Add(new Vector2Int(cell.y, -cell.x));
    }
}
```

2. **월드 좌표 계산:**
```csharp
public List<Vector2Int> GetWorldCellPositions()
{
    // 현재 블록의 각 셀을 월드 그리드 좌표로 변환
    return currentShape.Select(pos => gridPosition + pos).ToList();
}
```

3. **시각화 업데이트:**
```csharp
public void UpdateVisualization()
{
    // 기존 자식 CellVisual 제거
    // currentShape에 따라 새로운 CellVisual 생성
    foreach (Vector2Int pos in currentShape)
    {
        GameObject cellObj = Instantiate(cellPrefab);
        cellObj.transform.localPosition = new Vector3(pos.x, pos.y, 0);
    }
}
```

---

#### **BlockFactory.cs**
다양한 블록 모양을 생성하는 팩토리 패턴

```csharp
public GameObject blockPrefab;
public GameObject cellPrefab;

// 정적 메서드로 BlockData 생성
public static BlockData CreateTetrisI() { ... }
public static BlockData CreatePentominoF() { ... }
public static BlockData CreateItemSingle() { ... }  // 1칸 아이템 블록

// 실제 GameObject 생성
public Block CreateBlock(BlockData data, Vector3 position)
{
    GameObject blockObj = Instantiate(blockPrefab, position, ...);
    Block block = blockObj.GetComponent<Block>();
    block.blockData = data;
    block.UpdateVisualization();
    return block;
}
```

**블록 종류:**
- **테트리스 블록:** I, O, T, L, J, S, Z
- **펜토미노 블록:** F, I, L, N, P, T, U, V, W, X, Y, Z
- **아이템 블록 (1-3칸):** Single, Line2H, Line2V, Diagonal2, Line3H, Line3V, L3, T3

---

#### **BlockDragger.cs**
마우스 드래그로 블록 이동

```csharp
void OnMouseDown()
{
    isDragging = true;
    offset = block.transform.position - GetMouseWorldPos();
}

void OnMouseDrag()
{
    block.transform.position = GetMouseWorldPos() + offset;
}

void OnMouseUp()
{
    isDragging = false;
    blockPlacer.TryPlaceBlock(block); // 배치 시도
}
```

---

#### **BlockCollisionChecker.cs**
블록 배치 가능 여부 판정

```csharp
public bool CanPlace(Block block, GridMap gridMap)
{
    List<Vector2Int> worldPositions = block.GetWorldCellPositions();

    foreach (Vector2Int pos in worldPositions)
    {
        // 1. 그리드 범위 내인지 확인
        if (!gridMap.cells.ContainsKey(pos))
            return false;

        // 2. 해당 셀이 비어있는지 확인
        if (gridMap.IsCellOccupied(pos))
            return false;
    }

    return true;
}
```

---

#### **BlockPlacer.cs**
블록을 그리드에 배치

```csharp
public bool TryPlaceBlock(Block block)
{
    if (!collisionChecker.CanPlace(block, gridMap))
    {
        block.transform.position = block.originalPosition; // 원래 위치로
        return false;
    }

    // 배치 성공
    block.isPlacedOnGrid = true;

    // 각 셀을 점유 상태로 변경
    foreach (Vector2Int pos in block.GetWorldCellPositions())
    {
        Cell cell = gridMap.GetCell(pos);
        cell.isOccupied = true;
        cell.occupyingBlock = block;
    }

    // 이벤트 발생
    gridMapManager.OnBlockPlaced(block);

    return true;
}
```

---

## 3. 타워 배치 시스템

### 📝 개요
배치된 블록을 실제 공격하는 타워로 활성화하는 시스템입니다.

### 🔑 주요 스크립트

#### **TowerBlock.cs**
Block을 타워로 변환하고 타워 생성 관리

```csharp
[SerializeField] private TowerData[] towerDataList;  // 가능한 타워 종류들
private TowerBase activeTower;                        // 생성된 타워
private bool isTowerActive = false;
```

**핵심 메서드:**

1. **타워 활성화:**
```csharp
public void ActivateTower()
{
    if (isTowerActive) return;

    // 1. 랜덤 타워 데이터 선택 (가중치 기반)
    TowerData selectedData = GetRandomTowerData();

    // 2. 타워 컴포넌트 추가
    TowerBase towerComponent = gameObject.AddComponent(
        selectedData.towerType  // RangeTower_1, MeleeTower_1 등
    ) as TowerBase;

    // 3. 리플렉션으로 protected 필드 설정
    SetTowerProperties(towerComponent, selectedData);

    // 4. CircleCollider2D 추가 (사거리)
    CircleCollider2D rangeCollider = gameObject.AddComponent<CircleCollider2D>();
    rangeCollider.isTrigger = true;
    rangeCollider.radius = selectedData.Range;

    activeTower = towerComponent;
    isTowerActive = true;
}
```

2. **리플렉션을 통한 필드 설정:**
```csharp
void SetTowerProperties(TowerBase tower, TowerData data)
{
    var baseType = typeof(TowerBase);

    // protected 필드에 접근
    var rangeField = baseType.GetField("Range",
        BindingFlags.NonPublic | BindingFlags.Instance);
    rangeField?.SetValue(tower, data.Range);

    var fireRateField = baseType.GetField("fireRate", ...);
    fireRateField?.SetValue(tower, data.fireRate);

    // 기타 필드들...
}
```

3. **가중치 기반 랜덤 선택:**
```csharp
TowerData GetRandomTowerData()
{
    // 각 타워의 spawnWeight 합계 계산
    float totalWeight = towerDataList.Sum(t => t.spawnWeight);

    // 0 ~ totalWeight 사이 랜덤값
    float randomValue = Random.value * totalWeight;

    // 누적합으로 선택
    float cumulative = 0f;
    foreach (var data in towerDataList)
    {
        cumulative += data.spawnWeight;
        if (randomValue <= cumulative)
            return data;
    }
}
```

---

#### **BlockTowerManager.cs**
타워 업그레이드 및 관리 (선택 사항)

```csharp
public void UpgradeTower(TowerBlock towerBlock)
{
    TowerBase tower = towerBlock.GetActiveTower();

    // 공격력, 사거리 등 스탯 증가
    tower.damage *= 1.2f;
    tower.Range *= 1.1f;
}
```

---

## 4. 경로 찾기 시스템

### 📝 개요
몬스터가 이동할 경로를 계산하고 관리하는 시스템입니다.

### 🔑 주요 스크립트

#### **NewPathFinder.cs**
A* 알고리즘을 사용한 경로 탐색

```csharp
private List<Vector2Int> currentPath;
private Vector2Int startPos;
private Vector2Int goalPos;
```

**핵심 알고리즘 - A* Pathfinding:**

```csharp
public List<Vector2Int> FindPath(Vector2Int start, Vector2Int goal, GridMap gridMap)
{
    // 1. 초기화
    HashSet<Vector2Int> openSet = new HashSet<Vector2Int> { start };
    HashSet<Vector2Int> closedSet = new HashSet<Vector2Int>();

    Dictionary<Vector2Int, Vector2Int> cameFrom = new Dictionary<Vector2Int, Vector2Int>();
    Dictionary<Vector2Int, float> gScore = new Dictionary<Vector2Int, float>();
    Dictionary<Vector2Int, float> fScore = new Dictionary<Vector2Int, float>();

    gScore[start] = 0;
    fScore[start] = Heuristic(start, goal);  // 맨해튼 거리

    // 2. A* 메인 루프
    while (openSet.Count > 0)
    {
        // fScore가 가장 낮은 노드 선택
        Vector2Int current = GetLowestFScore(openSet, fScore);

        if (current == goal)
        {
            return ReconstructPath(cameFrom, current);
        }

        openSet.Remove(current);
        closedSet.Add(current);

        // 3. 이웃 노드 탐색 (8방향)
        foreach (Vector2Int neighbor in GetNeighbors(current, gridMap))
        {
            if (closedSet.Contains(neighbor)) continue;

            float tentativeGScore = gScore[current] + 1;

            if (!openSet.Contains(neighbor))
            {
                openSet.Add(neighbor);
            }
            else if (tentativeGScore >= gScore[neighbor])
            {
                continue;
            }

            // 더 나은 경로 발견
            cameFrom[neighbor] = current;
            gScore[neighbor] = tentativeGScore;
            fScore[neighbor] = gScore[neighbor] + Heuristic(neighbor, goal);
        }
    }

    return null; // 경로 없음
}

float Heuristic(Vector2Int a, Vector2Int b)
{
    // 맨해튼 거리
    return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
}
```

**경로 재계산:**
```csharp
public void RecalculatePath()
{
    // GridMap의 점유 상태가 변경되었을 때 호출
    currentPath = FindPath(startPos, goalPos, gridMap);
}
```

---

#### **MonsterPathManager.cs**
몬스터가 순회할 외곽 경로 생성 및 관리

```csharp
private List<Vector2Int> pathPositions;  // 경로 위치 (순서대로)
private List<GameObject> pathCells;      // 경로 시각화용 셀들
```

**핵심 기능:**

1. **외곽 경로 생성:**
```csharp
public void ShowMonsterPath()
{
    // 1. 그리드 외곽 셀 계산 (8방향 인접 체크)
    List<Vector2Int> perimeterPositions = GetMonsterPathPositions(gridMap);

    // 2. 고스트 셀 생성 (주황색 반투명)
    foreach (Vector2Int pos in perimeterPositions)
    {
        CreatePathCell(pos);  // monsterPathCellPrefab 생성
    }

    // 3. 경로 재계산
    pathFinder.RecalculatePath();

    // 4. 적 스폰 시작
    enemySpawner.StartSpawning();
    monsterSpawner.StartSpawning();
}
```

2. **외곽 위치 계산:**
```csharp
List<Vector2Int> GetMonsterPathPositions(GridMap gridMap)
{
    HashSet<Vector2Int> existingCells = new HashSet<Vector2Int>(gridMap.cells.Keys);
    HashSet<Vector2Int> pathPositions = new HashSet<Vector2Int>();

    Vector2Int[] directions = {
        (0,1), (0,-1), (-1,0), (1,0),      // 상하좌우
        (-1,1), (1,1), (-1,-1), (1,-1)     // 대각선
    };

    // 모든 그리드 셀의 8방향 인접 위치 중 빈 곳 찾기
    foreach (Vector2Int cellPos in existingCells)
    {
        foreach (Vector2Int dir in directions)
        {
            Vector2Int neighborPos = cellPos + dir;
            if (!existingCells.Contains(neighborPos))
            {
                pathPositions.Add(neighborPos);
            }
        }
    }

    return new List<Vector2Int>(pathPositions);
}
```

3. **토글 기능:**
```csharp
// 경로가 이미 있으면 제거
if (pathCells.Count > 0)
{
    ClearPathCells();
    enemySpawner.StopSpawning();
    monsterSpawner.StopSpawning();
}
```

---

## 5. 게임 관리 시스템

### 📝 개요
게임 전체 흐름과 UI를 관리하는 시스템입니다.

### 🔑 주요 스크립트

#### **GameManager.cs**
게임 상태 및 전체 흐름 관리

```csharp
public enum GameState
{
    Playing,
    Paused,
    GameOver
}

private GameState currentState;
private int playerHealth;
private int gold;
private int wave;
```

**핵심 기능:**
- 게임 초기화
- 게임 상태 전환 (Playing ↔ Paused ↔ GameOver)
- 자원 관리 (체력, 골드)
- 웨이브 진행

---

#### **GameUIManager.cs**
UI 표시 및 사용자 입력 처리

```csharp
[SerializeField] private Text healthText;
[SerializeField] private Text goldText;
[SerializeField] private Text waveText;
```

**주요 메서드:**
- `UpdateHealthUI(int health)` - 체력 UI 업데이트
- `UpdateGoldUI(int gold)` - 골드 UI 업데이트
- `ShowGameOverScreen()` - 게임 오버 화면 표시

---

#### **NewEnemySpawner.cs**
적 오브젝트 풀링 및 스폰 관리

```csharp
[SerializeField] private GameObject enemyPrefab;
[SerializeField] private int poolSize = 50;
private Queue<GameObject> enemyPool;
```

**오브젝트 풀 패턴:**
```csharp
void InitializePool()
{
    enemyPool = new Queue<GameObject>();

    for (int i = 0; i < poolSize; i++)
    {
        GameObject enemy = Instantiate(enemyPrefab);
        enemy.SetActive(false);
        enemyPool.Enqueue(enemy);
    }
}

GameObject GetFromPool()
{
    if (enemyPool.Count > 0)
    {
        GameObject enemy = enemyPool.Dequeue();
        enemy.SetActive(true);
        return enemy;
    }

    // 풀이 비었으면 새로 생성
    return Instantiate(enemyPrefab);
}

void ReturnToPool(GameObject enemy)
{
    enemy.SetActive(false);
    enemyPool.Enqueue(enemy);
}
```

---

#### **CameraController.cs**
카메라 이동 및 줌 컨트롤

```csharp
[SerializeField] private float moveSpeed = 10f;
[SerializeField] private float zoomSpeed = 2f;
[SerializeField] private float minZoom = 5f;
[SerializeField] private float maxZoom = 20f;

void Update()
{
    // WASD 또는 화살표 키로 이동
    float horizontal = Input.GetAxis("Horizontal");
    float vertical = Input.GetAxis("Vertical");

    transform.Translate(new Vector3(horizontal, vertical, 0) * moveSpeed * Time.deltaTime);

    // 마우스 휠로 줌
    float scroll = Input.GetAxis("Mouse ScrollWheel");
    Camera.main.orthographicSize = Mathf.Clamp(
        Camera.main.orthographicSize - scroll * zoomSpeed,
        minZoom,
        maxZoom
    );
}
```

---

## 📊 전체 시스템 흐름도

```
게임 시작
    ↓
GridMapManager.InitializeGrid()
    ↓ (width × height 그리드 생성)
    ↓
BlockFactory.CreateBlock()
    ↓ (블록 생성)
    ↓
BlockDragger (사용자 드래그)
    ↓
BlockPlacer.TryPlaceBlock()
    ↓ (충돌 체크)
    ↓
GridMapManager.OnBlockPlaced()
    ↓
    ├─→ ItemTowerBlock.ActivateItemTower() (아이템 타워인 경우)
    │      ↓ (버프 적용)
    │      └─→ FindNearbyTowers() → ApplyBuffToTower()
    │
    └─→ TowerBlock.ActivateTower() (일반 타워인 경우)
           ↓ (타워 컴포넌트 추가)
           └─→ TowerBase (RangeTower_1, MeleeTower_1 등)
                  ↓ (몬스터 감지)
                  └─→ OnTriggerEnter2D() → Attack()
```

---

## 🔧 주요 디자인 패턴

### 1. **팩토리 패턴 (Factory Pattern)**
- **BlockFactory**: 다양한 블록 생성
- 코드 재사용성 증가, 생성 로직 중앙화

### 2. **싱글톤 패턴 (Singleton Pattern)**
- **GameManager**, **GridMapManager**
- 게임 전역에서 단일 인스턴스 접근

### 3. **오브젝트 풀 패턴 (Object Pool Pattern)**
- **NewEnemySpawner**
- 빈번한 생성/파괴로 인한 성능 저하 방지

### 4. **옵저버 패턴 (Observer Pattern)**
- **OnBlockPlaced** 이벤트
- 블록 배치 시 여러 시스템이 반응

### 5. **컴포넌트 패턴 (Component Pattern)**
- Block + BlockDragger + BlockPlacer
- 기능별로 컴포넌트 분리, 유연한 조합

---

## 💡 핵심 알고리즘

### 1. **블록 회전 알고리즘**
```csharp
// 90도 시계방향 회전
(x, y) → (y, -x)

// 예시: L자 블록
(0,0) → (0,0)
(0,1) → (1,0)
(0,2) → (2,0)
(1,2) → (2,-1)
```

### 2. **A* 경로 탐색**
- **gScore**: 시작점에서 현재 노드까지의 실제 비용
- **hScore**: 현재 노드에서 목표까지의 추정 비용 (휴리스틱)
- **fScore**: gScore + hScore (최소 fScore 노드 선택)

### 3. **가중치 기반 랜덤 선택**
```csharp
// 예: A(10), B(30), C(60)
누적합: [10, 40, 100]
랜덤값 35 → B 선택
```

### 4. **그리드 외곽 계산**
- 모든 그리드 셀의 8방향 인접 위치 중 빈 공간 찾기
- HashSet으로 중복 제거

---

## 🎯 최적화 포인트

### 1. **Dictionary 사용**
- `Dictionary<Vector2Int, Cell>`: O(1) 셀 접근
- List보다 빠른 탐색

### 2. **HashSet 사용**
- 중복 제거 및 Contains 체크 O(1)
- 경로 계산, 외곽 셀 찾기에 사용

### 3. **Object Pooling**
- 적 GameObject 재사용
- Instantiate/Destroy 비용 절감

### 4. **Coroutine 활용**
- 적 스폰, 반짝임 효과 등
- 메인 스레드 블록 방지

---

## 📚 참고 자료

- **Unity Documentation**: https://docs.unity3d.com/
- **A* Pathfinding**: https://www.redblobgames.com/pathfinding/a-star/introduction.html
- **Object Pool Pattern**: https://gameprogrammingpatterns.com/object-pool.html
- **Component Pattern**: https://gameprogrammingpatterns.com/component.html

---

## ✅ 학습 체크리스트

- [ ] GridMap과 GridMapManager의 차이점 이해
- [ ] BlockData와 Block의 관계 이해
- [ ] 블록 회전 공식 암기 및 구현
- [ ] A* 알고리즘 단계별 이해
- [ ] 리플렉션을 통한 필드 접근 이해
- [ ] 오브젝트 풀 패턴 구현 연습
- [ ] Dictionary vs List 성능 차이 이해
- [ ] Coroutine 활용법 숙지

---

**작성일:** 2025-01-10
**버전:** 1.0
**작성자:** Claude Code Assistant
