# 백팩히어로 스타일 인벤토리 - 빠른 시작 가이드

## 📦 파일 구성

1. **InventoryItem.cs** - 아이템 데이터 구조
2. **InventoryGrid.cs** - 그리드 관리 시스템 (핵심)
3. **DraggableItem.cs** - 드래그 앤 드롭 기능
4. **InventoryTest.cs** - 테스트 예제
5. **SynergySystem.cs** - 시너지 체크 시스템
6. **README.md** - 상세 문서

## ⚡ 30초 빠른 설정

### 1. Unity에서 Canvas 생성
```
Canvas
└─ GridParent (Empty GameObject with RectTransform)
```

### 2. 스크립트 추가
- GridParent에 `InventoryGrid.cs` 추가
- 새 GameObject에 `InventoryTest.cs` 추가

### 3. 프리팹 생성

**CellPrefab:**
- UI Image (흰색, Alpha 0.3)
- 크기는 코드에서 자동 설정

**ItemPrefab:**
- UI Image
- UI Text (자식)
- CanvasGroup 컴포넌트
- DraggableItem 스크립트

### 4. Inspector 설정
- InventoryGrid 컴포넌트:
  - Grid Width/Height: 6
  - Cell Size: 80
  - Grid Parent, Cell Prefab, Item Prefab 연결

- InventoryTest 컴포넌트:
  - Inventory Grid 연결

### 5. 실행!
Play 버튼을 누르면 자동으로 테스트 아이템이 배치됩니다.

## 🎮 조작법

- **좌클릭 드래그**: 아이템 이동
- **우클릭**: 아이템 회전
- **R키**: 첫 번째 아이템 회전
- **P키**: 그리드 상태 출력
- **C키**: 모든 아이템 제거
- **S키**: 시너지 체크

## 💡 핵심 코드 예제

```csharp
// 아이템 생성
InventoryItem sword = new InventoryItem("Sword", 1, 4);
sword.color = Color.red;

// 배치
inventoryGrid.PlaceItem(sword, new Vector2Int(0, 0));

// 회전
inventoryGrid.RotateItem(sword);

// 이동
inventoryGrid.MoveItem(sword, new Vector2Int(2, 2));

// 제거
inventoryGrid.RemoveItem(sword);
```

## 🎯 타워디펜스 적용 예시

```csharp
// 타워 생성
InventoryItem arrowTower = new InventoryItem("Arrow Tower", 1, 1);
InventoryItem cannonTower = new InventoryItem("Cannon Tower", 2, 2);

// 필드 배치
inventoryGrid.PlaceItem(arrowTower, new Vector2Int(0, 0));
inventoryGrid.PlaceItem(cannonTower, new Vector2Int(2, 0));

// 인접 타워 찾기
List<InventoryItem> neighbors = GetAdjacentItems(arrowTower);

// 시너지 적용
foreach (var neighbor in neighbors)
{
    ApplySynergy(arrowTower, neighbor);
}
```

## 🔧 커스터마이징

### 그리드 크기 변경
```csharp
gridWidth = 8;
gridHeight = 8;
cellSize = 60f;
```

### 새로운 블록 형태 추가
```csharp
// L자 블록 (3x2에서 한 칸 빠짐) - 커스텀 로직 필요
// T자 블록 (3x2에서 코너 빠짐) - 커스텀 로직 필요
// 현재는 직사각형만 지원
```

### 시너지 규칙 추가
```csharp
synergyRules.Add(new SynergyRule
{
    name = "연쇄 반응",
    item1Type = "Magic",
    item2Type = "Magic",
    description = "마법 타워 3개 인접 시 연쇄 공격",
    bonusMultiplier = 2.0f
});
```

## 🐛 문제 해결

### 드래그가 안 됨
→ EventSystem이 Scene에 있는지 확인
→ Canvas에 GraphicRaycaster가 있는지 확인

### 아이템이 안 보임
→ ItemPrefab의 Image 컴포넌트 확인
→ Canvas Render Mode 확인

### 배치가 안 됨
→ Console에서 "Cannot place..." 메시지 확인
→ 그리드 범위 초과 또는 충돌 확인

## 📚 더 알아보기

자세한 내용은 **README.md** 참고!
- 고급 기능 설명
- 확장 아이디어
- 최적화 팁
- 시너지 시스템 상세 설명
