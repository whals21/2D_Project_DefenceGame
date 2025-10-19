using UnityEngine;

public class GridMap : MonoBehaviour
{
    public int width = 10;   // 가방 너비
    public int height = 6;   // 가방 높이
    public GridCell[,] grid; // 2D 배열로 맵 구성

    void Start()
    {
        grid = new GridCell[width, height];

        // 초기화: 모든 셀을 빈 상태로 설정
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new GridCell();
            }
        }
    }

    // 블록 배치 함수 예시
    public bool PlaceBlock(int x, int y, Block block)
    {
        if (IsInBounds(x, y) && grid[x, y].isEmpty)
        {
            grid[x, y].block = block;
            grid[x, y].isEmpty = false;
            return true;
        }
        return false;
    }

    public bool IsInBounds(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }
}

public class GridCell
{
    public bool isEmpty = true;
    public Block block = null;
}

public class Block
{
    public string blockName;
    public Color blockColor;
    // 블록의 기능, 타입 등 추가 가능
}