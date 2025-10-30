using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockData
{
    public string blockName;
    public List<Vector2Int> cellPositions; // 블록을 구성하는 셀들의 상대 위치 (로컬 좌표)
    public Color blockColor = Color.white;

    public BlockData(string name, List<Vector2Int> positions, Color color)
    {
        blockName = name;
        cellPositions = new List<Vector2Int>(positions);
        blockColor = color;
    }

    // 블록을 90도 회전
    public BlockData Rotate90()
    {
        List<Vector2Int> rotatedPositions = new List<Vector2Int>();
        foreach (Vector2Int pos in cellPositions)
        {
            // 90도 시계방향 회전: (x, y) -> (y, -x)
            rotatedPositions.Add(new Vector2Int(pos.y, -pos.x));
        }
        return new BlockData(blockName, rotatedPositions, blockColor);
    }

    // 블록의 중심점 계산 (회전 기준점)
    public Vector2Int GetCenter()
    {
        if (cellPositions.Count == 0) return Vector2Int.zero;

        int sumX = 0;
        int sumY = 0;
        foreach (Vector2Int pos in cellPositions)
        {
            sumX += pos.x;
            sumY += pos.y;
        }
        return new Vector2Int(sumX / cellPositions.Count, sumY / cellPositions.Count);
    }
}

