# 2D Sprite 인벤토리 시스템 - 빠른 설정 가이드

## 📦 파일 구성

**새로운 Sprite 버전:**
- **GridItem.cs** - 아이템 데이터
- **SpriteInventoryGrid.cs** - 그리드 시스템 (핵심)
- **DraggableSprite.cs** - 드래그 앤 드롭
- **SpriteInventoryTest.cs** - 테스트 스크립트

## ⚡ 5분 설정 가이드

### 1. Hierarchy 구조 (간단!)

```
Scene
├─ Main Camera (Z: -10)
├─ GridParent (Empty GameObject, Position: 0,0,0)
│   └─ SpriteInventoryGrid 스크립트 추가
└─ TestManager (Empty GameObject)
    └─ SpriteInventoryTest 스크립트 추가
```

**중요**: GridParent는 그냥 **Empty GameObject**입니다! (UI 아님)

---

### 2. Prefab 생성

#### CellPrefab (그리드 셀)
1. GameObject → 2D Object → **Sprites → Square** 생성
2. 이름: CellPrefab
3. SpriteRenderer:
   - Color: 흰색, Alpha 0.3
   - Sorting Layer: Default
   - Order in Layer: -1
4. Prefab으로 만들고 Scene에서 삭제

#### ItemPrefab (아이템)
1. GameObject → 2D Object → **Sprites → Square** 생성
2. 이름: ItemPrefab
3. **BoxCollider2D** 추가
   - Size: (1, 1)
4. **DraggableSprite** 스크립트 추가
5. SpriteRenderer:
   - Sorting Layer: Default
   - Order in Layer: 1
6. Prefab으로 만들고 Scene에서 삭제

---

### 3. Inspector 설정

#### GridParent의 SpriteInventoryGrid:
```
Grid Width: 6
Grid Height: 6
Cell Size: 1
Cell Prefab: CellPrefab
Item Prefab: ItemPrefab
Cell Color: 흰색, Alpha 0.3
Occupied Cell Color: 빨강, Alpha 0.3
```

#### TestManager의 SpriteInventoryTest:
```
Inventory Grid: GridParent의 SpriteInventoryGrid 연결
```

---

### 4. 카메라 설정

Main Camera:
- Position: (3, 3, -10)  // 그리드 중앙을 보도록
- Projection: Orthographic
- Size: 5

---

### 5. 실행!

Play 버튼 → 자동으로 아이템 배치됨

---

## 🎮 조작법

### 마우스:
- **좌클릭 드래그**: 아이템 이동
- **우클릭**: 아이템 회전

### 키보드:
- **R**: 첫 번째 아이템 회전
- **P**: 그리드 상태 콘솔 출력
- **C**: 모든 아이템 제거

---

## 💻 코드 사용법

```csharp
// 아이템 생성
GridItem sword = new GridItem("Sword", 1, 4);
sword.color = Color.red;

// 배치
inventoryGrid.PlaceItem(sword, new Vector2Int(0, 0));

// 회전
inventoryGrid.RotateItem(sword);

// 이동
inventoryGrid.MoveItem(sword, new Vector2Int(2, 2));

// 제거
inventoryGrid.RemoveItem(sword);

// 월드 좌표 → 그리드 좌표 변환
Vector2Int gridPos = inventoryGrid.WorldToGridPosition(worldPosition);

// 그리드 좌표 → 월드 좌표 변환
Vector3 worldPos = inventoryGrid.GridToWorldPosition(gridPos);

// 특정 위치의 아이템 가져오기
GridItem item = inventoryGrid.GetItemAtWorldPosition(mouseWorldPos);
```

---

## 🔧 커스터마이징

### 그리드 크기 변경:
```csharp
gridWidth = 8;
gridHeight = 8;
cellSize = 1.5f;  // 셀 크기를 크게
```

### 카메라 위치 자동 조정:
```csharp
void Start()
{
    Camera.main.transform.position = new Vector3(
        gridWidth * cellSize / 2f,
        gridHeight * cellSize / 2f,
        -10f
    );
    Camera.main.orthographicSize = Mathf.Max(gridWidth, gridHeight) * cellSize / 2f + 1f;
}
```

---

## 🎯 타워디펜스 적용

```csharp
// 타워 배치
GridItem arrowTower = new GridItem("Arrow Tower", 1, 1);
arrowTower.color = Color.red;

Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
Vector2Int gridPos = inventoryGrid.WorldToGridPosition(mouseWorldPos);

if (inventoryGrid.CanPlaceItem(arrowTower, gridPos))
{
    inventoryGrid.PlaceItem(arrowTower, gridPos);
    
    // 타워 로직 추가
    SpawnTower(arrowTower);
}
```

---

## 🐛 문제 해결

### 드래그가 안 됨
→ ItemPrefab에 **BoxCollider2D**가 있는지 확인
→ 카메라가 Orthographic인지 확인

### 아이템이 안 보임
→ 카메라 Position과 Size 확인
→ Sorting Order 확인 (Item: 1, Cell: -1)

### 클릭 감지가 안 됨
→ Physics 2D Raycaster가 아니라 **Collider2D**로 동작함
→ EventSystem 필요 없음!

---

## 💡 UI vs Sprite 비교

### UI 버전 장점:
- RectTransform으로 정렬 쉬움
- EventSystem으로 입력 쉬움

### **Sprite 버전 장점 (현재):**
- ✅ 게임 월드와 통합
- ✅ 물리 엔진 사용 가능
- ✅ 파티클, 이펙트 적용 쉬움
- ✅ 타워디펜스에 더 적합
- ✅ 카메라 줌/이동 쉬움

---

## 🚀 다음 단계

1. **시너지 시스템** 추가
2. **외곽선 쉐이더** 적용
3. **드래그 프리뷰** (초록/빨강)
4. **애니메이션** 효과

필요하면 말씀하세요!
