using UnityEngine;

public class Block : MonoBehaviour
{
    public string blockName;
    public Color blockColor;
    public SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer.color = blockColor;
    }
}