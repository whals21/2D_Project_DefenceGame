using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Monster Setting")]
    //monsterData 리스트를 다음을 변수
    [SerializeField] private MonsterData[] monsterDataList;
    //[SerializeField] private MonsterData monsterData;

    [Header("Path Setting")]
    [SerializeField] private NewPathFinder pathFinder; // WayPointPath 대신 NewPathFinder 사용
    [SerializeField] private MonsterPathManager monsterPathManager; // 경로 관리자 참조

    [Header("Spawn Setting")]
    [SerializeField] private float spawnInterval = 2f;
    [SerializeField] private Vector2Int spawnGridPosition; // 스폰할 그리드 좌표 (경로상의 특정 위치)
    [SerializeField] private bool useFirstWaypointAsSpawn = true; // true면 경로의 첫 지점에서 스폰

    private void Start()
    {
        // PathFinder 자동 탐색
        if (pathFinder == null)
        {
            pathFinder = FindObjectOfType<NewPathFinder>();
        }

        // MonsterPathManager 자동 탐색
        if (monsterPathManager == null)
        {
            monsterPathManager = FindObjectOfType<MonsterPathManager>();
        }

        StartCoroutine(SpawnMonster());
    }

    IEnumerator SpawnMonster()
    {
        yield return new WaitForSeconds(1f); //첫 몬스터 생성 전 1초 대기

        while (true)
        {
            // 경로 확인
            if (pathFinder == null || !pathFinder.HasPath())
            {
                Debug.LogWarning("⚠️ MonsterSpawner: 경로가 없습니다. 경로를 생성하세요.");
                yield return new WaitForSeconds(1f);
                continue;
            }

            //몬스터 데이터 리스트에서 랜덤으로 선택(가중치 부여, ex)1번 몬스터가 2번 몬스터보다 3배 더 자주 생성되도록 함)
            MonsterData randomMonsterData = GetRandomMonsterData();
            Spawn(randomMonsterData);
            yield return new WaitForSeconds(spawnInterval);
        }
    }

    MonsterData GetRandomMonsterData()
    {
        // 가중치 배열 생성
        float[] weights = new float[monsterDataList.Length];
        for(int i = 0; i < monsterDataList.Length; i++)
        {
            weights[i] = monsterDataList[i].spawnWeight;
        }

        // 가중치 배열의 누적합 계산
        float cumulativeWeight = 0f;
        float[] cumulativeWeights = new float[weights.Length];
        for(int i = 0; i < weights.Length; i++)
        {
            cumulativeWeight += weights[i];
            cumulativeWeights[i] = cumulativeWeight;
        }
        
        // 누적합을 기반으로 랜덤한 인덱스 선택
        float randomValue = Random.value * cumulativeWeight;
        for(int i = 0; i < weights.Length; i++)
        {
            if(randomValue <= cumulativeWeights[i])
            {
                return monsterDataList[i];
            }
        }
    
        return monsterDataList[0]; // 기본값 반환
    }

    void Spawn(MonsterData monsterData)
    {
        // 스폰 위치 결정
        Vector2Int spawnPos;
        if (useFirstWaypointAsSpawn)
        {
            List<Vector2Int> path = pathFinder.GetPath();
            spawnPos = path.Count > 0 ? path[0] : Vector2Int.zero; // 경로의 첫 번째 지점
        }
        else
        {
            spawnPos = spawnGridPosition; // 수동 설정한 그리드 좌표
        }

        // 월드 좌표로 변환하여 몬스터 생성
        Vector3 spawnWorldPos = new Vector3(spawnPos.x, spawnPos.y, 0);
        GameObject monsterObj = Instantiate(monsterData.monsterPrefab, spawnWorldPos, Quaternion.identity);

        //몬스터 컴포넌트가 존재하면 초기화
        if(monsterObj.TryGetComponent(out MonsterBase monster))
        {
            // NewPathFinder의 경로를 Transform[]로 변환
            Transform[] pathTransforms = ConvertPathToTransforms(pathFinder.GetPath());
            monster.Initialize(monsterData, pathTransforms); //몬스터 초기화
        }

        Debug.Log($"✅ Spawned {monsterData.monsterName} at grid position {spawnPos} (world: {spawnWorldPos})");
    }

    /// <summary>
    /// Vector2Int 경로를 Transform[] 배열로 변환
    /// MonsterBase가 Transform[] 경로를 사용하므로 변환 필요
    /// </summary>
    Transform[] ConvertPathToTransforms(List<Vector2Int> gridPath)
    {
        if (gridPath == null || gridPath.Count == 0)
            return new Transform[0];

        Transform[] transforms = new Transform[gridPath.Count];

        for (int i = 0; i < gridPath.Count; i++)
        {
            // 각 그리드 좌표에 대응하는 빈 GameObject 생성
            GameObject waypoint = new GameObject($"Waypoint_{i}");
            waypoint.transform.position = new Vector3(gridPath[i].x, gridPath[i].y, 0);
            waypoint.transform.SetParent(transform); // MonsterSpawner의 자식으로 설정
            transforms[i] = waypoint.transform;
        }

        return transforms;
    }
}
