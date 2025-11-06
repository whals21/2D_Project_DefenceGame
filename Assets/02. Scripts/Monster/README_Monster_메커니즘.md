# Monster 폴더 스크립트 메커니즘 정리

## 📋 목차
1. [몬스터 시스템 개요](#1-몬스터-시스템-개요)
2. [몬스터 기본 구조](#2-몬스터-기본-구조)
3. [경로 이동 시스템](#3-경로-이동-시스템)
4. [몬스터 스폰 시스템](#4-몬스터-스폰-시스템)
5. [데미지 및 체력 시스템](#5-데미지-및-체력-시스템)

---

## 1. 몬스터 시스템 개요

### 📝 개요
몬스터가 지정된 경로를 따라 이동하며, 타워의 공격을 받고, 목표 지점에 도달하면 플레이어에게 피해를 주는 시스템입니다.

### 🏗️ 몬스터 시스템 구조

```
MonsterData (ScriptableObject)
    ↓ (데이터 제공)
MonsterBase (추상 베이스 클래스)
    ├── Monster (기본 몬스터)
    └── NewEnemy (그리드 기반 몬스터)
         ↑
MonsterSpawner (스폰 관리)
```

---

## 2. 몬스터 기본 구조

### 🔑 주요 스크립트

#### **MonsterData.cs**
몬스터의 데이터를 정의하는 ScriptableObject

```csharp
[CreateAssetMenu(fileName = "New Monster Data", menuName = "SO/Monster Data")]
public class MonsterData : ScriptableObject
{
    [Header("Monster Info")]
    public string monsterName = "Monster";
    public GameObject monsterPrefab;
    public Sprite monsterSprite;

    [Header("Stats")]
    public float maxHP = 100f;
    public float moveSpeed = 2f;
    public int damage = 10;          // 플레이어에게 주는 피해
    public int goldReward = 10;      // 처치 시 보상

    [Header("Spawn Settings")]
    public float spawnWeight = 10f;  // 스폰 확률 가중치
}
```

**사용 방법:**
1. Unity Inspector에서 Create → SO → Monster Data
2. 여러 종류의 몬스터 데이터 생성 (일반, 빠른, 탱커 등)
3. MonsterSpawner의 monsterDataList에 등록

---

#### **MonsterBase.cs**
모든 몬스터의 추상 베이스 클래스

```csharp
public abstract class MonsterBase : MonoBehaviour
{
    [Header("Monster Data")]
    protected MonsterData monsterData;

    [Header("Stats")]
    protected float currentHP;
    protected float maxHP;
    protected float moveSpeed;
    protected int damage;
    protected int goldReward;

    [Header("Movement")]
    protected Transform[] waypoints;  // 이동 경로
    protected int currentWaypointIndex = 0;

    [Header("HP Display")]
    [SerializeField] private TMP_Text hpText;  // HP 표시용 TextMeshPro
}
```

**핵심 메서드:**

1. **초기화:**
```csharp
public virtual void Initialize(MonsterData data, Transform[] path)
{
    monsterData = data;
    waypoints = path;

    // 스탯 설정
    maxHP = data.maxHP;
    currentHP = maxHP;
    moveSpeed = data.moveSpeed;
    damage = data.damage;
    goldReward = data.goldReward;

    // 스프라이트 설정
    SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
    if (spriteRenderer != null && data.monsterSprite != null)
    {
        spriteRenderer.sprite = data.monsterSprite;
    }

    // HP 텍스트 초기화
    if (hpText == null)
    {
        hpText = GetComponentInChildren<TMP_Text>();
    }
    UpdateHPDisplay();

    Debug.Log($"✅ {monsterData.monsterName} 초기화 완료 (HP: {currentHP}/{maxHP})");
}
```

2. **데미지 처리:**
```csharp
public virtual void TakeDamage(int damageAmount)
{
    currentHP -= damageAmount;
    UpdateHPDisplay();

    Debug.Log($"💥 {monsterData.monsterName} 피해 받음: -{damageAmount} (남은 HP: {currentHP})");

    if (currentHP <= 0)
    {
        Die();
    }
}
```

3. **HP 표시 업데이트:**
```csharp
protected void UpdateHPDisplay()
{
    if (hpText != null)
    {
        hpText.text = $"{Mathf.CeilToInt(currentHP)}";
        // 또는: hpText.text = $"{currentHP}/{maxHP}";
    }
}
```

4. **죽음 처리:**
```csharp
protected virtual void Die()
{
    Debug.Log($"💀 {monsterData.monsterName} 사망");

    // 골드 보상 지급 (GameManager 호출)
    // GameManager.Instance.AddGold(goldReward);

    // 오브젝트 제거
    Destroy(gameObject);
}
```

5. **목표 도달:**
```csharp
protected virtual void ReachGoal()
{
    Debug.Log($"🎯 {monsterData.monsterName} 목표 도달! 플레이어 피해: -{damage}");

    // 플레이어에게 피해 (GameManager 호출)
    // GameManager.Instance.TakeDamage(damage);

    Destroy(gameObject);
}
```

---

#### **Monster.cs**
기본 몬스터 구현 (Transform[] 경로 이동)

```csharp
public class Monster : MonsterBase
{
    void Update()
    {
        MovePath();
    }

    void MovePath()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (currentWaypointIndex >= waypoints.Length)
        {
            ReachGoal();
            return;
        }

        // 현재 웨이포인트
        Transform targetWaypoint = waypoints[currentWaypointIndex];

        if (targetWaypoint == null)
        {
            Debug.LogError("❌ 웨이포인트가 null입니다!");
            return;
        }

        // 웨이포인트를 향해 이동
        Vector3 direction = (targetWaypoint.position - transform.position).normalized;
        transform.position += direction * moveSpeed * Time.deltaTime;

        // 웨이포인트 도달 체크
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distance < 0.1f)
        {
            currentWaypointIndex++;  // 다음 웨이포인트로
        }
    }
}
```

**특징:**
- 단순 직선 이동
- Transform 배열 경로 사용
- Update()에서 매 프레임 이동

---

#### **NewEnemy.cs**
그리드 기반 몬스터 (MonsterPathManager 경로 사용)

```csharp
public class NewEnemy : MonoBehaviour
{
    [Header("References")]
    private MonsterPathManager pathManager;
    private List<Vector2Int> path;

    [Header("Stats")]
    private float maxHealth = 100f;
    private float currentHealth;
    private float moveSpeed = 2f;

    [Header("Movement")]
    private int currentPathIndex = 0;
    private bool isMoving = false;

    [Header("HP Display")]
    [SerializeField] private TMP_Text hpText;

    void Awake()
    {
        currentHealth = maxHealth;

        if (hpText == null)
        {
            hpText = GetComponentInChildren<TMP_Text>();
        }
        UpdateHPDisplay();
    }

    void Start()
    {
        pathManager = FindObjectOfType<MonsterPathManager>();

        if (pathManager != null && pathManager.HasPath())
        {
            path = pathManager.GetPathPositions();
            isMoving = true;

            // 경로의 첫 번째 위치에서 시작
            if (path.Count > 0)
            {
                transform.position = new Vector3(path[0].x, path[0].y, 0);
            }
        }
    }

    void Update()
    {
        if (!isMoving || path == null || path.Count == 0)
            return;

        MoveAlongPath();
    }

    void MoveAlongPath()
    {
        if (currentPathIndex >= path.Count)
        {
            // 경로 끝 도달 → 순환 또는 제거
            currentPathIndex = 0;  // 순환
            // 또는: Destroy(gameObject);  // 제거
            return;
        }

        // 목표 위치
        Vector3 targetPosition = new Vector3(path[currentPathIndex].x, path[currentPathIndex].y, 0);

        // 이동
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        // 도달 체크
        if (Vector3.Distance(transform.position, targetPosition) < 0.01f)
        {
            currentPathIndex++;
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHPDisplay();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHPDisplay()
    {
        if (hpText != null)
        {
            hpText.text = $"{Mathf.CeilToInt(currentHealth)}";
        }
    }

    void Die()
    {
        Debug.Log($"💀 {gameObject.name} 사망");
        Destroy(gameObject);
    }
}
```

**특징:**
- List<Vector2Int> 경로 사용 (그리드 좌표)
- MonsterPathManager와 연동
- 외곽 순환 경로 이동

---

## 3. 경로 이동 시스템

### 📝 개요
몬스터가 지정된 웨이포인트를 따라 이동하는 시스템입니다.

### 🔑 주요 메커니즘

#### **1. Transform 기반 경로 이동 (Monster.cs)**

```csharp
// 경로 초기화
Transform[] waypoints = new Transform[pathLength];
for (int i = 0; i < pathLength; i++)
{
    GameObject waypoint = new GameObject($"Waypoint_{i}");
    waypoint.transform.position = new Vector3(x, y, 0);
    waypoints[i] = waypoint.transform;
}

// 이동 로직
void MovePath()
{
    // 1. 현재 웨이포인트 가져오기
    Transform target = waypoints[currentWaypointIndex];

    // 2. 방향 계산
    Vector3 direction = (target.position - transform.position).normalized;

    // 3. 이동
    transform.position += direction * moveSpeed * Time.deltaTime;

    // 4. 도달 체크
    if (Vector3.Distance(transform.position, target.position) < 0.1f)
    {
        currentWaypointIndex++;  // 다음 웨이포인트
    }
}
```

**장점:**
- 직관적인 경로 설정
- Unity Scene에서 시각적으로 확인 가능
- 자유로운 경로 형태

**단점:**
- GameObject 오버헤드
- 경로 변경 시 수동 업데이트 필요

---

#### **2. 그리드 좌표 기반 경로 이동 (NewEnemy.cs)**

```csharp
// 경로 초기화 (MonsterPathManager에서 가져옴)
List<Vector2Int> path = pathManager.GetPathPositions();

// 이동 로직
void MoveAlongPath()
{
    // 1. 목표 위치 (그리드 좌표 → 월드 좌표)
    Vector3 targetPos = new Vector3(path[currentPathIndex].x, path[currentPathIndex].y, 0);

    // 2. MoveTowards로 부드러운 이동
    transform.position = Vector3.MoveTowards(
        transform.position,
        targetPos,
        moveSpeed * Time.deltaTime
    );

    // 3. 도달 체크
    if (Vector3.Distance(transform.position, targetPos) < 0.01f)
    {
        currentPathIndex++;
    }

    // 4. 경로 끝 처리
    if (currentPathIndex >= path.Count)
    {
        currentPathIndex = 0;  // 순환
    }
}
```

**장점:**
- 메모리 효율적 (Vector2Int만 저장)
- MonsterPathManager와 자동 연동
- 그리드 변경 시 자동 업데이트

**단점:**
- 그리드 기반으로만 이동 가능
- 대각선 이동 시 부자연스러울 수 있음

---

#### **3. 이동 방식 비교**

| 항목 | Transform 기반 | 그리드 좌표 기반 |
|------|----------------|------------------|
| **메모리** | GameObject 생성 필요 | Vector2Int만 저장 |
| **유연성** | 자유로운 경로 | 그리드 제약 |
| **자동 업데이트** | 수동 | MonsterPathManager 연동 |
| **시각화** | Scene에서 확인 가능 | Gizmo로 확인 |
| **성능** | GameObject 오버헤드 | 더 효율적 |

---

## 4. 몬스터 스폰 시스템

### 🔑 주요 스크립트

#### **MonsterSpawner.cs**
몬스터 생성 및 경로 할당 관리

```csharp
public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Setting")]
    [SerializeField] private MonsterData[] monsterDataList;

    [Header("Path Setting")]
    [SerializeField] private NewPathFinder pathFinder;
    [SerializeField] private MonsterPathManager monsterPathManager;

    [Header("Spawn Setting")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Vector2Int spawnGridPosition;
    [SerializeField] private bool useFirstWaypointAsSpawn = true;

    private bool isSpawning = false;
    private Coroutine spawnCoroutine;
    private List<GameObject> spawnedMonsters = new List<GameObject>();
}
```

**핵심 메서드:**

1. **스폰 시작:**
```csharp
public void StartSpawning()
{
    if (isSpawning)
    {
        Debug.LogWarning("⚠️ 이미 스폰 중입니다.");
        return;
    }

    isSpawning = true;
    spawnCoroutine = StartCoroutine(SpawnMonster());
    Debug.Log("▶️ 몬스터 스폰 시작");
}
```

2. **스폰 코루틴:**
```csharp
IEnumerator SpawnMonster()
{
    yield return new WaitForSeconds(1f);  // 첫 스폰 전 1초 대기

    while (isSpawning)
    {
        // 1. 경로 확인
        if (pathFinder == null || !pathFinder.HasPath())
        {
            Debug.LogWarning("⚠️ 경로가 없습니다.");
            yield return new WaitForSeconds(1f);
            continue;
        }

        // 2. 랜덤 몬스터 선택 (가중치 기반)
        MonsterData randomMonsterData = GetRandomMonsterData();

        // 3. 스폰
        Spawn(randomMonsterData);

        // 4. 대기
        yield return new WaitForSeconds(spawnInterval);
    }
}
```

3. **가중치 기반 랜덤 선택:**
```csharp
MonsterData GetRandomMonsterData()
{
    // 1. 가중치 배열 생성
    float[] weights = new float[monsterDataList.Length];
    for (int i = 0; i < monsterDataList.Length; i++)
    {
        weights[i] = monsterDataList[i].spawnWeight;
    }

    // 2. 누적합 계산
    float cumulativeWeight = 0f;
    float[] cumulativeWeights = new float[weights.Length];
    for (int i = 0; i < weights.Length; i++)
    {
        cumulativeWeight += weights[i];
        cumulativeWeights[i] = cumulativeWeight;
    }

    // 3. 랜덤값 생성 (0 ~ totalWeight)
    float randomValue = Random.value * cumulativeWeight;

    // 4. 누적합 기반 선택
    for (int i = 0; i < weights.Length; i++)
    {
        if (randomValue <= cumulativeWeights[i])
        {
            return monsterDataList[i];
        }
    }

    return monsterDataList[0];  // 기본값
}
```

**예시:**
```
몬스터 A: weight = 50  → 누적합 = 50
몬스터 B: weight = 30  → 누적합 = 80
몬스터 C: weight = 20  → 누적합 = 100

랜덤값 45 → A 선택
랜덤값 65 → B 선택
랜덤값 95 → C 선택
```

4. **몬스터 스폰:**
```csharp
void Spawn(MonsterData monsterData)
{
    // 1. 스폰 위치 결정
    Vector2Int spawnPos;
    if (useFirstWaypointAsSpawn)
    {
        List<Vector2Int> path = pathFinder.GetPath();
        spawnPos = path.Count > 0 ? path[0] : Vector2Int.zero;
    }
    else
    {
        spawnPos = spawnGridPosition;
    }

    // 2. 월드 좌표로 변환
    Vector3 spawnWorldPos = new Vector3(spawnPos.x, spawnPos.y, 0);

    // 3. 몬스터 생성
    GameObject monsterObj = Instantiate(
        monsterData.monsterPrefab,
        spawnWorldPos,
        Quaternion.identity
    );

    // 4. 몬스터 컴포넌트 초기화
    if (monsterObj.TryGetComponent(out MonsterBase monster))
    {
        // NewPathFinder의 경로를 Transform[]로 변환
        Transform[] pathTransforms = ConvertPathToTransforms(pathFinder.GetPath());
        monster.Initialize(monsterData, pathTransforms);
    }

    // 5. 생성된 몬스터 추적
    spawnedMonsters.Add(monsterObj);

    Debug.Log($"✅ Spawned {monsterData.monsterName} at {spawnWorldPos}");
}
```

5. **경로 변환 (Vector2Int → Transform[]):**
```csharp
Transform[] ConvertPathToTransforms(List<Vector2Int> gridPath)
{
    if (gridPath == null || gridPath.Count == 0)
        return new Transform[0];

    Transform[] transforms = new Transform[gridPath.Count];

    for (int i = 0; i < gridPath.Count; i++)
    {
        // 각 그리드 좌표에 대응하는 GameObject 생성
        GameObject waypoint = new GameObject($"Waypoint_{i}");
        waypoint.transform.position = new Vector3(gridPath[i].x, gridPath[i].y, 0);
        waypoint.transform.SetParent(transform);  // MonsterSpawner의 자식으로
        transforms[i] = waypoint.transform;
    }

    return transforms;
}
```

**주의사항:**
- 경로 변경 시 기존 Waypoint 제거 필요
- MonsterSpawner의 자식으로 설정하여 관리 용이

6. **몬스터 정리:**
```csharp
public void ClearAllMonsters()
{
    foreach (GameObject monster in spawnedMonsters)
    {
        if (monster != null)
        {
            Destroy(monster);
        }
    }
    spawnedMonsters.Clear();

    // 기존 Waypoint도 제거
    ClearExistingWaypoints();

    Debug.Log("🧹 모든 몬스터 및 Waypoint 제거됨");
}

void ClearExistingWaypoints()
{
    // MonsterSpawner의 자식 중 "Waypoint_"로 시작하는 모든 오브젝트 제거
    for (int i = transform.childCount - 1; i >= 0; i--)
    {
        Transform child = transform.GetChild(i);
        if (child.name.StartsWith("Waypoint_"))
        {
            Destroy(child.gameObject);
        }
    }
}
```

**중요:**
- 역순으로 반복 (i--) → 삭제 중 인덱스 오류 방지
- 몬스터 제거 시 Waypoint도 함께 제거

---

## 5. 데미지 및 체력 시스템

### 📝 개요
몬스터가 타워의 공격을 받아 체력이 감소하고, 0 이하가 되면 사망하는 시스템입니다.

### 🔑 주요 메커니즘

#### **1. 데미지 처리 흐름**

```
타워 공격 (Tower.Attack())
    ↓
발사체 생성 (Bullet)
    ↓
발사체 이동 (Bullet.Update())
    ↓
타겟 도달 (Bullet.HitTarget())
    ↓
몬스터 데미지 (Monster.TakeDamage(damage))
    ↓
체력 감소 (currentHP -= damage)
    ↓
HP 표시 업데이트 (UpdateHPDisplay())
    ↓
    ├─→ currentHP > 0 → 계속 이동
    └─→ currentHP <= 0 → Die()
```

---

#### **2. TakeDamage() 구현**

```csharp
public virtual void TakeDamage(int damageAmount)
{
    // 1. 체력 감소
    currentHP -= damageAmount;

    // 2. HP 표시 업데이트
    UpdateHPDisplay();

    // 3. 피격 효과 (선택 사항)
    PlayHitEffect();

    // 4. 로그
    Debug.Log($"💥 {monsterData.monsterName} 피해: -{damageAmount} (남은 HP: {currentHP})");

    // 5. 사망 체크
    if (currentHP <= 0)
    {
        Die();
    }
}

void PlayHitEffect()
{
    // 빨간색 깜빡임 효과
    StartCoroutine(FlashRed());
}

IEnumerator FlashRed()
{
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    Color original = sr.color;

    sr.color = Color.red;
    yield return new WaitForSeconds(0.1f);
    sr.color = original;
}
```

---

#### **3. HP 표시 (TextMeshPro)**

```csharp
[Header("HP Display")]
[SerializeField] private TMP_Text hpText;

void Initialize(...)
{
    // HP 텍스트 자동 찾기
    if (hpText == null)
    {
        hpText = GetComponentInChildren<TMP_Text>();
    }
    UpdateHPDisplay();
}

void UpdateHPDisplay()
{
    if (hpText != null)
    {
        // 정수로 표시
        hpText.text = $"{Mathf.CeilToInt(currentHP)}";

        // 또는 분수로 표시
        // hpText.text = $"{currentHP}/{maxHP}";

        // 색상 변경 (체력에 따라)
        if (currentHP < maxHP * 0.3f)
        {
            hpText.color = Color.red;
        }
        else if (currentHP < maxHP * 0.6f)
        {
            hpText.color = Color.yellow;
        }
        else
        {
            hpText.color = Color.white;
        }
    }
}
```

**Unity 설정:**
1. 몬스터 프리팹에 Canvas 추가
2. Canvas에 TextMeshPro - Text 추가
3. Canvas의 Render Mode = World Space
4. hpText 변수에 할당

---

#### **4. 사망 처리**

```csharp
protected virtual void Die()
{
    Debug.Log($"💀 {monsterData.monsterName} 사망");

    // 1. 골드 보상
    GameManager gameManager = FindObjectOfType<GameManager>();
    if (gameManager != null)
    {
        gameManager.AddGold(goldReward);
    }

    // 2. 사망 이펙트 (선택 사항)
    PlayDeathEffect();

    // 3. 오브젝트 제거
    Destroy(gameObject);
}

void PlayDeathEffect()
{
    // 파티클 효과 생성
    if (deathEffectPrefab != null)
    {
        Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
    }

    // 사운드 재생
    // AudioManager.Instance.PlaySound("MonsterDeath");
}
```

---

#### **5. 목표 도달 처리**

```csharp
protected virtual void ReachGoal()
{
    Debug.Log($"🎯 {monsterData.monsterName} 목표 도달! 플레이어 피해: -{damage}");

    // 1. 플레이어에게 피해
    GameManager gameManager = FindObjectOfType<GameManager>();
    if (gameManager != null)
    {
        gameManager.TakeDamage(damage);
    }

    // 2. 몬스터 제거
    Destroy(gameObject);
}
```

---

## 📊 몬스터 시스템 흐름도

```
게임 시작
    ↓
MonsterPathManager.ShowMonsterPath()
    ↓ (외곽 경로 생성)
    ↓
MonsterSpawner.StartSpawning()
    ↓
SpawnMonster() 코루틴 시작
    ↓
    ┌─────────────────────────┐
    │ while (isSpawning)      │
    │     ↓                   │
    │ GetRandomMonsterData()  │ (가중치 기반)
    │     ↓                   │
    │ Spawn(monsterData)      │
    │     ↓                   │
    │ Instantiate 몬스터      │
    │     ↓                   │
    │ ConvertPathToTransforms │ (경로 변환)
    │     ↓                   │
    │ Monster.Initialize()    │
    │     ↓                   │
    │ WaitForSeconds(interval)│
    │     ↓                   │
    └─────────────────────────┘
            ↓
    Monster.Update() 루프 시작
            ↓
    MovePath() → 경로 따라 이동
            ↓
    ├─→ 웨이포인트 도달 → currentWaypointIndex++
    ├─→ TakeDamage() → currentHP 감소
    │       ↓
    │   UpdateHPDisplay()
    │       ↓
    │   currentHP <= 0?
    │       ↓ YES
    │   Die() → 골드 보상, Destroy
    │
    └─→ 경로 끝 도달 → ReachGoal()
            ↓
        플레이어 피해, Destroy
```

---

## 🔧 주요 디자인 패턴

### 1. **템플릿 메서드 패턴 (Template Method Pattern)**
```csharp
// MonsterBase: 추상 메서드로 공통 흐름 정의
public abstract class MonsterBase
{
    public virtual void Initialize(...) { }
    public virtual void TakeDamage(...) { }
    protected virtual void Die() { }
    protected virtual void ReachGoal() { }
}

// Monster, NewEnemy: 구체적인 구현
public class Monster : MonsterBase
{
    // 이동 로직만 구현
    void MovePath() { ... }
}
```

**장점:**
- 공통 로직 재사용 (데미지, HP 표시)
- 확장 용이 (새로운 몬스터 타입 추가)

---

### 2. **ScriptableObject 패턴**
```csharp
MonsterData (ScriptableObject)
```

**장점:**
- 데이터와 로직 분리
- Unity Inspector에서 편집 가능
- 메모리 효율적 (여러 몬스터가 같은 데이터 공유 가능)

---

### 3. **코루틴 패턴 (Coroutine Pattern)**
```csharp
IEnumerator SpawnMonster()
{
    while (isSpawning)
    {
        Spawn();
        yield return new WaitForSeconds(spawnInterval);
    }
}
```

**장점:**
- 시간 기반 작업 처리 용이
- Update()보다 가독성 좋음
- 일시정지/재개 가능

---

### 4. **오브젝트 추적 패턴**
```csharp
private List<GameObject> spawnedMonsters = new List<GameObject>();

void Spawn(...)
{
    GameObject monster = Instantiate(...);
    spawnedMonsters.Add(monster);
}

void ClearAllMonsters()
{
    foreach (GameObject monster in spawnedMonsters)
    {
        Destroy(monster);
    }
    spawnedMonsters.Clear();
}
```

**장점:**
- 생성된 오브젝트 관리 용이
- 일괄 제거 가능
- 메모리 누수 방지

---

## 💡 핵심 알고리즘

### 1. **가중치 기반 랜덤 선택**
```csharp
// 예시: A(50), B(30), C(20)
누적합: [50, 80, 100]

랜덤값 = Random.value * 100  // 0 ~ 100

if (랜덤값 <= 50)  → A 선택 (50% 확률)
else if (랜덤값 <= 80)  → B 선택 (30% 확률)
else  → C 선택 (20% 확률)
```

**시간 복잡도:** O(n)
**공간 복잡도:** O(n) (누적합 배열)

---

### 2. **웨이포인트 이동**
```csharp
// 1. 방향 계산
Vector3 direction = (target.position - current.position).normalized;

// 2. 이동
current.position += direction * speed * Time.deltaTime;

// 3. 도달 체크
if (Distance(current, target) < threshold)
{
    currentIndex++;
}
```

**시간 복잡도:** O(1) (매 프레임)
**정확도:** threshold 값에 따라 결정

---

### 3. **경로 변환 (Vector2Int → Transform[])**
```csharp
Transform[] ConvertPathToTransforms(List<Vector2Int> gridPath)
{
    Transform[] transforms = new Transform[gridPath.Count];

    for (int i = 0; i < gridPath.Count; i++)
    {
        GameObject waypoint = new GameObject($"Waypoint_{i}");
        waypoint.transform.position = new Vector3(gridPath[i].x, gridPath[i].y, 0);
        transforms[i] = waypoint.transform;
    }

    return transforms;
}
```

**시간 복잡도:** O(n) (n = 경로 길이)
**GameObject 생성 비용:** 높음

---

## 🎯 최적화 포인트

### 1. **Waypoint GameObject 재사용**
- 매 스폰마다 새로 생성하지 않고 재사용
- 경로 변경 시에만 업데이트

### 2. **Vector3.MoveTowards 사용**
```csharp
// 부드러운 이동 + 오버슈팅 방지
transform.position = Vector3.MoveTowards(
    current,
    target,
    speed * Time.deltaTime
);
```

### 3. **HP 표시 업데이트 최소화**
```csharp
// 변경 시에만 업데이트 (매 프레임 아님)
void TakeDamage(int damage)
{
    currentHP -= damage;
    UpdateHPDisplay();  // 여기서만 호출
}
```

### 4. **오브젝트 풀링 (선택 사항)**
```csharp
// 몬스터를 Destroy 대신 비활성화 후 재사용
void Die()
{
    gameObject.SetActive(false);
    MonsterPool.ReturnToPool(this);
}
```

### 5. **Waypoint 정리**
```csharp
// 역순으로 제거 (인덱스 오류 방지)
for (int i = transform.childCount - 1; i >= 0; i--)
{
    Destroy(transform.GetChild(i).gameObject);
}
```

---

## 🐛 일반적인 문제 및 해결

### 1. **MissingReferenceException: Transform has been destroyed**
**원인:** 몬스터가 Waypoint Transform을 참조 중인데 Waypoint가 삭제됨

**해결:**
```csharp
// Waypoint 접근 전 null 체크
Transform targetWaypoint = waypoints[currentWaypointIndex];
if (targetWaypoint == null)
{
    Debug.LogError("❌ 웨이포인트가 null입니다!");
    return;
}
```

**근본 해결:**
- 몬스터를 모두 제거한 후 Waypoint 제거
- ClearAllMonsters() → ClearExistingWaypoints() 순서

---

### 2. **몬스터가 움직이지 않음**
**체크 리스트:**
1. `isMoving = true` 설정 확인
2. `waypoints` 또는 `path`가 null이 아닌지 확인
3. `moveSpeed > 0` 확인
4. Update()에서 MovePath() 호출 확인
5. 경로 생성 확인 (MonsterPathManager.ShowMonsterPath())

---

### 3. **HP 표시가 안 보임**
**체크 리스트:**
1. Canvas의 Render Mode = World Space
2. Canvas의 Scale 확인 (너무 작으면 안 보임)
3. TextMeshPro 컴포넌트 확인
4. hpText 변수 할당 확인
5. Camera의 Culling Mask 확인

---

### 4. **몬스터가 너무 빠르거나 느림**
**조정 방법:**
```csharp
// MonsterData에서 moveSpeed 조정
moveSpeed = 2f;  // 기본값

// 또는 코드에서 동적 조정
monster.moveSpeed *= difficultyMultiplier;
```

---

## 📚 참고 자료

- **Unity Coroutines**: https://docs.unity3d.com/Manual/Coroutines.html
- **Vector3.MoveTowards**: https://docs.unity3d.com/ScriptReference/Vector3.MoveTowards.html
- **TextMeshPro**: https://docs.unity3d.com/Manual/com.unity.textmeshpro.html
- **ScriptableObject**: https://docs.unity3d.com/Manual/class-ScriptableObject.html

---

## ✅ 학습 체크리스트

- [ ] MonsterBase 추상 클래스 구조 이해
- [ ] Transform 기반 vs 그리드 좌표 기반 이동 차이
- [ ] 가중치 기반 랜덤 선택 알고리즘 구현
- [ ] Coroutine을 활용한 스폰 시스템
- [ ] TakeDamage()와 Die() 흐름 이해
- [ ] HP 표시 (TextMeshPro) 구현 방법
- [ ] Waypoint GameObject 관리 방법
- [ ] MissingReferenceException 원인 및 해결
- [ ] Vector3.MoveTowards vs 수동 이동 차이
- [ ] 오브젝트 추적 및 일괄 제거 패턴

---

**작성일:** 2025-01-10
**버전:** 1.0
**작성자:** Claude Code Assistant
