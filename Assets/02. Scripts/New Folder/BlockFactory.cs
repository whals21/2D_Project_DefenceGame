using System.Collections.Generic;
using UnityEngine;

public class BlockFactory : MonoBehaviour
{
    public GameObject blockPrefab; // Block 컴포넌트가 있는 프리팹
    public GameObject cellPrefab; // 블록을 구성하는 셀 프리팹

    // 테트리스 블록 예제들
    public static BlockData CreateTetrisI()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(3, 0)
        };
        return new BlockData("I", positions, Color.cyan);
    }

    public static BlockData CreateTetrisO()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        };
        return new BlockData("O", positions, Color.yellow);
    }

    public static BlockData CreateTetrisT()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1)
        };
        return new BlockData("T", positions, Color.magenta);
    }

    public static BlockData CreateTetrisL()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2),
            new Vector2Int(1, 2)
        };
        return new BlockData("L", positions, new Color(1f, 0.5f, 0f)); // Orange
    }

    public static BlockData CreateTetrisJ()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(1, 2),
            new Vector2Int(0, 2)
        };
        return new BlockData("J", positions, Color.blue);
    }

    public static BlockData CreateTetrisS()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1)
        };
        return new BlockData("S", positions, Color.green);
    }

    public static BlockData CreateTetrisZ()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1)
        };
        return new BlockData("Z", positions, Color.red);
    }

    // 펜토미노 블록 예제들
    public static BlockData CreatePentominoF()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(2, 1),
            new Vector2Int(1, 2)
        };
        return new BlockData("F", positions, Color.green);
    }

    public static BlockData CreatePentominoP()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 1),
            new Vector2Int(0, 2)
        };
        return new BlockData("P", positions, Color.yellow);
    }

    // ===== 아이템 타워용 1-3칸 블록 모양들 =====

    /// <summary>
    /// 1칸 블록 (Single Cell)
    /// </summary>
    public static BlockData CreateItemSingle()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0)
        };
        return new BlockData("Item_Single", positions, Color.yellow);
    }

    /// <summary>
    /// 2칸 블록 - 가로 (Horizontal Line)
    /// </summary>
    public static BlockData CreateItemLine2H()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0)
        };
        return new BlockData("Item_Line2H", positions, Color.yellow);
    }

    /// <summary>
    /// 2칸 블록 - 세로 (Vertical Line)
    /// </summary>
    public static BlockData CreateItemLine2V()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1)
        };
        return new BlockData("Item_Line2V", positions, Color.yellow);
    }

    /// <summary>
    /// 2칸 블록 - 대각선 (Diagonal)
    /// </summary>
    public static BlockData CreateItemDiagonal2()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 1)
        };
        return new BlockData("Item_Diagonal2", positions, Color.yellow);
    }

    /// <summary>
    /// 3칸 블록 - 가로 (Horizontal Line)
    /// </summary>
    public static BlockData CreateItemLine3H()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0)
        };
        return new BlockData("Item_Line3H", positions, Color.yellow);
    }

    /// <summary>
    /// 3칸 블록 - 세로 (Vertical Line)
    /// </summary>
    public static BlockData CreateItemLine3V()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(0, 2)
        };
        return new BlockData("Item_Line3V", positions, Color.yellow);
    }

    /// <summary>
    /// 3칸 블록 - L자 (L Shape)
    /// </summary>
    public static BlockData CreateItemL3()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, 1),
            new Vector2Int(1, 0)
        };
        return new BlockData("Item_L3", positions, Color.yellow);
    }

    /// <summary>
    /// 3칸 블록 - T자 (T Shape)
    /// </summary>
    public static BlockData CreateItemT3()
    {
        List<Vector2Int> positions = new List<Vector2Int>
        {
            new Vector2Int(0, 0),
            new Vector2Int(1, 0),
            new Vector2Int(2, 0),
            new Vector2Int(1, 1)
        };
        return new BlockData("Item_T3", positions, Color.yellow);
    }

    // 블록 생성
    public Block CreateBlock(BlockData data, Vector3 position)
    {
        if (blockPrefab == null)
        {
            Debug.LogError("BlockFactory: blockPrefab이 설정되지 않았습니다.");
            return null;
        }

        GameObject blockObj = Instantiate(blockPrefab, position, Quaternion.identity);
        Block block = blockObj.GetComponent<Block>();

        if (block == null)
        {
            block = blockObj.AddComponent<Block>();
        }

        block.blockData = data;
        block.cellPrefab = cellPrefab;
        block.gridPosition = new Vector2Int(Mathf.RoundToInt(position.x), Mathf.RoundToInt(position.y));

        // ✨ NEW: 블록을 한 번 회전시킨 상태로 스폰
        block.Rotate();

        block.UpdateVisualization();

        // BlockDragger 추가
        if (blockObj.GetComponent<BlockDragger>() == null)
        {
            blockObj.AddComponent<BlockDragger>();
        }

        return block;
    }
}

