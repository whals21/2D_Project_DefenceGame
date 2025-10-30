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

    public void ExpandBottom(Vector2Int pos)
    {
        // 특정 위치의 셀만 추가 (한 칸)
        AddCell(pos);
        // 맵 크기 동적 계산
        UpdateMapBounds();
        Debug.Log($"맵 아래로 확장: {width}x{height} - 위치: {pos}");
    }

    public void ExpandTop(Vector2Int pos)
    {
        // 특정 위치의 셀만 추가 (한 칸)
        AddCell(pos);
        // 맵 크기 동적 계산
        UpdateMapBounds();
        Debug.Log($"맵 위로 확장: {width}x{height} - 위치: {pos}");
    }

    public void ExpandLeft(Vector2Int pos)
    {
        // 특정 위치의 셀만 추가 (한 칸)
        AddCell(pos);
        // 맵 크기 동적 계산
        UpdateMapBounds();
        Debug.Log($"맵 왼쪽으로 확장: {width}x{height} - 위치: {pos}");
    }

    public void ExpandRight(Vector2Int pos)
    {
        // 특정 위치의 셀만 추가 (한 칸)
        AddCell(pos);
        // 맵 크기 동적 계산
        UpdateMapBounds();
        Debug.Log($"맵 오른쪽으로 확장: {width}x{height} - 위치: {pos}");
    }

    public void UpdateMapBounds()
    {
        // cells에 있는 모든 셀을 기반으로 width와 height 계산
        if (cells.Count == 0)
        {
            width = 0;
            height = 0;
            return;
        }

        int minX = int.MaxValue;
        int maxX = int.MinValue;
        int minY = int.MaxValue;
        int maxY = int.MinValue;

        foreach (var kvp in cells)
        {
            Vector2Int pos = kvp.Key;
            if (pos.x < minX) minX = pos.x;
            if (pos.x > maxX) maxX = pos.x;
            if (pos.y < minY) minY = pos.y;
            if (pos.y > maxY) maxY = pos.y;
        }

        width = maxX - minX + 1;
        height = maxY - minY + 1;
    }

    public void RedrawMap()
    {
        // 현재 width와 height에 맞춰 모든 셀을 다시 생성
        Dictionary<Vector2Int, Cell> newCells = new Dictionary<Vector2Int, Cell>();
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (cells.ContainsKey(pos))
                {
                    // 기존 셀이 있으면 유지
                    newCells[pos] = cells[pos];
                }
                else
                {
                    // 새 셀 생성
                    newCells[pos] = new Cell(x, y);
                }
            }
        }
        cells = newCells;
        Debug.Log($"맵 재구성 완료: {width}x{height}");
    }

}
