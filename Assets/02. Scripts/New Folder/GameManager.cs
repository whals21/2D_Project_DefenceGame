using UnityEngine;

/// <summary>
/// 게임 전체를 관리하는 매니저
/// 플레이어 정보, 게임 상태, 점수 등을 관리
/// </summary>
public class GameManager : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static GameManager Instance { get; private set; }

    [Header("Player Reference")]
    public PlayerController player; // 플레이어 컨트롤러 참조

    [Header("Game State")]
    public bool isGameStarted = false;
    public bool isGameOver = false;
    public bool isPaused = false;

    [Header("Game Settings")]
    [SerializeField] private int startingMoney = 100;
    [SerializeField] private int startingLives = 20;

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // PlayerController 자동 생성
        if (player == null)
        {
            GameObject playerObj = new GameObject("Player");
            player = playerObj.AddComponent<PlayerController>();
            playerObj.transform.SetParent(transform);
        }
    }

    void Start()
    {
        InitializeGame();
    }

    /// <summary>
    /// 게임 초기화
    /// </summary>
    void InitializeGame()
    {
        if (player != null)
        {
            player.Initialize(startingMoney, startingLives);
        }

        isGameStarted = false;
        isGameOver = false;
        isPaused = false;

        Debug.Log("🎮 GameManager initialized!");
    }

    /// <summary>
    /// 게임 시작
    /// </summary>
    public void StartGame()
    {
        isGameStarted = true;
        isGameOver = false;
        Debug.Log("▶️ Game Started!");
    }

    /// <summary>
    /// 게임 오버
    /// </summary>
    public void GameOver()
    {
        isGameOver = true;
        isGameStarted = false;
        Debug.Log("💀 Game Over!");

        // 게임 오버 처리 (예: UI 표시, 몬스터 스폰 중지 등)
    }

    /// <summary>
    /// 게임 일시정지
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Debug.Log("⏸️ Game Paused");
    }

    /// <summary>
    /// 게임 재개
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        Debug.Log("▶️ Game Resumed");
    }

    /// <summary>
    /// 게임 리셋
    /// </summary>
    public void ResetGame()
    {
        InitializeGame();
        Debug.Log("🔄 Game Reset");
    }

    void Update()
    {
        // 테스트용 키보드 입력
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (!isGameStarted)
                StartGame();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    // OnGUI는 GameUIManager에서 통합 관리됨
}

/// <summary>
/// 플레이어 상태를 관리하는 컨트롤러
/// 돈, 생명, 점수 등을 관리
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Player Stats")]
    [SerializeField] private int money = 100;
    [SerializeField] private int lives = 20;
    [SerializeField] private int score = 0;

    /// <summary>
    /// 플레이어 초기화
    /// </summary>
    public void Initialize(int startMoney, int startLives)
    {
        money = startMoney;
        lives = startLives;
        score = 0;

        Debug.Log($"💰 Player initialized: Money={money}, Lives={lives}");
    }

    /// <summary>
    /// 돈 추가
    /// </summary>
    public void AddMoney(int amount)
    {
        money += amount;
        Debug.Log($"💰 +{amount} money! Total: {money}");
    }

    /// <summary>
    /// 돈 차감
    /// </summary>
    public bool SpendMoney(int amount)
    {
        if (money >= amount)
        {
            money -= amount;
            Debug.Log($"💸 -{amount} money! Remaining: {money}");
            return true;
        }
        else
        {
            Debug.LogWarning($"⚠️ Not enough money! Need {amount}, have {money}");
            return false;
        }
    }

    /// <summary>
    /// 생명 차감
    /// </summary>
    public void SubtractLife(int amount)
    {
        lives -= amount;
        Debug.Log($"💔 -{amount} life! Remaining: {lives}");

        if (lives <= 0)
        {
            lives = 0;
            GameManager.Instance?.GameOver();
        }
    }

    /// <summary>
    /// 생명 추가
    /// </summary>
    public void AddLife(int amount)
    {
        lives += amount;
        Debug.Log($"💚 +{amount} life! Total: {lives}");
    }

    /// <summary>
    /// 점수 추가
    /// </summary>
    public void AddScore(int amount)
    {
        score += amount;
        Debug.Log($"⭐ +{amount} score! Total: {score}");
    }

    /// <summary>
    /// Getter 메서드들
    /// </summary>
    public int GetMoney() => money;
    public int GetLives() => lives;
    public int GetScore() => score;
}
