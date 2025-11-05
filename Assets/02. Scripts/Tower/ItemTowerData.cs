using UnityEngine;

/// <summary>
/// 아이템 타워 데이터 ScriptableObject
/// 인접한 타워들에게 버프를 제공하는 보조 타워
/// </summary>
[CreateAssetMenu(fileName = "New Item Tower Data", menuName = "SO/Item Tower Data")]
public class ItemTowerData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemName = "Buff Item";
    public Color itemColor = Color.yellow;
    [TextArea(2, 4)]
    public string description = "인접한 타워들에게 버프를 제공합니다.";

    [Header("Spawn Settings")]
    [Range(1f, 100f)]
    public float spawnWeight = 10f; // 스폰 가중치

    [Header("Buff Range")]
    [Range(1, 10)]
    public int buffRange = 2; // 버프 범위 (그리드 칸 단위)

    [Header("Buff Effects")]
    [Range(0f, 5f)]
    public float damageMultiplier = 1.2f; // 공격력 배율 (1.2 = 20% 증가)

    [Range(0f, 5f)]
    public float rangeMultiplier = 1.1f; // 사거리 배율 (1.1 = 10% 증가)

    [Range(0f, 5f)]
    public float fireRateMultiplier = 1.15f; // 공격속도 배율 (1.15 = 15% 증가)

    [Header("Additional Buffs")]
    public bool providesShield = false; // 방어막 제공 여부
    public int shieldAmount = 0; // 방어막 수치

    public bool providesHealing = false; // 힐링 제공 여부
    public float healingPerSecond = 0f; // 초당 회복량

    [Header("Visual")]
    public Sprite itemIcon; // 아이템 아이콘 (선택 사항)
    public GameObject buffEffectPrefab; // 버프 이펙트 프리팹 (선택 사항)

    /// <summary>
    /// 디버그 정보 출력
    /// </summary>
    public string GetDebugInfo()
    {
        return $"{itemName}\n" +
               $"Range: {buffRange} cells\n" +
               $"DMG: x{damageMultiplier:F2}, Range: x{rangeMultiplier:F2}, Speed: x{fireRateMultiplier:F2}";
    }
}
