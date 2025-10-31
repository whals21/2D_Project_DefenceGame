# 그리드 맵 & 블록 시스템 스크립트 문서

## 목차
1. [시스템 개요](#시스템-개요)
2. [그리드 맵 시스템](#그리드-맵-시스템)
3. [블록 시스템](#블록-시스템)
4. [Unity 씬 설정](#unity-씬-설정)
5. [사용법 가이드](#사용법-가이드)
6. [중요 메커니즘](#중요-메커니즘)

---

## 시스템 개요

이 시스템은 동적으로 확장 가능한 그리드 맵과 테트리스/펜토미노 블록 배치 시스템을 제공합니다.

### 주요 기능
- ✅ 동적 그리드 맵 확장 (한 칸씩)
- ✅ 블록 생성 및 배치
- ✅ 블록 드래그 & 드롭
- ✅ 블록 회전 (90도씩, R키)
- ✅ 블록 간 충돌 감지 및 방지
- ✅ 배치 미리보기 (초록색/빨간색)

---

## 그리드 맵 시스템

### 1. Cell.cs
**역할**: 그리드의 기본 단위인 셀을 나타내는 데이터 클래스

**구조**:
```csharp
public class Cell
{
    public Vector2Int position;  // 셀의 그리드 위치
    public bool isOccupied;      // 블록으로 차지되었는지 여부
}
```

**사용법**: 직접 사용하지 않음. `GridMap`에서 자동으로 관리됨.

---

### 2. GridMap.cs
**역할**: 그리드 맵의 데이터 구조를 관리하는 클래스 (MonoBehaviour 아님)

**주요 필드**:
- `width`, `height`: 맵 크기 (동적 계산)
- `cells`: Dictionary<Vector2Int, Cell> - 셀 저장소

**주요 메서드**:
- `AddCell(Vector2Int pos)`: 특정 위치에 셀 추가
- `HasCell(Vector2Int pos)`: 셀이 존재하는지 확인
- `IsOccupied(Vector2Int pos)`: 셀이 블록으로 차지되었는지 확인
- `SetOccupied(Vector2Int pos, bool value)`: 셀의 차지 상태 설정
- `ExpandBottom/Top/Left/Right(Vector2Int pos)`: 한 칸씩 맵 확장
- `UpdateMapBounds()`: 맵 크기를 동적으로 재계산 (최소/최대 좌표 기준)

**중요 메커니즘**:
- `Dictionary<Vector2Int, Cell>`을 사용하여 셀들을 효율적으로 저장
- 맵 크기는 동적으로 계산됨 (음수 좌표 지원)
- 각 셀은 고유한 위치를 가짐

---

### 3. GridMapManager.cs
**역할**: 그리드 맵의 시각화와 확장 기능을 관리하는 MonoBehaviour

**주요 필드**:
- `width`, `height`: 초기 그리드 크기 (기본값: 10x10)
- `cellPrefab`: 셀 프리팹
- `ghostCellPrefab`: 확장 가능 위치 표시용 프리팹
- `cellVisualizer`: 셀 시각화 컴포넌트

**주요 메서드**:
- `VisualizeGrid()`: 그리드 맵 시각화 (중복 방지)
- `ShowExpandableCells()`: 확장 가능한 위치에 고스트 셀 표시
- `ExpandAt(Vector2Int pos)`: 특정 위치에 셀 추가 및 시각화
- `GetExpandablePositions()`: 확장 가능한 위치 리스트 반환
- `GetGridMap()`: GridMap 인스턴스 반환 (블록 시스템에서 사용)
- `OnBlockPlaced(Block block)`: 블록 배치 시 호출
- `OnBlockRemoved(Block block)`: 블록 제거 시 호출

**중요 메커니즘**:
- `GetExpandablePositions()`: 모든 기존 셀의 상하좌우 인접 위치를 확장 가능 위치로 계산
- 중복 방지를 위해 `HashSet` 사용
- 이미 셀이 있는 위치는 확장 가능 위치에서 제외

---

### 4. CellVisualizer.cs
**역할**: 셀을 시각화하는 별도 컴포넌트

**주요 필드**:
- `cellPrefab`: 셀 프리팹
- `parentTransform`: 부모 Transform

**주요 메서드**:
- `VisualizeCell(Vector2Int pos)`: 특정 위치에 셀 오브젝트 생성

**사용법**: `GridMapManager`에서 자동으로 초기화됨

---

### 5. GhostCellClickHandler.cs
**역할**: 확장 가능 위치(고스트 셀) 클릭 처리

**주요 메서드**:
- `Initialize(GridMapManager manager, Vector2Int pos)`: 초기화
- `OnPointerClick()`: UI 클릭 처리 (IPointerClickHandler)
- `OnMouseDown()`: 일반 GameObject 클릭 처리

**사용법**: 런타임에 `GridMapManager.ShowExpandableCells()`에서 자동으로 추가됨

**중요 메커니즘**:
- UI와 일반 GameObject 클릭 모두 지원
- 클릭 시 `GridMapManager.ExpandAt()` 호출

---

## 블록 시스템

### 6. BlockData.cs
**역할**: 블록의 데이터 구조 정의 (Serializable)

**구조**:
```csharp
[System.Serializable]
public class BlockData
{
    public string blockName;              // 블록 이름
    public List<Vector2Int> cellPositions; // 셀들의 상대 위치 (로컬 좌표)
    public Color blockColor;              // 블록 색상
}
```

**주요 메서드**:
- `Rotate90()`: 90도 시계방향 회전된 새 BlockData 반환
- `GetCenter()`: 블록의 중심점 계산

**중요 메커니즘**:
- 회전 공식: `(x, y) -> (y, -x)` (90도 시계방향)
- 원본 데이터는 변경하지 않고 새로운 인스턴스 반환
- `cellPositions`는 로컬 좌표 (0,0 기준)

---

### 7. Block.cs
**역할**: 블록 GameObject의 컴포넌트

**주요 필드**:
- `blockData`: 블록 데이터
- `gridPosition`: 그리드상의 위치 (기준점)
- `rotation`: 회전 상태 (0, 1, 2, 3 = 0도, 90도, 180도, 270도)
- `isPlacedOnGrid`: 그리드에 배치되었는지 여부
- `cellPrefab`: 블록을 구성하는 셀 프리팹
- `lastPlacedPositions`: 마지막으로 배치된 위치들 저장 (충돌 감지용)

**주요 메서드**:
- `GetWorldCellPositions()`: 현재 위치에서의 블록 셀 위치들 반환
- `GetWorldCellPositionsAt(Vector2Int targetGridPos)`: 특정 위치에서의 블록 셀 위치들 반환
- `GetRotatedData()`: 현재 회전 상태의 BlockData 반환 (캐시됨)
- `Rotate()`: 블록을 90도 회전
- `UpdateVisualization()`: 블록 시각화 업데이트
- `GetLastPlacedPositions()`: 마지막으로 배치된 위치들 반환
- `SetPlacedPositions(List<Vector2Int> positions)`: 배치된 위치 저장

**중요 메커니즘**:
- 회전 데이터 캐싱으로 성능 최적화 (`currentRotatedData`)
- `lastPlacedPositions`로 충돌 감지 시 정확한 위치 추적
- 회전 시에도 `gridPosition`은 유지 (로컬 좌표만 변경)

---

### 8. BlockDragger.cs
**역할**: 블록의 드래그 & 드롭 처리

**주요 기능**:
- 마우스 클릭-드래그로 블록 이동
- 드래그 중 R키로 90도 회전
- 그리드에 자동 스냅
- 그리드 밖으로 드래그 시 블록 제거

**주요 메서드**:
- `OnMouseDown()`: 드래그 시작 (배치된 블록이면 먼저 제거)
- `OnMouseDrag()`: 드래그 중 (그리드 스냅 처리)
- `OnMouseUp()`: 드래그 종료 (배치 시도)
- `IsOutsideGrid()`: 블록이 그리드 밖에 있는지 확인
- `WorldToGridPosition(Vector3 worldPos)`: 월드 좌표를 그리드 좌표로 변환

**중요 메커니즘**:
- 드래그 중에는 블록을 그리드 좌표로 스냅 (`Mathf.RoundToInt`)
- `block.gridPosition`과 `transform.position` 동기화
- 회전 시 배치 가능 여부 확인 후 재배치 또는 원래 위치로 복귀

---

### 9. BlockPlacer.cs
**역할**: 블록을 그리드에 배치하고 관리

**주요 필드**:
- `gridMapManager`: GridMapManager 참조
- `previewMaterial`: 미리보기용 머티리얼 (선택사항)

**주요 메서드**:
- `UpdateBlockPreview(Block block)`: 블록 배치 미리보기 업데이트
- `TryPlaceBlock(Block block)`: 블록 배치 시도
- `CanPlaceBlockAt(Vector2Int gridPos, Block block, List<Vector2Int> cellPositions)`: 배치 가능 여부 확인
- `PlaceBlockOnGrid(Block block, Vector2Int gridPos)`: 블록을 그리드에 배치
- `RemoveBlockFromGrid(Block block)`: 그리드에서 블록 제거
- `CreatePreview()`: 배치 미리보기 생성 (초록색: 가능, 빨간색: 불가능)
- `ClearPreview()`: 미리보기 제거

**중요 메커니즘**:
- **이중 충돌 체크**:
  1. `GridMap.IsOccupied()` 체크 (그리드 셀 상태)
  2. `BlockCollisionChecker` 체크 (블록 간 충돌)
- **배치 프로세스**:
  1. 기존 위치에서 제거 (`SetOccupied(false)`)
  2. 새 위치 설정 (`gridPosition` 업데이트)
  3. 새 위치에 배치 (`SetOccupied(true)`)
  4. 배치 위치 저장 (`SetPlacedPositions()`)
  5. BlockCollisionChecker에 등록
- 현재 블록의 기존 위치는 충돌 체크에서 제외 (`HashSet` 사용)

---

### 10. BlockCollisionChecker.cs
**역할**: 블록 간 충돌 감지

**주요 필드**:
- `placedBlocks`: 배치된 블록 목록

**주요 메서드**:
- `CheckBlockCollision(Block checkingBlock, Vector2Int targetPosition)`: 블록 간 충돌 체크
- `CheckAllCollisions(Block checkingBlock, Vector2Int targetPosition)`: 그리드 범위 + 블록 간 충돌 모두 체크
- `RegisterBlock(Block block)`: 블록을 배치 목록에 등록
- `UnregisterBlock(Block block)`: 블록을 배치 목록에서 제거
- `GetBlockCellPositionsAt(Block block, Vector2Int gridPosition)`: 특정 위치의 블록 셀 위치들 반환

**중요 메커니즘**:
- 배치된 블록 목록(`placedBlocks`) 관리
- **`GetLastPlacedPositions()` 사용**: 실제 배치된 위치 기준으로 충돌 체크 (회전 상태와 무관)
- `HashSet`으로 빠른 충돌 검색 (O(1) 조회)
- 자기 자신은 충돌 체크에서 제외

---

### 11. BlockFactory.cs
**역할**: 블록 생성 및 프리팹 관리

**주요 필드**:
- `blockPrefab`: Block 컴포넌트가 있는 프리팹
- `cellPrefab`: 블록을 구성하는 셀 프리팹

**주요 메서드**:
- `CreateBlock(BlockData data, Vector3 position)`: 블록 생성 및 초기화
- `CreateTetrisI/O/T/L/J/S/Z()`: 테트리스 블록 데이터 생성 (정적 메서드)
- `CreatePentominoF/P()`: 펜토미노 블록 데이터 생성 (정적 메서드)

**중요 메커니즘**:
- 블록 생성 시 `BlockDragger` 컴포넌트 자동 추가
- 블록 데이터와 시각화 자동 설정
- 정적 메서드로 블록 데이터 생성 (인스턴스 불필요)

---

### 12. BlockTest.cs
**역할**: 블록 시스템 테스트용 헬퍼 스크립트

**주요 필드**:
- `blockFactory`: BlockFactory 참조
- `spawnPosition`: 블록 생성 위치 (기본값: 15, 5, 0)

**주요 기능**:
- 키보드 입력으로 블록 생성
- 랜덤 블록 생성
- 모든 블록 제거

**사용법**:
- **1-7**: 테트리스 블록 생성 (I, O, T, L, J, S, Z)
- **8-9**: 펜토미노 블록 생성 (F, P)
- **Space**: 랜덤 블록 생성
- **C**: 모든 블록 제거
- 화면에 안내 메시지 표시 (`OnGUI`)

---

## Unity 씬 설정

### 필수 GameObject 구조

```
씬 (Scene)
├── GridMapManager (GameObject)
│   ├── GridMapManager (Component)
│   │   ├── Width: 10
│   │   ├── Height: 10
│   │   ├── Cell Prefab: [셀 프리팹 할당]
│   │   ├── Ghost Cell Prefab: [고스트 셀 프리팹 할당]
│   │   └── Cell Visualizer: [CellVisualizer 컴포넌트 할당]
│   └── CellVisualizer (Component) [선택사항]
│       ├── Cell Prefab: [셀 프리팹 할당]
│       └── Parent Transform: [GridMapManager Transform 할당]
│
├── BlockPlacer (GameObject)
│   └── BlockPlacer (Component)
│       └── Grid Map Manager: [GridMapManager 할당]
│
├── BlockCollisionChecker (GameObject)
│   └── BlockCollisionChecker (Component)
│       └── (자동으로 GridMapManager 찾음)
│
├── BlockFactory (GameObject)
│   └── BlockFactory (Component)
│       ├── Block Prefab: [블록 프리팹 할당]
│       └── Cell Prefab: [셀 프리팹 할당]
│
└── BlockTest (GameObject) [선택사항]
    └── BlockTest (Component)
        ├── Block Factory: [BlockFactory 할당]
        └── Spawn Position: (15, 5, 0)
```

### 프리팹 설정

#### 1. Cell Prefab
- **구성**: SpriteRenderer가 있는 GameObject
- **크기**: 1x1 단위
- **용도**: 그리드 셀 및 블록 구성 요소

#### 2. Ghost Cell Prefab
- **구성**: SpriteRenderer가 있는 GameObject
- **선택사항**: Button 컴포넌트 (UI 사용 시)
- **용도**: 확장 가능 위치 표시
- **참고**: Collider2D는 런타임에 자동 추가됨

#### 3. Block Prefab
- **구성**: 빈 GameObject
- **필수 컴포넌트**:
  - `Block` 컴포넌트
  - `BlockDragger` 컴포넌트
  - `Collider2D` (클릭 감지용)
- **용도**: 블록 GameObject 템플릿

---

## 사용법 가이드

### 그리드 맵 확장하기

#### 방법 1: UI 버튼 사용
```csharp
GridMapManager gridMapManager = FindObjectOfType<GridMapManager>();
gridMapManager.ShowExpandableCells(); // 확장 가능 위치 표시
// 고스트 셀을 클릭하면 자동으로 확장됨
```

#### 방법 2: 코드로 직접 확장
```csharp
gridMapManager.ExpandAt(new Vector2Int(5, 5)); // 특정 위치에 셀 추가
```

### 블록 생성하기

```csharp
BlockFactory factory = FindObjectOfType<BlockFactory>();

// 테트리스 블록 생성
BlockData tetrisT = BlockFactory.CreateTetrisT();
Block block = factory.CreateBlock(tetrisT, new Vector3(15, 5, 0));

// 펜토미노 블록 생성
BlockData pentominoF = BlockFactory.CreatePentominoF();
Block block2 = factory.CreateBlock(pentominoF, new Vector3(18, 5, 0));
```

### 블록 조작하기

#### 드래그 & 배치
1. **블록 클릭**: 마우스로 블록을 클릭
2. **드래그**: 마우스를 이동하여 블록 이동
3. **그리드 스냅**: 블록이 그리드 좌표로 자동 스냅됨
4. **미리보기**: 초록색(배치 가능) 또는 빨간색(배치 불가능)
5. **배치**: 마우스 버튼을 놓으면 자동 배치

#### 회전
- **R키**: 드래그 중 R키를 누르면 90도씩 회전
- 회전 후 배치 가능 여부 자동 확인
- 배치 불가능 시 자동으로 원래 위치로 복귀

#### 재배치
- 이미 배치된 블록을 다시 클릭하여 드래그 가능
- 새로운 위치에 배치 가능

#### 제거
- 블록을 그리드 밖으로 드래그하면 자동 제거

---

## 중요 메커니즘

### 1. 그리드 좌표 시스템
- 그리드 좌표는 정수 단위 (`Vector2Int`)
- 월드 좌표와 1:1 매핑 (x, y 좌표 그대로 사용)
- 예: 그리드 좌표 (5, 3) = 월드 좌표 (5, 3, 0)

### 2. 블록 위치 계산
- 블록의 `gridPosition`은 기준점 (일반적으로 첫 번째 셀 또는 중심)
- 각 셀의 위치 = `gridPosition + localPosition`
- 회전 시 `localPosition`만 변경되고 `gridPosition`은 유지

### 3. 충돌 감지 메커니즘

**2단계 충돌 체크**:
1. **GridMap.IsOccupied() 체크**: 그리드 셀의 차지 상태 확인
2. **BlockCollisionChecker 체크**: 다른 블록과의 충돌 확인

**충돌 체크 순서**:
```csharp
// 1. 현재 블록의 기존 위치들 제외
HashSet<Vector2Int> currentBlockPositions = ...;

// 2. 각 셀 위치 검사
foreach (cellPos in cellPositions) {
    if (gridMap.IsOccupied(cellPos) && !currentBlockPositions.Contains(cellPos)) {
        return false; // 다른 블록이 차지하고 있음
    }
}

// 3. BlockCollisionChecker로 추가 확인
if (collisionChecker.CheckBlockCollision(...)) {
    return false; // 충돌 발생
}
```

### 4. 배치 프로세스

**순서**:
1. 기존 위치에서 제거 (`SetOccupied(false)`)
2. 새 위치 설정 (`gridPosition` 업데이트)
3. 새 위치에 배치 (`SetOccupied(true)`)
4. 배치 위치 저장 (`SetPlacedPositions()`)
5. BlockCollisionChecker에 등록 (`RegisterBlock()`)

**중요**: 기존 위치를 먼저 제거하여 다른 블록이 해당 위치를 사용할 수 있도록 함

### 5. 회전 메커니즘

**회전 공식**:
```
90도 시계방향: (x, y) -> (y, -x)
```

**회전 프로세스**:
1. `rotation` 값 증가 (0 -> 1 -> 2 -> 3 -> 0)
2. 회전 데이터 캐시 초기화 (`currentRotatedData = null`)
3. 시각화 업데이트 (`UpdateVisualization()`)
4. 배치된 블록이면 배치 가능 여부 확인
5. 배치 가능하면 재배치, 불가능하면 원래 위치로 복귀

### 6. 블록 위치 추적

**`lastPlacedPositions` 사용 이유**:
- 블록이 회전하면 `GetWorldCellPositions()`는 새로운 위치를 반환
- 충돌 체크 시에는 실제 배치된 위치를 사용해야 함
- 따라서 배치 시 위치를 저장하고 충돌 체크에 사용

---

## 주의사항

### 필수 설정
1. **프리팹 설정**: 모든 프리팹이 올바르게 설정되어 있어야 함
2. **BlockCollisionChecker**: 씬에 반드시 하나만 존재해야 함
3. **Camera 설정**: `OnMouseDown` 사용 시 Camera가 필요함
4. **이벤트 시스템**: UI Button 사용 시 EventSystem 필요

### 동작 특징
1. **그리드 좌표**: 블록의 위치는 항상 정수 좌표로 스냅됨
2. **회전 기준**: 블록은 기준점(`gridPosition`)을 중심으로 회전
3. **충돌 감지**: 배치된 위치를 기준으로 충돌 체크
4. **미리보기**: 드래그 중에만 표시됨

---

## 문제 해결

### 블록이 겹쳐지는 경우
- ✅ `BlockCollisionChecker`가 씬에 있는지 확인
- ✅ `GridMapManager.OnBlockPlaced()`가 호출되는지 확인
- ✅ `PlaceBlockOnGrid()`에서 `SetPlacedPositions()`가 호출되는지 확인

### 블록이 그리드에 맞지 않는 경우
- ✅ `BlockDragger.WorldToGridPosition()` 메서드 확인
- ✅ 블록의 `gridPosition`이 올바르게 업데이트되는지 확인
- ✅ `transform.position`과 `gridPosition`이 동기화되는지 확인

### 회전 시 프리뷰가 어긋나는 경우
- ✅ `BlockPlacer.UpdateBlockPreview()`에서 `block.gridPosition` 사용 확인
- ✅ 회전 후 `gridPosition`이 변경되지 않는지 확인

### 블록이 배치되지 않는 경우
- ✅ `CanPlaceBlockAt()`의 반환값 확인
- ✅ `GridMap.HasCell()`이 true를 반환하는지 확인
- ✅ 충돌 체크가 올바르게 작동하는지 확인

---

## 확장 가능성

이 시스템은 다음과 같이 확장 가능합니다:

### 추가 가능한 기능
- 다양한 블록 모양 추가 (`BlockFactory`에 메서드 추가)
- 블록 회전 애니메이션 추가
- 블록 저장/로드 기능 (JSON/XML)
- 블록 스왑 기능
- 블록 미러링 기능
- 블록 회전 제한 (특정 블록만 회전 가능)
- 블록 그룹화 기능
- 블록 힌트 시스템

### 성능 최적화
- 셀 오브젝트 풀링
- 블록 위치 캐싱
- 충돌 체크 최적화 (공간 분할)

---

## 코드 예제

### 커스텀 블록 생성
```csharp
// 새로운 블록 모양 정의
public static BlockData CreateCustomBlock()
{
    List<Vector2Int> positions = new List<Vector2Int>
    {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(2, 0),
        new Vector2Int(1, 1)
    };
    return new BlockData("Custom", positions, Color.blue);
}

// 블록 생성
BlockFactory factory = FindObjectOfType<BlockFactory>();
BlockData customBlock = CreateCustomBlock();
Block block = factory.CreateBlock(customBlock, new Vector3(10, 10, 0));
```

### 그리드 확장 애니메이션
```csharp
// 확장 가능 위치를 일정 시간마다 표시
IEnumerator ShowExpandableCellsPeriodically()
{
    while (true)
    {
        gridMapManager.ShowExpandableCells();
        yield return new WaitForSeconds(2f);
    }
}
```

---

## 버전 정보

- **최종 업데이트**: 2024
- **Unity 버전**: 호환 (2020.3 이상 권장)
- **의존성**: UnityEngine, System.Collections.Generic

