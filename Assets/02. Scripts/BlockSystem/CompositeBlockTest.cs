using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Composite Block 방식 테스트
/// (1,1) 크기의 셀 프리팹을 조합하여 블록 생성
/// </summary>
public class CompositeBlockTest : MonoBehaviour
{
    [Header("References")]
    public SpriteInventoryGrid grid;
    public GameObject cellPrefab; // (1,1) 크기의 기본 셀 프리팹

    [Header("Spawn Settings")]
    [Tooltip("블록들을 생성할 영역 (Transform)")]
    public Transform spawnArea;
    public float blockSpacing = 1.5f; // 블록 간 간격
    public int blocksPerRow = 4; // 한 줄에 배치할 블록 수

    [Header("Tetromino Sprites (7 types)")]
    [Tooltip("I: 4x1 블록")]
    public Sprite tetrominoI;
    [Tooltip("O: 2x2 블록")]
    public Sprite tetrominoO;
    [Tooltip("T: T자 블록")]
    public Sprite tetrominoT;
    [Tooltip("S: S자 블록")]
    public Sprite tetrominoS;
    [Tooltip("Z: Z자 블록")]
    public Sprite tetrominoZ;
    [Tooltip("J: J자 블록")]
    public Sprite tetrominoJ;
    [Tooltip("L: L자 블록")]
    public Sprite tetrominoL;

    [Header("Pentomino Sprites (12 types)")]
    [Tooltip("F: F자 블록")]
    public Sprite pentominoF;
    [Tooltip("I: 5x1 블록")]
    public Sprite pentominoI;
    [Tooltip("L: L자 블록 (5칸)")]
    public Sprite pentominoL;
    [Tooltip("N: N자 블록")]
    public Sprite pentominoN;
    [Tooltip("P: P자 블록")]
    public Sprite pentominoP;
    [Tooltip("T: T자 블록 (5칸)")]
    public Sprite pentominoT;
    [Tooltip("U: U자 블록")]
    public Sprite pentominoU;
    [Tooltip("V: V자 블록")]
    public Sprite pentominoV;
    [Tooltip("W: W자 블록")]
    public Sprite pentominoW;
    [Tooltip("X: X자 블록 (십자가)")]
    public Sprite pentominoX;
    [Tooltip("Y: Y자 블록")]
    public Sprite pentominoY;
    [Tooltip("Z: Z자 블록 (5칸)")]
    public Sprite pentominoZ;

    // 블록 데이터 리스트
    private List<BlockData> tetrominoBlocks = new List<BlockData>();
    private List<BlockData> pentominoBlocks = new List<BlockData>();

    // CompositeBlockBuilder
    private CompositeBlockBuilder blockBuilder;

    /// <summary>
    /// 블록 데이터 구조체
    /// </summary>
    [System.Serializable]
    public struct BlockData
    {
        public string name;
        public Sprite sprite;
        public Vector2Int[] shape; // 블록이 차지하는 상대 좌표들
        public Color color;

        public BlockData(string name, Sprite sprite, Vector2Int[] shape, Color color)
        {
            this.name = name;
            this.sprite = sprite;
            this.shape = shape;
            this.color = color;
        }
    }

    void Start()
    {
        if (grid == null)
        {
            Debug.LogError("❌ Grid reference is null! Please assign SpriteInventoryGrid.");
            return;
        }

        if (cellPrefab == null)
        {
            Debug.LogError("❌ CellPrefab is null! Please assign a cell prefab.");
            return;
        }

        // CompositeBlockBuilder 초기화
        blockBuilder = gameObject.AddComponent<CompositeBlockBuilder>();
        blockBuilder.cellPrefab = cellPrefab;
        blockBuilder.grid = grid;

        // 블록 데이터 초기화
        InitializeTetrominoBlocks();
        InitializePentominoBlocks();

        // 블록 생성 (1초 후)
        Invoke("CreateCompositeBlocks", 1f);
    }

