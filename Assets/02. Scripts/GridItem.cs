using UnityEngine;

[System.Serializable]
public class GridItem 
{
    public string itemName;
    public int width;
    public int height;
    public Sprite sprite;
    public Color color = Color.white;
    
    public Vector2Int gridPosition = new Vector2Int(-1, -1);
    public int rotation = 0; // 0, 90, 180, 270

    // 블록의 실제 형태 (상대 좌표 배열) - 테트리스/펜토미노 블록용
    public Vector2Int[] shape = null;

    public GridItem(string name, int w, int h, Sprite spr = null)
    {
        itemName = name;
        width = w;
        height = h;
        sprite = spr;
    }
    
    public Vector2Int GetRotatedSize()
    {
        if (rotation == 90 || rotation == 270)
            return new Vector2Int(height, width);
        return new Vector2Int(width, height);
    }

    public void Rotate()
    {
        rotation = (rotation + 90) % 360;
    }

    // Shape가 있는지 확인
    public bool HasShape()
    {
        return shape != null && shape.Length > 0;
    }
}
