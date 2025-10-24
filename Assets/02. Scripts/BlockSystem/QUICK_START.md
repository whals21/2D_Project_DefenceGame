# 🚀 백팩 시스템 5분 시작 가이드

## 📦 새로 추가된 파일들

### BlockSystem 폴더:
```
BlockSystem/
├── 📄 BackpackInventoryTest.cs       - 테스트 스크립트
├── 📄 ItemTooltip.cs                 - 툴팁 시스템
├── 📄 ItemVisualEffects.cs           - 시각 효과
├── 📘 BACKPACK_SYSTEM_GUIDE.md       - 상세 가이드
└── 📘 QUICK_START.md                 - 이 파일
```

### 개선된 기존 파일:
```
02. Scripts/
├── ✨ SpriteInventoryGrid.cs         - 프리뷰 시스템 추가
└── ✨ DraggableSprite.cs             - 드래그 중 회전 추가
```

---

## ⚡ 5분 설정

### 1️⃣ Prefab 생성 (2분)

#### CellPrefab
```
1. Hierarchy 우클릭 → 2D Object → Sprites → Square
2. 이름: "CellPrefab"
3. Inspector:
   └─ SpriteRenderer
      ├─ Color: 흰색, Alpha = 0.3
      └─ Order in Layer: -1
4. Project로 드래그 → Prefab 생성
5. Hierarchy에서 삭제
```

#### ItemPrefab
```
1. Hierarchy 우클릭 → 2D Object → Sprites → Square
2. 이름: "ItemPrefab"
3. Add Component:
   ├─ BoxCollider2D (Size: 1, 1)
   ├─ DraggableSprite
   ├─ ItemVisualEffects (선택)
   └─ ItemTooltip (선택)
4. Inspector:
   └─ SpriteRenderer
      ├─ Color: 흰색
      └─ Order in Layer: 1
5. Project로 드래그 → Prefab 생성
6. Hierarchy에서 삭제
```

### 2️⃣ 씬 설정 (2분)

```
Hierarchy:
├─ Main Camera
│   ├─ Position: (3, 3, -10)
│   ├─ Projection: Orthographic
│   └─ Size: 5
│
├─ GridParent (Create Empty)
│   └─ Add Component: SpriteInventoryGrid
│       ├─ Grid Width: 6
│       ├─ Grid Height: 6
│       ├─ Cell Size: 1
│       ├─ Cell Prefab: [CellPrefab 드래그]
│       ├─ Item Prefab: [ItemPrefab 드래그]
│       └─ Colors (기본값 사용)
│
└─ TestManager (Create Empty)
    └─ Add Component: BackpackInventoryTest
        └─ Grid: [GridParent 드래그]
```

### 3️⃣ 실행! (1분)

▶️ **Play 버튼 클릭**

자동으로 백팩 스타일 아이템들이 생성됩니다:
- 🔴 Small Potion (1x2)
- 🔵 Large Potion (2x2)
- ⚔️ Long Sword (1x3)
- 🛡️ Heavy Shield (2x3)
- 🏹 Longbow (1x4)
- 💍 Rings (1x1 x2)

---

## 🎮 조작법

### 기본 조작
- **좌클릭 + 드래그**: 아이템 이동
- **드래그 중 R키**: 회전
- **드래그 중 마우스 휠**: 회전
- **우클릭**: 배치된 아이템 회전

### 프리뷰 시스템 (NEW!)
- 드래그하면 **자동으로 배치 프리뷰** 표시
- 🟢 **녹색**: 배치 가능한 위치
- 🔴 **빨강**: 배치 불가능한 위치

### 테스트 단축키
- **P**: 그리드 상태 출력
- **C**: 모든 아이템 제거
- **T**: 테스트 아이템 재생성

---

## ✨ 주요 개선 사항

### 백팩 히어로 스타일로 업그레이드된 기능:

1. **실시간 배치 프리뷰** ⭐
   - 드래그 중 배치 가능/불가능 실시간 표시
   - 그리드 셀 하이라이트
   - 아이템 투명 프리뷰

2. **드래그 중 회전** ⭐
   - R키 또는 마우스 휠로 즉시 회전
   - 회전 시 프리뷰 실시간 업데이트

3. **부드러운 애니메이션** (선택)
   - 배치 시 스케일 애니메이션
   - 마우스 오버 하이라이트
   - 잘못된 배치 시 흔들림

4. **툴팁 시스템** (선택)
   - 아이템 정보 표시
   - 마우스 따라다니기

---

## 🎯 코드 사용 예제

### 아이템 추가하기

```csharp
public class MyInventory : MonoBehaviour
{
    public SpriteInventoryGrid grid;

    void Start()
    {
        // 아이템 생성
        GridItem sword = new GridItem("Magic Sword", 1, 3);
        sword.color = Color.cyan;

        // 배치
        grid.PlaceItem(sword, new Vector2Int(0, 0));
    }
}
```

### 배치 가능 여부 확인

```csharp
Vector2Int position = new Vector2Int(2, 2);

if (grid.CanPlaceItem(myItem, position))
{
    Debug.Log("배치 가능!");
    grid.PlaceItem(myItem, position);
}
else
{
    Debug.Log("배치 불가능!");
}
```

### 아이템 이동

```csharp
// 드래그 없이 프로그래밍 방식으로 이동
grid.MoveItem(myItem, new Vector2Int(3, 3));
```

### 프리뷰 수동 제어

```csharp
// 프리뷰 표시
Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
grid.ShowPlacementPreview(myItem, mousePos);

// 프리뷰 숨기기
grid.HidePlacementPreview();
```

---

## 🔧 문제 해결

### ❌ 드래그가 안 됨
- BoxCollider2D 확인
- Camera가 Orthographic인지 확인
- DraggableSprite의 grid, itemData 확인

### ❌ 프리뷰가 안 보임
- Valid/Invalid Placement Color의 Alpha 값 확인
- ItemPrefab 할당 확인

### ❌ 회전이 안 됨
- 드래그 중: R키 또는 마우스 휠 위
- 배치 후: 우클릭
- 회전 공간 확인

---

## 📚 더 자세한 내용

전체 가이드는 [BACKPACK_SYSTEM_GUIDE.md](BACKPACK_SYSTEM_GUIDE.md)를 참조하세요!

### 다루는 내용:
- 🎨 커스터마이징 방법
- 🔧 고급 기능 사용법
- 📖 전체 API 레퍼런스
- 🎯 백팩 히어로와 비교
- 🐛 상세한 문제 해결

---

## 🎉 완료!

이제 백팩 히어로/배틀러 스타일의 인벤토리 시스템을 사용할 수 있습니다!

**주요 특징:**
- ✅ 격자 기반 배치
- ✅ 실시간 프리뷰 (녹색/빨강)
- ✅ 드래그 중 회전
- ✅ 다양한 크기 아이템
- ✅ 충돌 감지
- ✅ 부드러운 애니메이션

**즐거운 개발 되세요!** 🚀