    /// <summary>
    /// 테트로미노 블록 데이터 초기화
    /// </summary>
    void InitializeTetrominoBlocks()
    {
        // I 블록 (4x1)
        tetrominoBlocks.Add(new BlockData(
            "Tetromino I",
            tetrominoI,
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0) },
            new Color(0f, 1f, 1f) // 청록색
        ));

        // O 블록 (2x2)
        tetrominoBlocks.Add(new BlockData(
            "Tetromino O",
            tetrominoO,
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(0, 1), new Vector2Int(1, 1) },
            new Color(1f, 1f, 0f) // 노란색
        ));

        // T 블록
        tetrominoBlocks.Add(new BlockData(
            "Tetromino T",
            tetrominoT,
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 0) },
            new Color(0.6f, 0f, 1f) // 보라색
        ));

        // S 블록
        tetrominoBlocks.Add(new BlockData(
            "Tetromino S",
            tetrominoS,
            new Vector2Int[] { new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) },
            new Color(0f, 1f, 0f) // 초록색
        ));

        // Z 블록
        tetrominoBlocks.Add(new BlockData(
            "Tetromino Z",
            tetrominoZ,
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Color(1f, 0f, 0f) // 빨간색
        ));

        // J 블록
        tetrominoBlocks.Add(new BlockData(
            "Tetromino J",
            tetrominoJ,
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Color(0f, 0f, 1f) // 파란색
        ));

        // L 블록
        tetrominoBlocks.Add(new BlockData(
            "Tetromino L",
            tetrominoL,
            new Vector2Int[] { new Vector2Int(2, 1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Color(1f, 0.5f, 0f) // 주황색
        ));

        Debug.Log($"✅ Initialized {tetrominoBlocks.Count} Tetromino blocks");
    }

    /// <summary>
    /// 펜토미노 블록 데이터 초기화
    /// </summary>
    void InitializePentominoBlocks()
    {
        // F 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino F",
            pentominoF,
            new Vector2Int[] { new Vector2Int(1, 2), new Vector2Int(2, 2), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0) },
            new Color(0.8f, 0.4f, 0.8f)
        ));

        // I 블록 (5x1)
        pentominoBlocks.Add(new BlockData(
            "Pentomino I",
            pentominoI,
            new Vector2Int[] { new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(3, 0), new Vector2Int(4, 0) },
            new Color(0.2f, 0.8f, 1f)
        ));

        // L 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino L",
            pentominoL,
            new Vector2Int[] { new Vector2Int(0, 3), new Vector2Int(0, 2), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0) },
            new Color(1f, 0.6f, 0.2f)
        ));

        // N 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino N",
            pentominoN,
            new Vector2Int[] { new Vector2Int(1, 3), new Vector2Int(1, 2), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 1) },
            new Color(0.9f, 0.3f, 0.3f)
        ));

        // P 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino P",
            pentominoP,
            new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(0, 0) },
            new Color(0.4f, 0.9f, 0.4f)
        ));

        // T 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino T",
            pentominoT,
            new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(2, 2), new Vector2Int(1, 1), new Vector2Int(1, 0) },
            new Color(0.7f, 0.3f, 0.9f)
        ));

        // U 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino U",
            pentominoU,
            new Vector2Int[] { new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0), new Vector2Int(2, 1) },
            new Color(0.3f, 0.5f, 1f)
        ));

        // V 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino V",
            pentominoV,
            new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(0, 1), new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Color(1f, 0.4f, 0.6f)
        ));

        // W 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino W",
            pentominoW,
            new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Color(0.5f, 0.8f, 0.3f)
        ));

        // X 블록 (십자가)
        pentominoBlocks.Add(new BlockData(
            "Pentomino X",
            pentominoX,
            new Vector2Int[] { new Vector2Int(1, 2), new Vector2Int(0, 1), new Vector2Int(1, 1), new Vector2Int(2, 1), new Vector2Int(1, 0) },
            new Color(1f, 0.8f, 0.2f)
        ));

        // Y 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino Y",
            pentominoY,
            new Vector2Int[] { new Vector2Int(1, 3), new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(1, 1), new Vector2Int(1, 0) },
            new Color(0.6f, 0.9f, 0.8f)
        ));

        // Z 블록
        pentominoBlocks.Add(new BlockData(
            "Pentomino Z",
            pentominoZ,
            new Vector2Int[] { new Vector2Int(0, 2), new Vector2Int(1, 2), new Vector2Int(1, 1), new Vector2Int(1, 0), new Vector2Int(2, 0) },
            new Color(0.9f, 0.5f, 0.2f)
        ));

        Debug.Log($"✅ Initialized {pentominoBlocks.Count} Pentomino blocks");
    }

    /// <summary>
    /// Composite Block 방식으로 블록 생성
    /// </summary>
    void CreateCompositeBlocks()
    {
        if (spawnArea == null)
        {
            Debug.LogError("❌ Spawn Area is not assigned!");
            return;
        }

        Debug.Log("🎮 Creating Composite Blocks in spawn area...");

        Vector3 startPos = spawnArea.position;
        int row = 0;
        int col = 0;

        // 테트로미노 블록 생성
        Debug.Log("🔷 Spawning Tetromino blocks (Composite)...");
        foreach (var blockData in tetrominoBlocks)
        {
            if (blockData.sprite != null)
            {
                Vector3 spawnPos = startPos + new Vector3(col * blockSpacing, -row * blockSpacing, 0f);
                CreateCompositeBlock(blockData, spawnPos);

                col++;
                if (col >= blocksPerRow)
                {
                    col = 0;
                    row++;
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Sprite missing for {blockData.name}");
            }
        }

        // 펜토미노 블록 생성
        Debug.Log("🔶 Spawning Pentomino blocks (Composite)...");
        foreach (var blockData in pentominoBlocks)
        {
            if (blockData.sprite != null)
            {
                Vector3 spawnPos = startPos + new Vector3(col * blockSpacing, -row * blockSpacing, 0f);
                CreateCompositeBlock(blockData, spawnPos);

                col++;
                if (col >= blocksPerRow)
                {
                    col = 0;
                    row++;
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ Sprite missing for {blockData.name}");
            }
        }

        int totalBlocks = tetrominoBlocks.Count + pentominoBlocks.Count;
        Debug.Log($"✅ Created {totalBlocks} composite blocks in spawn area!");
    }

    /// <summary>
    /// CompositeBlockBuilder를 사용하여 블록 생성
    /// </summary>
    void CreateCompositeBlock(BlockData blockData, Vector3 worldPosition)
    {
        if (blockBuilder == null)
        {
            Debug.LogError("❌ BlockBuilder is null!");
            return;
        }

        // CompositeBlockBuilder로 블록 생성
        GameObject blockObj = blockBuilder.CreateCompositeBlock(
            blockData.name,
            blockData.shape,
            blockData.sprite,
            blockData.color,
            worldPosition
        );

        if (blockObj != null)
        {
            Debug.Log($"✅ Created composite block: {blockData.name} with {blockData.shape.Length} cells");
        }
        else
        {
            Debug.LogError($"❌ Failed to create {blockData.name}");
        }
    }

    void Update()
    {
        // 키보드 단축키
        if (Input.GetKeyDown(KeyCode.P))
        {
            grid.PrintGridState();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllItems();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            CreateCompositeBlocks();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log($"ℹ️ {grid.GetGridInfo()}");
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            grid.TogglePurchaseMode();
        }
    }

    void ClearAllItems()
    {
        var items = grid.GetPlacedItems();
        for (int i = items.Count - 1; i >= 0; i--)
        {
            grid.RemoveItem(items[i]);
        }
        Debug.Log("🧹 Cleared all items!");
    }
}
