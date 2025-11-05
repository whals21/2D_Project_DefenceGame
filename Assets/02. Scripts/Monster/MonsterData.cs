using UnityEngine;

/*================================================================================
[ScriptableObject] 스크립터블 오브젝트
- 클래스 인스턴스와는 별도로 대량의 데이터를 저장하는데 사용할 수 있는 데이터 컨테이너
- 값의 사본이 생성되는 것을 방지하여 메모리 사용을 줄여줌
- 이는 연결된 MonoBehaviour 스크립트에 변경되지 않는 데이터를 저장하는 프리팹이 있는 프로젝트의 경우 유용하다
- 게임 오브젝트에 붙지 않고 프로젝트 폴더 .assset파일로 저장되어 런타임 중에도 여러 오브젝트가 같은 데이터를 공유할 수 있다
- 예를 들어, 몬스터, 아이템, 스킬 등의 데이터를 저장하는데 사용할 수 있다
- 게임 플레이중에 변하는 정보가 실시간 저장되지 않는다. 이 기능을 원하면 PlayerPrefs를 사용하면 된다

================================================================================*/
[CreateAssetMenu(fileName = "New Monster Data", menuName = "SO/Monster Data")] // 스크립터블 오브젝트 생성 메뉴
public class MonsterData : ScriptableObject
{
    public string monsterName;
    public int maxHP;
    public float moveSpeed;
    public GameObject monsterPrefab;
    public float spawnWeight; //생성 가중치
}
