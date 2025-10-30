using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public Vector2Int position;
    public bool isOccupied;

    public Cell(int x, int y)
    {
        position = new Vector2Int(x, y);
        isOccupied = false;
    }
}

