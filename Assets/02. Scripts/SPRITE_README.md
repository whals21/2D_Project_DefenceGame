# 🎮 2D Sprite 인벤토리 시스템 - 완성!

## ✨ 새로 만든 파일들 (Sprite 버전)

### 핵심 시스템:
✅ **GridItem.cs** - 아이템 데이터 구조  
✅ **SpriteInventoryGrid.cs** - 2D 그리드 시스템  
✅ **DraggableSprite.cs** - 마우스 드래그 앤 드롭  
✅ **SpriteInventoryTest.cs** - 테스트 코드  
✅ **SpriteSynergySystem.cs** - 시너지 체크 + 비주얼  

### 문서:
📄 **SPRITE_GUIDE.md** - 상세한 설정 가이드  

---

## 🚀 30초 빠른 시작

### 1. Unity 설정:
```
Scene
├─ Main Camera (Position: 3,3,-10, Orthographic, Size: 5)
├─ GridParent (Empty GameObject)
│   └─ SpriteInventoryGrid 스크립트
└─ TestManager (Empty GameObject)
    └─ SpriteInventoryTest 스크립트
```

### 2. Prefab 생성:
- **CellPrefab**: 2D Sprite Square (흰색, Alpha 0.3)
- **ItemPrefab**: 2D Sprite Square + BoxCollider2D + DraggableSprite

### 3. Inspector 연결:
- Grid Width/Height: 6
- Cell Size: 1
- Prefab들 연결

### 4. 실행!

---

## 🎯 주요 차이점 (UI vs Sprite)

### ❌ 제거된 것들:
- RectTransform
- Canvas / Canvas Group
- EventSystem
- UI Image

### ✅ 새로 추가된 것들:
- Transform (일반 게임오브젝트)
- SpriteRenderer
- BoxCollider2D
- OnMouseDown/Drag/Up

### 💪 장점:
1. **게임 월드와 완전 통합** - 타워디펜스에 완벽
2. **물리 시스템 사용 가능**
3. **파티클/이펙트 적용 쉬움**
4. **카메라 줌/이동 자유로움**
5. **UI 레이어 분리 불필요**

---

## 🎮 사용법

### 기본:
```csharp
// 아이템 생성
GridItem tower = new GridItem("Arrow Tower", 1, 4);
tower.color = Color.red;

// 배치
inventoryGrid.PlaceItem(tower, new Vector2Int(0, 0));

// 회전/이동/제거
inventoryGrid.RotateItem(tower);
inventoryGrid.MoveItem(tower, new Vector2Int(2, 2));
inventoryGrid.RemoveItem(tower);
```

### 좌표 변환:
```csharp
// 월드 → 그리드
Vector2Int gridPos = inventoryGrid.WorldToGridPosition(worldPos);

// 그리드 → 월드
Vector3 worldPos = inventoryGrid.GridToWorldPosition(gridPos);
```

### 마우스로 배치:
```csharp
Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
Vector2Int gridPos = inventoryGrid.WorldToGridPosition(mouseWorld);

if (inventoryGrid.CanPlaceItem(item, gridPos))
{
    inventoryGrid.PlaceItem(item, gridPos);
}
```

---

## 🔧 타워디펜스 적용 예시

```csharp
public class TowerPlacer : MonoBehaviour
{
    public SpriteInventoryGrid grid;
    public GridItem currentTower;
    
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int gridPos = grid.WorldToGridPosition(mousePos);
            
            if (grid.CanPlaceItem(currentTower, gridPos))
            {
                grid.PlaceItem(currentTower, gridPos);
                SpawnActualTower(gridPos);
            }
        }
    }
    
    void SpawnActualTower(Vector2Int gridPos)
    {
        Vector3 worldPos = grid.GridToWorldPosition(gridPos);
        // 실제 타워 게임오브젝트 생성
        GameObject tower = Instantiate(towerPrefab, worldPos, Quaternion.identity);
    }
}
```

---

## ⌨️ 조작법

**마우스:**
- 좌클릭 드래그: 아이템 이동
- 우클릭: 아이템 회전

**키보드:**
- **R**: 첫 번째 아이템 회전
- **P**: 그리드 상태 출력
- **C**: 모든 아이템 제거
- **S**: 시너지 체크

---

## 💡 다음 단계

1. ✅ **외곽선 추가**: SpriteOutline.shader 사용
2. ✅ **시너지 시스템**: SpriteSynergySystem 사용 (S키)
3. 🔜 **드래그 프리뷰**: 배치 가능/불가능 색상 표시
4. 🔜 **애니메이션**: DOTween으로 부드러운 배치 효과

---

## 📁 전체 파일 목록

### Sprite 버전 (★ 사용):
- GridItem.cs
- SpriteInventoryGrid.cs
- DraggableSprite.cs
- SpriteInventoryTest.cs
- SpriteSynergySystem.cs
- SPRITE_GUIDE.md

### UI 버전 (참고용):
- InventoryItem.cs
- InventoryGrid.cs
- DraggableItem.cs
- InventoryTest.cs
- SynergySystem.cs

### 공통:
- SpriteOutline.shader (외곽선)

---

## 🎨 비주얼 개선

### 외곽선 추가:
1. SpriteOutline.shader 사용
2. Material 생성
3. ItemPrefab의 SpriteRenderer에 적용

### 시너지 연결선:
- S키 누르면 자동으로 노란 선 그려짐
- SpriteSynergySystem이 자동 처리

---

## 🐛 문제 해결

### 드래그가 안 됨
→ BoxCollider2D 확인
→ 카메라가 Orthographic인지 확인

### 아이템이 안 보임
→ 카메라 Position (3, 3, -10) 확인
→ Sorting Order 확인

### 셀이 안 보임
→ Cell Color의 Alpha 값 확인 (0.3)

---

**모든 준비 완료! 이제 Unity에서 테스트하세요!** 🚀

상세한 내용은 SPRITE_GUIDE.md를 참고하세요.
