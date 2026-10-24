using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class ActorVisual : MonoBehaviour
{
    private static Sprite sharedSprite;
    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (sharedSprite == null)
        {
            sharedSprite = CreateCircleSprite();
        }

        spriteRenderer.sprite = sharedSprite;
    }

    public void SetStyle(Color color, float size, int sortingOrder)
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
        transform.localScale = Vector3.one * size;
    }

    private static Sprite CreateCircleSprite()
    {
        const int textureSize = 64;
        Texture2D texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        texture.name = "Runtime Actor Circle";
        texture.filterMode = FilterMode.Bilinear;
        Color32[] pixels = new Color32[textureSize * textureSize];
        Vector2 center = new Vector2((textureSize - 1) * 0.5f, (textureSize - 1) * 0.5f);
        float radius = textureSize * 0.47f;

        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                byte alpha = distance <= radius ? (byte)255 : (byte)0;
                pixels[y * textureSize + x] = new Color32(255, 255, 255, alpha);
            }
        }

        texture.SetPixels32(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize);
    }
}