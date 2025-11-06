# Tower 폴더 스크립트 메커니즘 정리

## 📋 목차
1. [타워 시스템 개요](#1-타워-시스템-개요)
2. [타워 기본 구조](#2-타워-기본-구조)
3. [공격 타워 시스템](#3-공격-타워-시스템)
4. [아이템 타워 시스템](#4-아이템-타워-시스템)
5. [발사체 시스템](#5-발사체-시스템)
6. [시각적 효과](#6-시각적-효과)

---

## 1. 타워 시스템 개요

### 📝 개요
블록 기반 타워 디펜스 게임의 타워 공격 및 버프 시스템입니다. 타워는 크게 **공격 타워**와 **아이템 타워(버프 타워)**로 나뉩니다.

### 🏗️ 타워 시스템 구조

```
TowerBase (추상 베이스 클래스)
    ├── RangeTower_1 (원거리 타워)
    ├── MeleeTower (근접 타워)
    ├── CanonTower (포탑 타워)
    └── GlowTower (특수 타워)

ItemTowerBlock (아이템 타워 - 버프 제공)
    └── ItemTowerPulseEffect (시각 효과)
```

---

## 2. 타워 기본 구조

### 🔑 주요 스크립트

#### **TowerBase.cs**
모든 공격 타워의 추상 베이스 클래스

```csharp
public abstract class TowerBase : MonoBehaviour
{
    // 타워 기본 스탯
    protected float Range = 3f;          // 공격 사거리
    protected float fireRate = 1f;       // 초당 공격 횟수
    protected int damage = 10;           // 공격력
    protected float bulletSpeed = 5f;    // 발사체 속도

    // 발사체 프리팹
    [SerializeField] protected GameObject bulletPrefab;

    // 내부 상태
    private float lastFireTime;
    private List<MonsterBase> enemiesInRange = new List<MonsterBase>();
    private MonsterBase currentTarget;
}
```

**핵심 메서드:**

1. **적 감지 (CircleCollider2D Trigger):**
```csharp
void OnTriggerEnter2D(Collider2D other)
{
    MonsterBase enemy = other.GetComponent<MonsterBase>();
    if (enemy != null && !enemiesInRange.Contains(enemy))
    {
        enemiesInRange.Add(enemy);
        Debug.Log($"✅ {gameObject.name} 감지: {enemy.name}");
    }
}

void OnTriggerExit2D(Collider2D other)
{
    MonsterBase enemy = other.GetComponent<MonsterBase>();
    if (enemy != null)
    {
        enemiesInRange.Remove(enemy);
        if (currentTarget == enemy)
        {
            currentTarget = null; // 타겟 해제
        }
    }
}
```

2. **타겟 선택 및 공격:**
```csharp
void Update()
{
    // 1. 타겟 선택 (가장 가까운 적)
    if (currentTarget == null || !enemiesInRange.Contains(currentTarget))
    {
        currentTarget = GetClosestEnemy();
    }

    // 2. 공격 쿨다운 체크
    if (currentTarget != null && Time.time >= lastFireTime + (1f / fireRate))
    {
        Attack();
        lastFireTime = Time.time;
    }
}

MonsterBase GetClosestEnemy()
{
    if (enemiesInRange.Count == 0) return null;

    MonsterBase closest = null;
    float minDistance = float.MaxValue;

    foreach (MonsterBase enemy in enemiesInRange)
    {
        if (enemy == null) continue;

        float distance = Vector3.Distance(transform.position, enemy.transform.position);
        if (distance < minDistance)
        {
            minDistance = distance;
            closest = enemy;
        }
    }

    return closest;
}
```

3. **추상 메서드:**
```csharp
protected abstract void Attack();  // 각 타워 타입이 구현
```

---

#### **TowerData.cs**
타워 데이터를 정의하는 ScriptableObject

```csharp
[CreateAssetMenu(fileName = "New Tower Data", menuName = "SO/Tower Data")]
public class TowerData : ScriptableObject
{
    [Header("Tower Info")]
    public string towerName;
    public GameObject towerPrefab;
    public System.Type towerType;  // typeof(RangeTower_1) 등

    [Header("Stats")]
    public float Range = 3f;
    public float fireRate = 1f;
    public int damage = 10;
    public float bulletSpeed = 5f;

    [Header("Spawn Weight")]
    public float spawnWeight = 10f;  // 등장 확률 가중치

    [Header("Visuals")]
    public GameObject bulletPrefab;
    public Color towerColor = Color.white;
}
```

**사용 예시:**
- Unity Inspector에서 Right-Click → Create → SO → Tower Data
- 여러 종류의 타워 데이터 생성 (원거리, 근접, 포탑 등)
- TowerBlock에서 towerDataList로 관리

---

#### **TowerSpawner.cs**
테스트용 타워 스폰 시스템

```csharp
[SerializeField] private TowerData[] towerDataList;
[SerializeField] private KeyCode spawnKey = KeyCode.T;

void Update()
{
    if (Input.GetKeyDown(spawnKey))
    {
        SpawnRandomTower();
    }
}

void SpawnRandomTower()
{
    TowerData randomData = GetRandomTowerData();
    Vector3 spawnPos = GetSpawnPosition();

    GameObject towerObj = Instantiate(randomData.towerPrefab, spawnPos, Quaternion.identity);
    TowerBase tower = towerObj.AddComponent(randomData.towerType) as TowerBase;

    // 리플렉션으로 protected 필드 설정
    SetTowerStats(tower, randomData);
}
```

---

## 3. 공격 타워 시스템

### 🔫 타워 타입별 특징

#### **RangeTower_1.cs** - 원거리 타워
가장 기본적인 원거리 공격 타워

```csharp
public class RangeTower_1 : TowerBase
{
    protected override void Attack()
    {
        if (currentTarget == null || bulletPrefab == null)
            return;

        // 1. 발사체 생성
        GameObject bulletObj = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // 2. 발사체 초기화
        Bullet bullet = bulletObj.GetComponent<Bullet>();
        if (bullet != null)
        {
            bullet.Initialize(currentTarget, damage, bulletSpeed);
        }

        Debug.Log($"🎯 {gameObject.name} 발사 → {currentTarget.name}");
    }
}
```

**특징:**
- 단일 타겟 공격
- 발사체 속도 조절 가능
- 기본 데미지

---

#### **MeleeTower.cs** - 근접 타워
근거리 범위 공격 타워

```csharp
public class MeleeTower : TowerBase
{
    [SerializeField] private float splashRadius = 1.5f;  // 범위 공격 반경

    protected override void Attack()
    {
        if (currentTarget == null)
            return;

        // 범위 내 모든 적 공격
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, splashRadius);

        foreach (Collider2D hit in hits)
        {
            MonsterBase enemy = hit.GetComponent<MonsterBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"⚔️ {gameObject.name} 근접 공격 → {enemy.name} (-{damage})");
            }
        }

        // 시각 효과 (선택 사항)
        ShowAttackEffect();
    }

    void ShowAttackEffect()
    {
        // 범위 공격 시각 효과 (원형 이펙트 등)
    }
}
```

**특징:**
- 범위 공격 (AOE)
- 발사체 없음 (즉시 데미지)
- 짧은 사거리, 높은 공격력

---

#### **CanonTower.cs** - 포탑 타워
폭발 발사체를 사용하는 타워

```csharp
public class CanonTower : TowerBase
{
    [SerializeField] private GameObject canonBulletPrefab;  // 포탄 프리팹

    protected override void Attack()
    {
        if (currentTarget == null || canonBulletPrefab == null)
            return;

        // 포탄 발사
        GameObject bulletObj = Instantiate(canonBulletPrefab, transform.position, Quaternion.identity);

        CanonBullet canonBullet = bulletObj.GetComponent<CanonBullet>();
        if (canonBullet != null)
        {
            // 폭발 반경과 함께 초기화
            canonBullet.Initialize(currentTarget, damage, bulletSpeed, explosionRadius: 2f);
        }
    }
}
```

**특징:**
- 폭발 범위 데미지
- 느린 공격 속도, 높은 데미지
- 다수 적 처리에 유리

---

#### **GlowTower.cs** - 특수 효과 타워
슬로우 또는 DOT(Damage Over Time) 효과

```csharp
public class GlowTower : TowerBase
{
    [SerializeField] private float slowEffect = 0.5f;  // 50% 슬로우

    protected override void Attack()
    {
        if (currentTarget == null)
            return;

        // 데미지 + 디버프 적용
        currentTarget.TakeDamage(damage);
        currentTarget.ApplySlow(slowEffect, duration: 2f);

        Debug.Log($"✨ {gameObject.name} 슬로우 공격 → {currentTarget.name}");
    }
}
```

**특징:**
- 적 이동 속도 감소
- 낮은 데미지, 유틸리티 중심
- 다른 타워 보조

---

## 4. 아이템 타워 시스템

### 📝 개요
공격하지 않고 인접한 타워에 버프를 제공하는 보조 타워입니다.

### 🔑 주요 스크립트

#### **ItemTowerData.cs**
아이템 타워 데이터 ScriptableObject

```csharp
[CreateAssetMenu(fileName = "New Item Tower Data", menuName = "SO/Item Tower Data")]
public class ItemTowerData : ScriptableObject
{
    [Header("Item Info")]
    public string itemName = "Buff Item";
    public Color itemColor = Color.yellow;
    public float spawnWeight = 10f;

    [Header("Buff Range")]
    public int buffRange = 2;  // 버프 범위 (그리드 칸 수)

    [Header("Buff Multipliers")]
    public float damageMultiplier = 1.2f;      // 공격력 × 1.2
    public float rangeMultiplier = 1.1f;       // 사거리 × 1.1
    public float fireRateMultiplier = 1.15f;   // 공격속도 × 1.15

    [Header("Visual Effects")]
    public GameObject buffEffectPrefab;  // 버프 이펙트
}
```

---

#### **ItemTowerBlock.cs**
아이템 타워의 핵심 로직

```csharp
[RequireComponent(typeof(Block))]
public class ItemTowerBlock : MonoBehaviour
{
    [SerializeField] private ItemTowerData itemData;

    private Block block;
    private List<TowerBlock> buffedTowers = new List<TowerBlock>();
    private Dictionary<TowerBlock, List<SpriteRenderer>> buffedCellRenderers;
    private Coroutine glowCoroutine;
}
```

**핵심 메서드:**

1. **아이템 타워 활성화:**
```csharp
public void ActivateItemTower()
{
    if (isActive || itemData == null) return;

    isActive = true;

    // 1. 인접한 타워들 찾기
    List<TowerBlock> nearbyTowers = FindNearbyTowers();

    // 2. 각 타워에 버프 적용 + 반짝임 효과
    foreach (TowerBlock tower in nearbyTowers)
    {
        ApplyBuffToTower(tower);
        buffedTowers.Add(tower);

        // 접촉하는 셀 찾아서 반짝이게
        List<SpriteRenderer> contactCells = FindContactCells(tower);
        if (contactCells.Count > 0)
        {
            buffedCellRenderers[tower] = contactCells;
        }
    }

    // 3. 반짝임 코루틴 시작
    if (buffedCellRenderers.Count > 0)
    {
        glowCoroutine = StartCoroutine(GlowEffect());
    }
}
```

2. **상하좌우 인접 타워 찾기:**
```csharp
List<TowerBlock> FindNearbyTowers()
{
    List<TowerBlock> nearbyTowers = new List<TowerBlock>();

    if (!block.isPlacedOnGrid) return nearbyTowers;

    // 아이템 타워가 차지하는 모든 셀
    List<Vector2Int> itemPositions = block.GetWorldCellPositions();

    // 상하좌우 4방향만 체크 (대각선 제외)
    Vector2Int[] directions = new Vector2Int[]
    {
        new Vector2Int(0, 1),   // 위
        new Vector2Int(0, -1),  // 아래
        new Vector2Int(-1, 0),  // 왼쪽
        new Vector2Int(1, 0)    // 오른쪽
    };

    HashSet<Vector2Int> checkedPositions = new HashSet<Vector2Int>();

    foreach (Vector2Int itemPos in itemPositions)
    {
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = itemPos + dir;

            if (checkedPositions.Contains(checkPos) || itemPositions.Contains(checkPos))
                continue;

            checkedPositions.Add(checkPos);

            // 해당 위치의 타워 찾기
            TowerBlock towerBlock = FindTowerAtPosition(checkPos);
            if (towerBlock != null && !nearbyTowers.Contains(towerBlock))
            {
                nearbyTowers.Add(towerBlock);
            }
        }
    }

    return nearbyTowers;
}
```

3. **버프 적용 (리플렉션 사용):**
```csharp
void ApplyBuffToTower(TowerBlock tower)
{
    TowerBase towerBase = tower.GetComponent<TowerBase>();
    if (towerBase == null) return;

    // 리플렉션으로 protected 필드 접근
    var baseType = typeof(TowerBase);

    // 사거리 증가
    var rangeField = baseType.GetField("Range",
        BindingFlags.NonPublic | BindingFlags.Instance);
    if (rangeField != null && itemData.rangeMultiplier != 1f)
    {
        float currentRange = (float)rangeField.GetValue(towerBase);
        float newRange = currentRange * itemData.rangeMultiplier;
        rangeField.SetValue(towerBase, newRange);
        Debug.Log($"📏 사거리: {currentRange:F1} → {newRange:F1}");
    }

    // 공격속도 증가
    var fireRateField = baseType.GetField("fireRate", ...);
    if (fireRateField != null)
    {
        float currentFireRate = (float)fireRateField.GetValue(towerBase);
        float newFireRate = currentFireRate * itemData.fireRateMultiplier;
        fireRateField.SetValue(towerBase, newFireRate);
    }

    // 공격력 증가
    var damageField = baseType.GetField("damage", ...);
    if (damageField != null)
    {
        int currentDamage = (int)damageField.GetValue(towerBase);
        int newDamage = Mathf.RoundToInt(currentDamage * itemData.damageMultiplier);
        damageField.SetValue(towerBase, newDamage);
    }

    // CircleCollider2D 반경도 업데이트
    CircleCollider2D rangeCollider = tower.GetComponent<CircleCollider2D>();
    if (rangeCollider != null)
    {
        rangeCollider.radius *= itemData.rangeMultiplier;
    }
}
```

4. **접촉 셀 찾기 및 반짝임:**
```csharp
List<SpriteRenderer> FindContactCells(TowerBlock tower)
{
    List<SpriteRenderer> contactCells = new List<SpriteRenderer>();

    // 아이템 타워의 셀 위치들
    List<Vector2Int> itemPositions = block.GetWorldCellPositions();

    // 타워 블록의 셀 위치들
    Block towerBlock = tower.GetComponent<Block>();
    List<Vector2Int> towerPositions = towerBlock.GetWorldCellPositions();
    HashSet<Vector2Int> towerPosSet = new HashSet<Vector2Int>(towerPositions);

    // 아이템 타워의 SpriteRenderer들
    SpriteRenderer[] allRenderers = block.GetComponentsInChildren<SpriteRenderer>();

    // 상하좌우 4방향 체크
    Vector2Int[] directions = { (0,1), (0,-1), (-1,0), (1,0) };

    for (int i = 0; i < itemPositions.Count && i < allRenderers.Length; i++)
    {
        Vector2Int itemPos = itemPositions[i];

        // 이 셀이 타워와 인접한지 체크
        bool isContact = false;
        foreach (Vector2Int dir in directions)
        {
            Vector2Int checkPos = itemPos + dir;
            if (towerPosSet.Contains(checkPos))
            {
                isContact = true;
                break;
            }
        }

        // 접촉하는 셀이면 SpriteRenderer 추가
        if (isContact && allRenderers[i] != null)
        {
            contactCells.Add(allRenderers[i]);
        }
    }

    return contactCells;
}
```

5. **반짝임 효과 코루틴:**
```csharp
IEnumerator GlowEffect()
{
    // 원래 색상 저장
    Dictionary<SpriteRenderer, Color> originalColors = new Dictionary<SpriteRenderer, Color>();
    foreach (var kvp in buffedCellRenderers)
    {
        foreach (SpriteRenderer renderer in kvp.Value)
        {
            if (renderer != null)
            {
                originalColors[renderer] = renderer.color;
            }
        }
    }

    while (isActive && buffedCellRenderers.Count > 0)
    {
        // Sine 함수로 알파값 변화 (minAlpha ↔ maxAlpha)
        float alpha = Mathf.Lerp(minAlpha, maxAlpha,
            (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f);

        // 모든 접촉 셀의 알파값 업데이트
        foreach (var kvp in buffedCellRenderers)
        {
            foreach (SpriteRenderer renderer in kvp.Value)
            {
                if (renderer != null && originalColors.ContainsKey(renderer))
                {
                    Color color = originalColors[renderer];
                    color.a = alpha;
                    renderer.color = color;
                }
            }
        }

        yield return null;
    }

    // 원래 색상으로 복원
    foreach (var kvp in originalColors)
    {
        if (kvp.Key != null)
        {
            kvp.Key.color = kvp.Value;
        }
    }
}
```

---

#### **ItemTowerSpawner.cs**
아이템 타워 생성 및 시각 효과 적용

```csharp
void SpawnItemTower(ItemTowerData itemData, Vector3 position)
{
    // 1. 블록 생성
    BlockData randomBlockShape = GetRandomItemShape();  // 1-3칸 블록
    Block block = blockFactory.CreateBlock(randomBlockShape, position);

    // 2. TowerBlock/TowerBase 컴포넌트 제거 (공격 방지)
    TowerBlock existingTowerBlock = block.GetComponent<TowerBlock>();
    if (existingTowerBlock != null)
    {
        DestroyImmediate(existingTowerBlock);
    }

    TowerBase[] towerBases = block.GetComponents<TowerBase>();
    foreach (TowerBase towerBase in towerBases)
    {
        if (towerBase != null)
        {
            DestroyImmediate(towerBase);
        }
    }

    CircleCollider2D circleCollider = block.GetComponent<CircleCollider2D>();
    if (circleCollider != null)
    {
        DestroyImmediate(circleCollider);
    }

    // 3. 시각적 효과 적용
    SpriteRenderer[] cellRenderers = block.GetComponentsInChildren<SpriteRenderer>();
    foreach (SpriteRenderer renderer in cellRenderers)
    {
        // 색상 + 반투명
        Color colorWithAlpha = itemData.itemColor;
        colorWithAlpha.a = 0.85f;  // 85% 불투명도
        renderer.color = colorWithAlpha;

        // 렌더링 순서 변경
        renderer.sortingOrder = 5;
    }

    // 4. ItemTowerBlock 컴포넌트 추가
    ItemTowerBlock itemTowerBlock = block.gameObject.AddComponent<ItemTowerBlock>();
    itemTowerBlock.SetItemData(itemData);

    // 5. 배치 및 활성화
    block.isPlacedOnGrid = true;
    itemTowerBlock.ActivateItemTower();

    // 6. 시각 효과 추가
    AddVisualEffects(block.gameObject, cellRenderers);
}

void AddVisualEffects(GameObject itemTowerObj, SpriteRenderer[] cellRenderers)
{
    // 펄스 효과 컴포넌트 추가
    ItemTowerPulseEffect pulseEffect = itemTowerObj.AddComponent<ItemTowerPulseEffect>();
    pulseEffect.Initialize(cellRenderers);

    // 각 셀에 외곽선 추가
    foreach (SpriteRenderer cellRenderer in cellRenderers)
    {
        if (cellRenderer != null)
        {
            AddOutlineToCell(cellRenderer);
        }
    }
}

void AddOutlineToCell(SpriteRenderer cellRenderer)
{
    // 외곽선용 GameObject 생성
    GameObject outlineObj = new GameObject("Outline");
    outlineObj.transform.SetParent(cellRenderer.transform);
    outlineObj.transform.localPosition = Vector3.zero;
    outlineObj.transform.localScale = Vector3.one * 1.08f;  // 8% 크게

    // SpriteRenderer 복사
    SpriteRenderer outlineRenderer = outlineObj.AddComponent<SpriteRenderer>();
    outlineRenderer.sprite = cellRenderer.sprite;
    outlineRenderer.sortingOrder = cellRenderer.sortingOrder - 1;  // 뒤에

    // 노란빛 외곽선
    outlineRenderer.color = new Color(1f, 1f, 0.5f, 0.4f);
}
```

---

#### **ItemTowerPulseEffect.cs**
아이템 타워의 펄스 애니메이션

```csharp
public class ItemTowerPulseEffect : MonoBehaviour
{
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float scaleAmplitude = 0.05f;       // 5% 크기 변화
    [SerializeField] private float brightnessAmplitude = 0.15f;  // 15% 밝기 변화

    private SpriteRenderer[] cellRenderers;
    private Color[] originalColors;
    private Vector3 originalScale;
    private float timeOffset;

    public void Initialize(SpriteRenderer[] renderers)
    {
        cellRenderers = renderers;
        originalScale = transform.localScale;
        timeOffset = Random.Range(0f, 2f * Mathf.PI);  // 랜덤 시작 위상

        // 원래 색상 저장
        originalColors = new Color[cellRenderers.Length];
        for (int i = 0; i < cellRenderers.Length; i++)
        {
            if (cellRenderers[i] != null)
            {
                originalColors[i] = cellRenderers[i].color;
            }
        }
    }

    void Update()
    {
        if (cellRenderers == null || cellRenderers.Length == 0) return;

        // 1. 스케일 펄스 (크기 변화)
        float scaleMultiplier = 1f + Mathf.Sin((Time.time + timeOffset) * pulseSpeed) * scaleAmplitude;
        transform.localScale = originalScale * scaleMultiplier;

        // 2. 밝기 펄스 (색상 밝기 변화)
        float brightness = 1f + Mathf.Sin((Time.time + timeOffset) * pulseSpeed * 1.5f) * brightnessAmplitude;

        for (int i = 0; i < cellRenderers.Length; i++)
        {
            if (cellRenderers[i] != null && originalColors != null && i < originalColors.Length)
            {
                Color newColor = originalColors[i] * brightness;
                newColor.a = originalColors[i].a;  // 알파값 유지
                cellRenderers[i].color = newColor;
            }
        }
    }

    void OnDestroy()
    {
        // 원래 상태로 복원
        if (originalScale != Vector3.zero)
        {
            transform.localScale = originalScale;
        }

        if (cellRenderers != null && originalColors != null)
        {
            for (int i = 0; i < cellRenderers.Length; i++)
            {
                if (cellRenderers[i] != null && i < originalColors.Length)
                {
                    cellRenderers[i].color = originalColors[i];
                }
            }
        }
    }
}
```

---

## 5. 발사체 시스템

### 🔑 주요 스크립트

#### **Bullet.cs**
기본 발사체 (직선 이동)

```csharp
public class Bullet : MonoBehaviour
{
    private MonsterBase target;
    private int damage;
    private float speed;

    public void Initialize(MonsterBase targetEnemy, int bulletDamage, float bulletSpeed)
    {
        target = targetEnemy;
        damage = bulletDamage;
        speed = bulletSpeed;
    }

    void Update()
    {
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            Destroy(gameObject);
            return;
        }

        // 타겟을 향해 이동
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // 타겟에 도달했는지 체크
        float distance = Vector3.Distance(transform.position, target.transform.position);
        if (distance < 0.1f)
        {
            HitTarget();
        }
    }

    void HitTarget()
    {
        if (target != null)
        {
            target.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
```

**특징:**
- 단순 직선 추적
- 타겟 사망 시 소멸
- 도달 시 데미지 및 파괴

---

#### **CanonBullet.cs**
포탄 발사체 (포물선 + 폭발)

```csharp
public class CanonBullet : MonoBehaviour
{
    private MonsterBase target;
    private int damage;
    private float speed;
    private float explosionRadius;

    [SerializeField] private GameObject explosionEffectPrefab;

    public void Initialize(MonsterBase targetEnemy, int bulletDamage, float bulletSpeed, float explosionRadius)
    {
        target = targetEnemy;
        damage = bulletDamage;
        speed = bulletSpeed;
        this.explosionRadius = explosionRadius;
    }

    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        // 타겟을 향해 이동 (직선이지만 더 느림)
        Vector3 direction = (target.transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // 회전 (포탄이 날아가는 방향)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 도달 체크
        if (Vector3.Distance(transform.position, target.transform.position) < 0.2f)
        {
            Explode();
        }
    }

    void Explode()
    {
        // 폭발 이펙트 생성
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 폭발 범위 내 모든 적에게 데미지
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D hit in hits)
        {
            MonsterBase enemy = hit.GetComponent<MonsterBase>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
                Debug.Log($"💥 폭발 데미지 → {enemy.name} (-{damage})");
            }
        }

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // 폭발 범위 시각화
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
```

**특징:**
- 범위 폭발 데미지
- 다수 적 동시 공격
- 시각 효과 지원

---

## 6. 시각적 효과

### 🔑 주요 기법

#### 1. **반짝임 효과 (Glow Effect)**
```csharp
// Sine 함수를 사용한 부드러운 알파값 변화
float alpha = Mathf.Lerp(minAlpha, maxAlpha,
    (Mathf.Sin(Time.time * glowSpeed) + 1f) / 2f);

renderer.color = new Color(r, g, b, alpha);
```

**원리:**
- `Mathf.Sin()`: -1 ~ 1 사이 값 반환
- `+ 1f`: 0 ~ 2 범위로 변환
- `/ 2f`: 0 ~ 1 범위로 정규화
- `Mathf.Lerp()`: minAlpha ~ maxAlpha 사이로 매핑

---

#### 2. **펄스 효과 (Pulse Effect)**
```csharp
// 스케일 펄스
float scaleMultiplier = 1f + Mathf.Sin(Time.time * pulseSpeed) * scaleAmplitude;
transform.localScale = originalScale * scaleMultiplier;

// 밝기 펄스
float brightness = 1f + Mathf.Sin(Time.time * pulseSpeed * 1.5f) * brightnessAmplitude;
renderer.color = originalColor * brightness;
```

**특징:**
- 크기와 밝기가 동시에 변화
- 서로 다른 속도로 변화 (시각적 다양성)
- 원래 상태 저장 및 복원

---

#### 3. **외곽선 효과 (Outline Effect)**
```csharp
// 같은 스프라이트를 8% 크게 뒤에 배치
GameObject outline = new GameObject("Outline");
outline.transform.localScale = Vector3.one * 1.08f;

SpriteRenderer outlineRenderer = outline.AddComponent<SpriteRenderer>();
outlineRenderer.sprite = cellRenderer.sprite;
outlineRenderer.sortingOrder = cellRenderer.sortingOrder - 1;  // 뒤에
outlineRenderer.color = new Color(1f, 1f, 0.5f, 0.4f);  // 노란빛
```

**원리:**
- 원본 스프라이트를 복제
- 약간 크게 만들어서 뒤에 배치
- 다른 색상 적용 → 외곽선처럼 보임

---

#### 4. **발사체 회전**
```csharp
// 이동 방향으로 회전
Vector3 direction = (target.position - transform.position).normalized;
float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
transform.rotation = Quaternion.Euler(0, 0, angle);
```

**원리:**
- `Mathf.Atan2()`: 벡터의 각도 계산 (라디안)
- `Mathf.Rad2Deg`: 라디안 → 도 변환
- Z축 회전 적용 (2D)

---

## 📊 타워 시스템 흐름도

```
블록 배치
    ↓
GridMapManager.OnBlockPlaced()
    ↓
    ├─→ ItemTowerBlock 감지?
    │       ↓ YES
    │   ItemTowerBlock.ActivateItemTower()
    │       ↓
    │   FindNearbyTowers() (상하좌우 4방향)
    │       ↓
    │   ApplyBuffToTower() (리플렉션)
    │       ↓
    │   FindContactCells()
    │       ↓
    │   GlowEffect() 코루틴 시작
    │       ↓
    │   ItemTowerPulseEffect 추가
    │
    └─→ TowerBlock 감지?
            ↓ YES
        TowerBlock.ActivateTower()
            ↓
        랜덤 TowerData 선택
            ↓
        TowerBase 컴포넌트 추가 (RangeTower_1 등)
            ↓
        리플렉션으로 스탯 설정
            ↓
        CircleCollider2D 추가 (사거리)
            ↓
        Update() 루프 시작
            ↓
        OnTriggerEnter2D() → 적 감지
            ↓
        GetClosestEnemy() → 타겟 선택
            ↓
        공격 쿨다운 체크
            ↓
        Attack() → 발사체 생성
            ↓
        Bullet.Update() → 타겟 추적
            ↓
        HitTarget() → 데미지 적용
```

---

## 🔧 주요 디자인 패턴

### 1. **상속 및 추상 클래스 (Inheritance & Abstract Class)**
```csharp
TowerBase (abstract)
    └── RangeTower_1, MeleeTower, CanonTower, GlowTower

protected abstract void Attack();  // 각 타워가 구현
```

**장점:**
- 공통 로직 재사용 (적 감지, 타겟 선택)
- 타워 타입별 고유 공격 방식 구현

---

### 2. **ScriptableObject 패턴**
```csharp
TowerData, ItemTowerData (ScriptableObject)
```

**장점:**
- 데이터와 로직 분리
- Unity Inspector에서 쉽게 편집
- 메모리 효율적 (인스턴스 공유)

---

### 3. **초기화 패턴 (Initialize Pattern)**
```csharp
bullet.Initialize(target, damage, speed);
itemTowerBlock.SetItemData(itemData);
pulseEffect.Initialize(cellRenderers);
```

**장점:**
- Awake/Start보다 명시적
- 생성 시점에 필요한 데이터 전달
- 외부에서 제어 가능

---

### 4. **리플렉션 (Reflection)**
```csharp
var field = typeof(TowerBase).GetField("Range",
    BindingFlags.NonPublic | BindingFlags.Instance);
field.SetValue(tower, newValue);
```

**용도:**
- protected 필드 접근
- TowerData → TowerBase 스탯 설정
- ItemTowerBlock → 버프 적용

**주의:**
- 성능 오버헤드 (초기화 시에만 사용)
- 타입 안정성 낮음 (오타 주의)

---

## 💡 핵심 알고리즘

### 1. **타겟 선택 - 최근접 적**
```csharp
MonsterBase GetClosestEnemy()
{
    MonsterBase closest = null;
    float minDistance = float.MaxValue;

    foreach (MonsterBase enemy in enemiesInRange)
    {
        float distance = Vector3.Distance(transform.position, enemy.transform.position);
        if (distance < minDistance)
        {
            minDistance = distance;
            closest = enemy;
        }
    }

    return closest;
}
```

**시간 복잡도:** O(n)

**다른 타겟팅 전략:**
- 최전방 적 (목표에 가까운 적)
- 체력 낮은 적
- 체력 높은 적

---

### 2. **상하좌우 인접 체크**
```csharp
Vector2Int[] directions = { (0,1), (0,-1), (-1,0), (1,0) };

foreach (Vector2Int itemPos in itemPositions)
{
    foreach (Vector2Int dir in directions)
    {
        Vector2Int checkPos = itemPos + dir;
        // 해당 위치에 타워가 있는지 확인
    }
}
```

**시간 복잡도:** O(4 × 아이템셀수 × 전체블록수)

**최적화:**
- HashSet으로 중복 체크 방지
- GridMap에서 직접 조회 (Dictionary)

---

### 3. **Sine 기반 애니메이션**
```csharp
// 0 ~ 1 범위의 부드러운 값
float t = (Mathf.Sin(Time.time * speed) + 1f) / 2f;

// minValue ~ maxValue 사이 보간
float value = Mathf.Lerp(minValue, maxValue, t);
```

**특징:**
- 자연스러운 반복 움직임
- 시작/끝 속도 부드러움
- 주기 조절 가능 (speed)

---

## 🎯 최적화 포인트

### 1. **CircleCollider2D Trigger 사용**
- Physics2D.OverlapCircle 대신 OnTriggerEnter2D 사용
- 매 프레임 검색 불필요 → 진입/퇴장만 감지

### 2. **리플렉션 최소화**
- 초기화 시에만 사용 (ActivateTower, ApplyBuff)
- Update() 루프에서 사용하지 않음

### 3. **List 대신 배열 (SpriteRenderer[])**
- 고정 크기 데이터
- 메모리 연속 배치 → 캐시 효율

### 4. **Coroutine 활용**
- GlowEffect(): 매 프레임 알파값 변경
- Update()보다 가독성 좋음
- yield return null로 성능 제어

### 5. **DestroyImmediate 사용**
- ItemTowerSpawner에서 TowerBlock 제거 시
- Destroy()는 프레임 끝까지 대기 → 의도치 않은 동작

---

## 📚 참고 자료

- **Unity Physics2D**: https://docs.unity3d.com/Manual/Physics2DReference.html
- **Reflection in C#**: https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/reflection
- **Coroutines**: https://docs.unity3d.com/Manual/Coroutines.html
- **ScriptableObject**: https://docs.unity3d.com/Manual/class-ScriptableObject.html

---

## ✅ 학습 체크리스트

- [ ] TowerBase 추상 클래스 구조 이해
- [ ] OnTriggerEnter2D를 통한 적 감지 메커니즘
- [ ] 리플렉션으로 protected 필드 접근 방법
- [ ] 상하좌우 인접 체크 알고리즘
- [ ] Coroutine을 활용한 애니메이션
- [ ] Sine 함수 기반 부드러운 변화 구현
- [ ] ScriptableObject를 통한 데이터 관리
- [ ] CircleCollider2D Trigger 최적화
- [ ] 아이템 타워 버프 시스템 전체 흐름
- [ ] DestroyImmediate vs Destroy 차이점

---

**작성일:** 2025-01-10
**버전:** 1.0
**작성자:** Claude Code Assistant
