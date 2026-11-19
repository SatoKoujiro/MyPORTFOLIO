using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(BoxCollider2D))]
public class Block : MonoBehaviour
{
    public int X { get; private set; }
    public int Y { get; private set; }
    public BlockType Type { get; private set; }

    private BoardManager board;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Initialize(BoardManager owner, int x, int y, BlockType type)
    {
        board = owner;
        SetPosition(x, y);
        SetType(type);
    }

    public void SetPosition(int x, int y)
    {
        X = x;
        Y = y;
    }

    public void SetType(BlockType type)
    {
        Type = type;
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = GetColor(type);
    }

    public void SetSelected(bool selected)
    {
        transform.localScale = selected ? Vector3.one * 1.15f : Vector3.one;
    }

    private Color GetColor(BlockType type)
    {
        switch (type)
        {
            case BlockType.Red: return new Color(1f, 0.25f, 0.25f);
            case BlockType.Blue: return new Color(0.25f, 0.55f, 1f);
            case BlockType.Green: return new Color(0.25f, 0.85f, 0.4f);
            case BlockType.Yellow: return new Color(1f, 0.85f, 0.2f);
            case BlockType.Purple: return new Color(0.7f, 0.35f, 1f);
            default: return Color.white;
        }
    }

    private void OnMouseDown()
    {
        if (board != null) board.OnBlockClicked(this);
    }
}
