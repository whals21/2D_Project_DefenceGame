using System.Collections.Generic;
using UnityEngine;

public class BlockTest : MonoBehaviour
{
    public BlockFactory blockFactory;
    public Vector3 spawnPosition = new Vector3(15, 5, 0); // 그리드 옆에 생성
    private List<Block> createdBlocks = new List<Block>();

    void Start()
    {
        if (blockFactory == null)
        {
            blockFactory = FindObjectOfType<BlockFactory>();
        }

        if (blockFactory == null)
        {
            Debug.LogWarning("BlockTest: BlockFactory를 찾을 수 없습니다. Inspector에서 할당해주세요.");
        }

        // 키보드 입력 안내
        Debug.Log("=== 블록 테스트 컨트롤 ===");
        Debug.Log("1: I 블록 생성");
        Debug.Log("2: O 블록 생성");
        Debug.Log("3: T 블록 생성");
        Debug.Log("4: L 블록 생성");
        Debug.Log("5: J 블록 생성");
        Debug.Log("6: S 블록 생성");
        Debug.Log("7: Z 블록 생성");
        Debug.Log("8: 펜토미노 F 생성");
        Debug.Log("9: 펜토미노 P 생성");
        Debug.Log("C: 모든 블록 제거");
        Debug.Log("Space: 랜덤 블록 생성");
        Debug.Log("========================");
    }

    void Update()
    {
        if (blockFactory == null) return;

        // 숫자 키로 블록 생성
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            CreateBlock(BlockFactory.CreateTetrisI());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            CreateBlock(BlockFactory.CreateTetrisO());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            CreateBlock(BlockFactory.CreateTetrisT());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            CreateBlock(BlockFactory.CreateTetrisL());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            CreateBlock(BlockFactory.CreateTetrisJ());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            CreateBlock(BlockFactory.CreateTetrisS());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            CreateBlock(BlockFactory.CreateTetrisZ());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            CreateBlock(BlockFactory.CreatePentominoF());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            CreateBlock(BlockFactory.CreatePentominoP());
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            ClearAllBlocks();
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            CreateRandomBlock();
        }
    }

    void CreateBlock(BlockData blockData)
    {
        if (blockFactory == null)
        {
            Debug.LogError("BlockTest: BlockFactory가 설정되지 않았습니다.");
            return;
        }

        // 약간씩 다른 위치에 생성 (겹치지 않게)
        Vector3 pos = spawnPosition + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
        Block block = blockFactory.CreateBlock(blockData, pos);

        if (block != null)
        {
            createdBlocks.Add(block);
            Debug.Log($"블록 생성: {blockData.blockName} - 위치: {pos}");
        }
    }

    void CreateRandomBlock()
    {
        BlockData randomBlock = null;
        int random = Random.Range(0, 9);

        switch (random)
        {
            case 0:
                randomBlock = BlockFactory.CreateTetrisI();
                break;
            case 1:
                randomBlock = BlockFactory.CreateTetrisO();
                break;
            case 2:
                randomBlock = BlockFactory.CreateTetrisT();
                break;
            case 3:
                randomBlock = BlockFactory.CreateTetrisL();
                break;
            case 4:
                randomBlock = BlockFactory.CreateTetrisJ();
                break;
            case 5:
                randomBlock = BlockFactory.CreateTetrisS();
                break;
            case 6:
                randomBlock = BlockFactory.CreateTetrisZ();
                break;
            case 7:
                randomBlock = BlockFactory.CreatePentominoF();
                break;
            case 8:
                randomBlock = BlockFactory.CreatePentominoP();
                break;
        }

        if (randomBlock != null)
        {
            CreateBlock(randomBlock);
        }
    }

    void ClearAllBlocks()
    {
        foreach (Block block in createdBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
        createdBlocks.Clear();
        Debug.Log("모든 블록 제거됨");
    }

    void OnGUI()
    {
        // 화면에 안내 메시지 표시
        GUIStyle style = new GUIStyle();
        style.fontSize = 16;
        style.normal.textColor = Color.white;
        style.alignment = TextAnchor.UpperLeft;

        string instructions = "블록 테스트 컨트롤:\n" +
                            "1-7: 테트리스 블록 생성\n" +
                            "8-9: 펜토미노 블록 생성\n" +
                            "Space: 랜덤 블록 생성\n" +
                            "C: 모든 블록 제거\n" +
                            "R: 드래그 중 블록 회전";

        GUI.Label(new Rect(10, 10, 400, 200), instructions, style);
    }
}

