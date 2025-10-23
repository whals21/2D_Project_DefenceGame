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
}
