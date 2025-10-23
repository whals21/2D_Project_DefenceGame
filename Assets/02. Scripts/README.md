# 백팩히어로 스타일 인벤토리 시스템

## 📋 개요
그리드 기반 인벤토리 시스템으로 다양한 크기의 블록을 배치, 이동, 회전, 제거할 수 있습니다.

## 🎮 주요 기능

### 1. 다양한 크기의 아이템
- 1x1, 1x2, 1x4, 2x3, 3x2 등 자유로운 크기 지원
- 각 아이템마다 고유한 색상과 이름 설정 가능

### 2. 배치 시스템
- 그리드 충돌 검사
- 범위 벗어남 방지
- 자동 위치 스냅

### 3. 회전 기능
- 90도 단위 회전 (0, 90, 180, 270)
- 회전 시 크기 자동 조정 (1x4 → 4x1)
- 회전 불가능한 경우 원상복구

### 4. 드래그 앤 드롭
- 마우스로 아이템 드래그
- 실시간 위치 업데이트
- 배치 불가능 시 원래 위치로 복귀

## 🔧 Unity 설정 방법

### 1. 프리팹 준비

#### CellPrefab (그리드 셀)
```
GameObject
├─ RectTransform
└─ Image (흰색, Alpha 0.3)
```

#### ItemPrefab (아이템)
```
GameObject
├─ RectTransform
├─ Image (색상은 코드에서 설정)
├─ CanvasGroup
├─ DraggableItem (스크립트)
└─ Text (자식 오브젝트, 아이템 이름 표시)
```

### 2. Canvas 구조
```
Canvas
└─ GridParent (RectTransform)
    └─ InventoryGrid (스크립트 연결)
```

### 3. Inspector 설정

**InventoryGrid 컴포넌트:**
- Grid Width: 6
- Grid Height: 6
- Cell Size: 80
- Grid Parent: GridParent Transform
- Cell Prefab: CellPrefab
- Item Prefab: ItemPrefab

**InventoryTest 컴포넌트:**
- Inventory Grid: InventoryGrid 컴포넌트 연결

## 💻 코드 사용 예시

### 기본 사용법

```csharp
// 1. 아이템 생성
InventoryItem sword = new InventoryItem("Long Sword", 1, 4);
sword.color = Color.red;

// 2. 배치 가능 여부 확인
Vector2Int position = new Vector2Int(0, 0);
if (inventoryGrid.CanPlaceItem(sword, position))
{
    // 3. 아이템 배치
    inventoryGrid.PlaceItem(sword, position);
}

// 4. 아이템 회전
inventoryGrid.RotateItem(sword);

// 5. 아이템 이동
inventoryGrid.MoveItem(sword, new Vector2Int(2, 2));

// 6. 아이템 제거
inventoryGrid.RemoveItem(sword);
```

### 고급 사용법

```csharp
// 특정 위치의 아이템 가져오기
InventoryItem item = inventoryGrid.GetItemAtPosition(new Vector2Int(3, 2));

// 모든 배치된 아이템 가져오기
List<InventoryItem> allItems = inventoryGrid.GetPlacedItems();

// 그리드 상태 출력 (디버깅)
inventoryGrid.PrintGridState();
```

## ⌨️ 키보드 단축키 (InventoryTest에서 제공)

- **R**: 첫 번째 아이템 회전
- **P**: 그리드 상태 콘솔 출력
- **C**: 모든 아이템 제거
- **T**: 배치 가능 여부 테스트

## 🖱️ 마우스 조작

- **좌클릭 드래그**: 아이템 이동
- **우클릭**: 아이템 회전

## 📊 그리드 상태 출력 예시

```
Grid State:
[ ][ ][ ][ ][ ][ ]
[ ][ ][ ][ ][ ][ ]
[ ][ ][ ][ ][ ][ ]
[X][X][ ][ ][ ][ ]
[X][X][ ][X][ ][ ]
[X][X][X][X][ ][ ]
```
- `[X]`: 차지된 공간
- `[ ]`: 빈 공간

