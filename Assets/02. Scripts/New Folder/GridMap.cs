using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridMap
{
    public int width;
    public int height;
    public Dictionary<Vector2Int, Cell> cells = new Dictionary<Vector2Int, Cell>();


    public void AddCell(Vector2Int pos)
    {
        if (!cells.ContainsKey(pos))
        {
            cells[pos] = new Cell(pos.x, pos.y);
            Debug.Log($"셀 추가됨: {pos}");
        }
    }

    public GridMap(int width, int height)
    {
        this.width = width;
        this.height = height;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                AddCell(new Vector2Int(x, y));
            }
        }
    }

    public bool IsOccupied(Vector2Int pos)
    {
        return cells.ContainsKey(pos) && cells[pos].isOccupied;
    }

    public void SetOccupied(Vector2Int pos, bool value)
    {
        if (cells.ContainsKey(pos))
            cells[pos].isOccupied = value;
    }

    public bool HasCell(Vector2Int pos)
    {
        return cells.ContainsKey(pos);
    }

}
