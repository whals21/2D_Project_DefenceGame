using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class InventoryItem
{
    public string itemName;
    public int width;
    public int height;
    public Sprite sprite;
    public Color color = Color.white;
    
    // 현재 그리드상의 위치 (왼쪽 하단 기준)
    public Vector2Int gridPosition;
    
    // 회전 상태 (0, 90, 180, 270도)
    public int rotation = 0;
    
    public InventoryItem(string name, int w, int h, Sprite spr = null)
    {
        itemName = name;
        width = w;
        height = h;
        sprite = spr;
        gridPosition = new Vector2Int(-1, -1); // 배치되지 않음
    }
    
    // 회전 시 실제 차지하는 크기
    public Vector2Int GetRotatedSize()
    {
        if (rotation == 90 || rotation == 270)
            return new Vector2Int(height, width);
        return new Vector2Int(width, height);
    }
    
    // 회전
    public void Rotate()
    {
        rotation = (rotation + 90) % 360;
    }
}
