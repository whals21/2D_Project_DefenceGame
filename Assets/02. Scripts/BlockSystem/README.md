# 🎒 백팩 히어로/배틀러 스타일 인벤토리 시스템

Unity 2D용 격자 기반 인벤토리 시스템 - 백팩 히어로, 백팩 배틀러 스타일 완벽 구현

![Version](https://img.shields.io/badge/version-2.0-blue)
![Unity](https://img.shields.io/badge/Unity-2020.3+-green)
![License](https://img.shields.io/badge/license-MIT-yellow)

---

## 🌟 주요 특징

### ✨ 백팩 히어로 핵심 기능
- ✅ **격자 기반 인벤토리** - 자유로운 아이템 배치
- ✅ **실시간 배치 프리뷰** - 녹색/빨강 시각적 피드백
- ✅ **드래그 중 회전** - R키 또는 마우스 휠
- ✅ **다양한 크기** - 1x1부터 NxM까지
- ✅ **충돌 감지** - 스마트한 배치 제한
- ✅ **부드러운 애니메이션** - 배치/호버 효과
- ✅ **툴팁 시스템** - 아이템 정보 표시

---

## 📁 파일 구조

```
BlockSystem/
├── 📄 BackpackInventoryTest.cs      - 테스트 스크립트 (다양한 아이템 예제)
├── 📄 ItemTooltip.cs                - 아이템 정보 툴팁
├── 📄 ItemVisualEffects.cs          - 시각 효과 (애니메이션, 호버)
├── 📘 QUICK_START.md                - 5분 빠른 시작 가이드 ⭐
├── 📘 BACKPACK_SYSTEM_GUIDE.md      - 전체 상세 가이드
└── 📘 README.md                     - 이 파일

../02. Scripts/
├── ✨ SpriteInventoryGrid.cs        - 그리드 시스템 (프리뷰 추가)
├── ✨ DraggableSprite.cs            - 드래그 앤 드롭 (회전 개선)
└── 📄 GridItem.cs                   - 아이템 데이터 구조
```

---

## 🚀 빠른 시작

### 1. Prefab 생성

#### CellPrefab (그리드 셀)
```
2D Object → Sprites → Square
├─ 이름: "CellPrefab"
├─ Color: 흰색 (Alpha 0.3)
└─ Order in Layer: -1
```

#### ItemPrefab (아이템)
```
2D Object → Sprites → Square
├─ 이름: "ItemPrefab"
├─ Add Component:
│  ├─ BoxCollider2D
│  ├─ DraggableSprite
│  ├─ ItemVisualEffects (선택)
│  └─ ItemTooltip (선택)
└─ Order in Layer: 1
```

### 2. 씬 설정

```
Main Camera
├─ Position: (3, 3, -10)
├─ Projection: Orthographic
└─ Size: 5

GridParent (Empty GameObject)
└─ SpriteInventoryGrid
   ├─ Grid Width: 6
   ├─ Grid Height: 6
   ├─ Cell Prefab: [CellPrefab]
   └─ Item Prefab: [ItemPrefab]

TestManager (Empty GameObject)
└─ BackpackInventoryTest
   └─ Grid: [GridParent]
```

### 3. 실행!

▶️ Play 버튼 → 자동으로 테스트 아이템 생성!

**더 자세한 내용:** [QUICK_START.md](QUICK_START.md)

---

## 🎮 사용법

### 기본 조작
| 조작 | 기능 |
|------|------|
| 좌클릭 + 드래그 | 아이템 이동 |
| 드래그 중 R키 | 회전 (90도) |
| 드래그 중 마우스 휠 | 회전 |
| 우클릭 | 배치된 아이템 회전 |
| 마우스 오버 | 하이라이트 + 툴팁 |

### 프리뷰 시스템 (NEW!)
- 🟢 **녹색**: 배치 가능한 위치
- 🔴 **빨강**: 배치 불가능한 위치
- 드래그하면 **자동으로 표시**

### 코드 예제

```csharp
public class MyGame : MonoBehaviour
{
    public SpriteInventoryGrid grid;

    void Start()
    {
        // 아이템 생성
        GridItem sword = new GridItem("Magic Sword", 1, 3);
        sword.color = Color.cyan;

        // 배치
        if (grid.CanPlaceItem(sword, new Vector2Int(0, 0)))
        {
            grid.PlaceItem(sword, new Vector2Int(0, 0));
        }
    }
}
```

---

## 📚 API 레퍼런스

### SpriteInventoryGrid

#### 배치 관련
```csharp
bool PlaceItem(GridItem item, Vector2Int position)
bool RemoveItem(GridItem item, bool destroyVisual = true)
bool MoveItem(GridItem item, Vector2Int newPosition)
bool RotateItem(GridItem item)
bool CanPlaceItem(GridItem item, Vector2Int position)
```

#### 프리뷰 시스템 (NEW!)
```csharp
void ShowPlacementPreview(GridItem item, Vector3 worldPosition)
void HidePlacementPreview()
```

#### 좌표 변환
```csharp
Vector3 GridToWorldPosition(Vector2Int gridPos)
Vector2Int WorldToGridPosition(Vector3 worldPos)
```

#### 조회
```csharp
GridItem GetItemAtPosition(Vector2Int position)
GridItem GetItemAtWorldPosition(Vector3 worldPos)
List<GridItem> GetPlacedItems()
```

### GridItem

```csharp
// 생성자
GridItem(string name, int width, int height, Sprite sprite = null)

// 속성
string itemName
int width, height
Sprite sprite
Color color
Vector2Int gridPosition
int rotation  // 0, 90, 180, 270

// 메서드
Vector2Int GetRotatedSize()
void Rotate()
```

---

## 🎨 커스터마이징

### 색상 설정

Inspector에서 다음 색상 조정 가능:
- **Cell Color**: 빈 셀 색상 (기본: 흰색, Alpha 0.3)
- **Occupied Cell Color**: 점유된 셀 (기본: 빨강, Alpha 0.3)
- **Valid Placement Color**: 배치 가능 (기본: 녹색, Alpha 0.5) ⭐
- **Invalid Placement Color**: 배치 불가 (기본: 빨강, Alpha 0.5) ⭐

### 애니메이션 설정

ItemVisualEffects 컴포넌트:
```csharp
useScaleAnimation = true;         // 배치 시 애니메이션
scaleAnimDuration = 0.2f;        // 지속 시간

useHoverEffect = true;            // 호버 효과
hoverBrightness = 1.2f;          // 밝기

useShakeOnInvalidPlacement = true; // 흔들림 효과
shakeIntensity = 0.1f;            // 강도
```

---

## 🆚 백팩 히어로와 비교

| 기능 | 백팩 히어로 | 이 시스템 |
|------|------------|----------|
| 격자 배치 | ✅ | ✅ |
| 아이템 회전 | ✅ | ✅ (개선됨) |
| 실시간 프리뷰 | ✅ | ✅ (녹색/빨강) |
| 드래그 앤 드롭 | ✅ | ✅ |
| 다양한 크기 | ✅ | ✅ (무제한) |
| 툴팁 | ✅ | ✅ (선택) |
| 애니메이션 | ✅ | ✅ (커스터마이징) |
| Unity 통합 | ❌ | ✅ (완벽) |

---

## 🎯 사용 사례

### 타워 디펜스 게임
```csharp
// 타워 배치 시스템
GridItem tower = new GridItem("Arrow Tower", 2, 2);
tower.color = Color.red;
grid.PlaceItem(tower, selectedPosition);
```

### RPG 인벤토리
```csharp
// 무기/방어구 관리
GridItem helmet = new GridItem("Iron Helmet", 2, 2);
GridItem sword = new GridItem("Long Sword", 1, 3);
GridItem potion = new GridItem("HP Potion", 1, 1);
```

### 퍼즐 게임
```csharp
// 테트리스 스타일 배치 퍼즐
GridItem piece = new GridItem("L-Piece", 2, 3);
piece.Rotate();  // 90도 회전
```

---

## 🐛 문제 해결

| 문제 | 해결책 |
|------|--------|
| 드래그가 안 됨 | BoxCollider2D 확인, Camera Orthographic 확인 |
| 프리뷰가 안 보임 | Valid/Invalid Color의 Alpha 값 확인 (0.5 권장) |
| 회전이 안 됨 | 드래그 중: R키, 배치 후: 우클릭 |
| 아이템이 안 보임 | Camera Position 확인 (Z: -10) |

**더 많은 해결책:** [BACKPACK_SYSTEM_GUIDE.md](BACKPACK_SYSTEM_GUIDE.md#-문제-해결)

---

## 📖 전체 문서

1. **[QUICK_START.md](QUICK_START.md)** - 5분 빠른 시작 ⭐
2. **[BACKPACK_SYSTEM_GUIDE.md](BACKPACK_SYSTEM_GUIDE.md)** - 전체 상세 가이드
3. **[README.md](README.md)** - 이 파일 (개요)

---

## 🔄 업데이트 내역

### v2.0 (최신) - 백팩 히어로 스타일
- ✨ 실시간 배치 프리뷰 시스템 추가
- ✨ 드래그 중 회전 기능 (R키, 마우스 휠)
- ✨ 시각적 피드백 강화 (녹색/빨강)
- ✨ 부드러운 애니메이션 추가
- ✨ 툴팁 시스템 추가
- 🔧 DraggableSprite 개선
- 📚 전체 문서 재작성

### v1.0 - 기본 그리드 시스템
- ✅ 격자 기반 배치
- ✅ 드래그 앤 드롭
- ✅ 아이템 회전 (우클릭)
- ✅ 충돌 감지

---

## 💡 다음 개발 계획

### 계획 중인 기능
- [ ] 아이템 스왑 (두 아이템 위치 교환)
- [ ] 자동 정렬 알고리즘
- [ ] 카테고리 필터링
- [ ] 저장/불러오기 (JSON)
- [ ] Object Pooling 최적화

---

## 🤝 기여

버그 리포트, 기능 제안 환영합니다!

---

## 📄 라이선스

MIT License - 자유롭게 사용하세요!

---

## 🎉 완성!

이제 백팩 히어로/배틀러 스타일의 완벽한 인벤토리 시스템을 사용할 수 있습니다!

**시작하기:** [QUICK_START.md](QUICK_START.md)

**즐거운 개발 되세요!** 🚀
