# 🎒 백팩 히어로/배틀러 스타일 인벤토리 시스템

## 📋 목차
1. [주요 기능](#주요-기능)
2. [빠른 시작](#빠른-시작)
3. [상세 설정 가이드](#상세-설정-가이드)
4. [조작법](#조작법)
5. [고급 기능](#고급-기능)
6. [스크립트 참조](#스크립트-참조)

---

## 🎯 주요 기능

### ✨ 백팩 히어로 스타일 핵심 기능
- ✅ **격자 기반 인벤토리**: 자유로운 아이템 배치
- ✅ **실시간 배치 프리뷰**: 드래그 중 배치 가능/불가능 시각적 피드백
  - 🟢 녹색: 배치 가능
  - 🔴 빨강: 배치 불가능
- ✅ **아이템 회전**:
  - 드래그 중: R키 또는 마우스 휠
  - 배치 후: 우클릭
- ✅ **충돌 감지**: 중복 배치 방지
- ✅ **부드러운 애니메이션**: 배치 시 스케일 애니메이션
- ✅ **마우스 오버 효과**: 아이템 하이라이트
- ✅ **툴팁 시스템**: 아이템 정보 표시 (선택사항)

---

## 🚀 빠른 시작 (5분)

### 1단계: Unity 씬 설정

```
Scene
├─ Main Camera
│   ├─ Position: (3, 3, -10)
│   ├─ Projection: Orthographic
│   └─ Size: 5
│
├─ GridParent (Empty GameObject)
│   └─ SpriteInventoryGrid 컴포넌트
│
└─ (선택) TestManager
    └─ SpriteInventoryTest 컴포넌트
```

### 2단계: Prefab 생성

#### CellPrefab (그리드 셀)
1. Hierarchy에서 우클릭 → 2D Object → Sprites → Square
2. 이름을 "CellPrefab"으로 변경
3. SpriteRenderer 설정:
   - Color: 흰색, Alpha 0.3
   - Sorting Layer: Default
   - Order in Layer: -1
4. Project로 드래그하여 Prefab 생성
5. Hierarchy에서 삭제

#### ItemPrefab (아이템)
1. Hierarchy에서 우클릭 → 2D Object → Sprites → Square
2. 이름을 "ItemPrefab"으로 변경
3. 컴포넌트 추가:
   - **BoxCollider2D** (크기: 1x1)
   - **DraggableSprite** 스크립트
   - **(선택) ItemTooltip** 스크립트
   - **(선택) ItemVisualEffects** 스크립트
4. SpriteRenderer 설정:
   - Color: 흰색
   - Order in Layer: 1
5. Project로 드래그하여 Prefab 생성
6. Hierarchy에서 삭제

### 3단계: SpriteInventoryGrid 설정

GridParent 오브젝트를 선택하고 Inspector에서:

```
Grid Settings:
├─ Grid Width: 6
├─ Grid Height: 6
└─ Cell Size: 1

Visual Settings:
├─ Cell Prefab: [CellPrefab 할당]
├─ Item Prefab: [ItemPrefab 할당]
├─ Cell Color: 흰색 (Alpha 0.3)
├─ Occupied Cell Color: 빨강 (Alpha 0.3)
├─ Valid Placement Color: 녹색 (Alpha 0.5)    ← 새로 추가!
└─ Invalid Placement Color: 빨강 (Alpha 0.5)  ← 새로 추가!
```

### 4단계: 테스트

▶️ Play 버튼을 눌러서 실행하면 6x6 그리드가 표시됩니다!

---

## 📖 상세 설정 가이드

### GridItem (아이템 데이터)

```csharp
GridItem item = new GridItem("Sword", 1, 3);  // 이름, 가로, 세로
item.color = Color.blue;
item.sprite = yourSprite;  // (선택) 커스텀 스프라이트
```

### 프로그래밍 방식으로 아이템 추가

```csharp
public class InventoryManager : MonoBehaviour
{
    public SpriteInventoryGrid grid;
    public Sprite swordSprite;

    void Start()
    {
        // 아이템 생성
        GridItem sword = new GridItem("Long Sword", 1, 3, swordSprite);
        sword.color = new Color(0.8f, 0.8f, 1f);  // 연한 파랑

        // 그리드에 배치
        grid.PlaceItem(sword, new Vector2Int(0, 0));
    }
}
```

### 다양한 크기의 아이템 예시

```csharp
// 백팩 히어로 스타일 아이템들
GridItem potion = new GridItem("Potion", 1, 2);     // 세로로 긴 물약
potion.color = Color.red;

GridItem shield = new GridItem("Shield", 2, 2);     // 정사각형 방패
shield.color = Color.cyan;

GridItem bow = new GridItem("Bow", 1, 4);           // 긴 활
bow.color = Color.green;

GridItem ring = new GridItem("Ring", 1, 1);         // 작은 반지
ring.color = Color.yellow;
```

---

## 🎮 조작법

### 마우스 조작
| 조작 | 기능 |
|------|------|
| **좌클릭 + 드래그** | 아이템 이동 |
| **우클릭** | 배치된 아이템 회전 |
| **마우스 오버** | 아이템 하이라이트 (0.5초 후 툴팁 표시) |

### 드래그 중 조작
| 조작 | 기능 |
|------|------|
| **R 키** | 아이템 회전 (90도 단위) |
| **마우스 휠 위** | 아이템 회전 |
| **마우스 이동** | 실시간 배치 프리뷰 표시 |

### 키보드 단축키 (테스트 모드)
| 키 | 기능 |
|----|------|
| **P** | 그리드 상태 출력 (콘솔) |
| **C** | 모든 아이템 제거 |

---

## 🔧 고급 기능

### 1. 아이템 툴팁 (ItemTooltip)

ItemPrefab에 추가하여 아이템 정보를 표시합니다:

```csharp
// ItemTooltip 설정 예시
itemObject.GetComponent<ItemTooltip>().itemName = "Magic Sword";
itemObject.GetComponent<ItemTooltip>().itemDescription = "강력한 마법이 깃든 검";
itemObject.GetComponent<ItemTooltip>().itemStats = "공격력 +15\n치명타 +5%";
```

**설정값:**
- `showDelay`: 툴팁이 나타나기까지 대기 시간 (기본: 0.3초)
- `tooltipOffset`: 마우스로부터의 오프셋
- `tooltipPrefab`: 커스텀 툴팁 Prefab (TextMeshPro 사용)

### 2. 시각적 효과 (ItemVisualEffects)

아이템 배치 시 애니메이션과 효과를 추가합니다:

```csharp
// 설정 옵션
useScaleAnimation = true;           // 배치 시 스케일 애니메이션
scaleAnimDuration = 0.2f;          // 애니메이션 지속 시간

useHoverEffect = true;              // 마우스 오버 시 밝아짐
hoverBrightness = 1.2f;            // 밝기 배수

useShakeOnInvalidPlacement = true; // 잘못된 배치 시 흔들림
shakeIntensity = 0.1f;             // 흔들림 강도
```

### 3. 동적 그리드 크기 조절

```csharp
// 런타임에 그리드 크기 변경
public class DynamicGrid : MonoBehaviour
{
    public SpriteInventoryGrid grid;

    public void ExpandGrid()
    {
        grid.gridWidth = 8;
        grid.gridHeight = 8;
        // 그리드 재초기화 필요 (Start 다시 호출)
    }
}
```

### 4. 배치 제한 영역

특정 영역에만 아이템 배치를 제한하려면:

```csharp
// CanPlaceItem을 커스터마이징
bool CustomCanPlace(GridItem item, Vector2Int position)
{
    // 예: 상단 2줄은 배치 불가
    if (position.y >= gridHeight - 2)
        return false;

    return grid.CanPlaceItem(item, position);
}
```

---

## 📚 스크립트 참조

### 핵심 스크립트

#### 1. **SpriteInventoryGrid.cs**
그리드 시스템의 핵심 관리자

**주요 메서드:**
```csharp
// 배치 관련
bool PlaceItem(GridItem item, Vector2Int position)
bool RemoveItem(GridItem item, bool destroyVisual = true)
bool MoveItem(GridItem item, Vector2Int newPosition)
bool RotateItem(GridItem item)
bool CanPlaceItem(GridItem item, Vector2Int position)

// 프리뷰 시스템 (NEW!)
void ShowPlacementPreview(GridItem item, Vector3 worldPosition)
void HidePlacementPreview()

// 좌표 변환
Vector3 GridToWorldPosition(Vector2Int gridPos)
Vector2Int WorldToGridPosition(Vector3 worldPos)

// 조회
GridItem GetItemAtPosition(Vector2Int position)
List<GridItem> GetPlacedItems()
```

#### 2. **DraggableSprite.cs**
드래그 앤 드롭 기능

**주요 기능:**
- 마우스 드래그 처리
- 실시간 배치 프리뷰 표시
- 드래그 중 회전 (R키, 마우스 휠)
- 배치 성공/실패 처리

#### 3. **GridItem.cs**
아이템 데이터 구조

**속성:**
```csharp
string itemName;           // 아이템 이름
int width, height;         // 크기
Sprite sprite;             // 스프라이트 (선택)
Color color;               // 색상
Vector2Int gridPosition;   // 현재 그리드 위치
int rotation;              // 회전 각도 (0, 90, 180, 270)
```

**메서드:**
```csharp
Vector2Int GetRotatedSize()  // 회전된 크기 반환
void Rotate()                // 90도 회전
```

#### 4. **ItemTooltip.cs** (선택)
아이템 정보 툴팁

**설정:**
- `itemName`: 표시할 이름
- `itemDescription`: 설명
- `itemStats`: 스탯 정보
- `showDelay`: 표시 지연 시간

#### 5. **ItemVisualEffects.cs** (선택)
시각적 효과

**효과 종류:**
- 배치 시 스케일 애니메이션
- 마우스 오버 하이라이트
- 잘못된 배치 시 흔들림 효과

---

## 🎨 커스터마이징

### 색상 테마 변경

```csharp
// Inspector에서 설정 가능
Cell Color: (1, 1, 1, 0.3)              // 기본 셀
Occupied Cell Color: (1, 0.5, 0.5, 0.3) // 점유된 셀
Valid Placement Color: (0, 1, 0, 0.5)    // 배치 가능 (녹색)
Invalid Placement Color: (1, 0, 0, 0.5)  // 배치 불가 (빨강)
```

### 그리드 외관

```csharp
// CellPrefab의 SpriteRenderer를 수정하여:
// - 테두리만 있는 셀
// - 그라데이션 셀
// - 육각형 셀 등 다양한 모양 가능
```

---

## 🐛 문제 해결

### 드래그가 안 됨
✅ **해결책:**
1. ItemPrefab에 **BoxCollider2D**가 있는지 확인
2. 카메라가 **Orthographic**인지 확인
3. DraggableSprite의 `grid`와 `itemData`가 할당되었는지 확인

### 프리뷰가 안 보임
✅ **해결책:**
1. `Valid/Invalid Placement Color`의 **Alpha 값** 확인 (0.5 권장)
2. ItemPrefab이 올바르게 할당되었는지 확인
3. 카메라 위치 확인 (Z: -10)

### 회전이 안 됨
✅ **해결책:**
1. 드래그 중: **R키** 또는 **마우스 휠 위**로 시도
2. 배치 후: 아이템 위에서 **우클릭**
3. 회전 공간이 충분한지 확인

### 아이템이 그리드 밖으로 나감
✅ **해결책:**
- `CanPlaceItem` 메서드가 경계 체크를 합니다
- 드래그 후 놓으면 자동으로 원래 위치로 복귀

---

## 🎯 백팩 히어로와의 비교

| 기능 | 백팩 히어로 | 이 시스템 |
|------|------------|----------|
| 격자 배치 | ✅ | ✅ |
| 아이템 회전 | ✅ | ✅ (드래그 중 + 배치 후) |
| 실시간 프리뷰 | ✅ | ✅ (녹색/빨강 하이라이트) |
| 드래그 앤 드롭 | ✅ | ✅ |
| 다양한 크기 | ✅ | ✅ |
| 툴팁 | ✅ | ✅ (선택) |
| 애니메이션 | ✅ | ✅ (선택) |
| 시너지 시스템 | ✅ | ✅ (SpriteSynergySystem) |

---

## 📝 다음 단계

### 추가 가능한 기능
1. **아이템 스왑**: 두 아이템 위치 교환
2. **자동 정렬**: 아이템 자동 배치
3. **카테고리 필터**: 무기/방어구/소모품 분리
4. **아이템 강화**: 배치 위치에 따른 보너스
5. **저장/불러오기**: JSON으로 인벤토리 저장

### 성능 최적화
- Object Pooling 적용
- 큰 그리드(10x10 이상)는 시각적 업데이트 최적화

---

## 📄 라이선스 및 크레딧

- Unity 2D 스프라이트 시스템 기반
- 백팩 히어로/배틀러 게임플레이 참조
- 제작: Claude AI Assistant

---

**즐거운 개발 되세요! 🎮**

질문이나 버그 리포트는 이슈로 남겨주세요.