## 🎯 타워디펜스 게임 적용 예시

```csharp
// 타워 블록 생성
InventoryItem arrowTower = new InventoryItem("Arrow Tower", 1, 1);
arrowTower.color = new Color(0.8f, 0.3f, 0.3f);

InventoryItem cannonTower = new InventoryItem("Cannon Tower", 2, 2);
cannonTower.color = new Color(0.3f, 0.3f, 0.8f);

InventoryItem magicTower = new InventoryItem("Magic Tower", 1, 2);
magicTower.color = new Color(0.8f, 0.3f, 0.8f);

// 필드에 배치
inventoryGrid.PlaceItem(arrowTower, new Vector2Int(0, 0));
inventoryGrid.PlaceItem(cannonTower, new Vector2Int(1, 0));
inventoryGrid.PlaceItem(magicTower, new Vector2Int(3, 0));

// 시너지 체크 (인접한 타워 확인)
foreach (var tower in inventoryGrid.GetPlacedItems())
{
    CheckSynergyWithNeighbors(tower);
}
```

## 🔍 시너지 체크 예시 코드

```csharp
void CheckSynergyWithNeighbors(InventoryItem tower)
{
    Vector2Int pos = tower.gridPosition;
    Vector2Int size = tower.GetRotatedSize();
    
    // 인접한 8방향 체크
    Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(-1, 0), new Vector2Int(1, 0),
        new Vector2Int(0, -1), new Vector2Int(0, 1),
        new Vector2Int(-1, -1), new Vector2Int(-1, 1),
        new Vector2Int(1, -1), new Vector2Int(1, 1)
    };
    
    foreach (var dir in directions)
    {
        Vector2Int checkPos = pos + dir;
        InventoryItem neighbor = inventoryGrid.GetItemAtPosition(checkPos);
        
        if (neighbor != null)
        {
            // 시너지 로직 실행
            ApplySynergy(tower, neighbor);
        }
    }
}
```

## 🚀 확장 아이디어

### 1. 결합 시스템
- 동일한 타워 3개 → 업그레이드된 타워로 결합
- 특정 조합 → 새로운 타워 생성

### 2. 드래그 프리뷰
- 드래그 중 배치 가능 여부를 색상으로 표시
- 초록색: 배치 가능
- 빨간색: 배치 불가능

### 3. 자동 정렬
- 빈 공간 자동 채우기
- 최적화된 배치 제안

### 4. 저장/불러오기
```csharp
// 저장
string json = JsonUtility.ToJson(inventoryGrid.GetPlacedItems());
PlayerPrefs.SetString("Inventory", json);

// 불러오기
string json = PlayerPrefs.GetString("Inventory");
List<InventoryItem> items = JsonUtility.FromJson<List<InventoryItem>>(json);
```

## 📝 주의사항

1. **Canvas Scaler 설정**: UI Scale Mode를 "Scale With Screen Size"로 설정 권장
2. **Event System**: Scene에 EventSystem이 있어야 드래그 앤 드롭 작동
3. **Layer 순서**: 아이템이 셀보다 위에 그려지도록 Hierarchy 순서 조정
4. **성능**: 그리드 크기가 클수록 충돌 검사 비용 증가 (최적화 필요 시 QuadTree 사용)

## 🎨 비주얼 개선 아이디어

1. 아이템 호버 시 외곽선 표시
2. 배치 시 애니메이션 효과
3. 시너지 발동 시 연결선 그리기
4. 아이템별 고유 아이콘 추가
5. 툴팁 표시 (아이템 정보)

## 🐛 디버깅 팁

- `PrintGridState()`: 그리드 상태를 콘솔에 출력
- Debug.Log로 모든 주요 동작 추적
- Scene View에서 Gizmos로 그리드 범위 표시 가능
