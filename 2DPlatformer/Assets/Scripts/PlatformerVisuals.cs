using UnityEngine;

public static class PlatformerVisuals
{
    private static Sprite squareSprite;
    private static Sprite circleSprite;
    private static Sprite triangleSprite;

    public static Sprite SquareSprite => squareSprite ?? (squareSprite = CreateSolidSprite());
    public static Sprite CircleSprite => circleSprite ?? (circleSprite = CreateCircleSprite(64));
    private static Sprite TriangleSprite => triangleSprite ?? (triangleSprite = CreateTriangleSprite(64));

    public static GameObject CreateCircle(string name, Vector2 position, Vector2 size, Color color, int order)
    {
        return CreateSpriteObject(name, position, size, color, order, CircleSprite);
    }

    public static GameObject CreateBox(string name, Vector2 position, Vector2 size, Color color, int order)
    {
        return CreateSpriteObject(name, position, size, color, order, SquareSprite);
    }

    public static GameObject CreateTriangle(string name, Vector2 position, Vector2 size, Color color, int order)
    {
        return CreateSpriteObject(name, position, size, color, order, TriangleSprite);
    }

    public static void AddPart(Transform parent, string name, Vector2 position, Vector2 size,
        Color color, int order, Sprite sprite)
    {
        GameObject part = new GameObject(name);
        part.transform.SetParent(parent, false);
        part.transform.localPosition = position;
        part.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = part.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
    }

    private static GameObject CreateSpriteObject(string name, Vector2 position, Vector2 size,
        Color color, int order, Sprite sprite)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.position = position;
        gameObject.transform.localScale = new Vector3(size.x, size.y, 1f);
        SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = color;
        renderer.sortingOrder = order;
        return gameObject;
    }

    private static Sprite CreateSolidSprite()
    {
        Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        texture.SetPixel(0, 0, Color.white);
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    private static Sprite CreateCircleSprite(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        float center = (resolution - 1) * 0.5f;
        float radius = resolution * 0.48f;
        for (int y = 0; y < resolution; y++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                float alpha = Mathf.Clamp01(radius + 1f - distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution),
            new Vector2(0.5f, 0.5f), resolution);
    }

    private static Sprite CreateTriangleSprite(int resolution)
    {
        Texture2D texture = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        for (int y = 0; y < resolution; y++)
        {
            float halfWidth = (y / (float)(resolution - 1)) * (resolution - 1) * 0.5f;
            float center = (resolution - 1) * 0.5f;
            int left = Mathf.CeilToInt(center - halfWidth);
            int right = Mathf.FloorToInt(center + halfWidth);
            for (int x = left; x <= right; x++)
                texture.SetPixel(x, y, Color.white);
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, resolution, resolution),
            new Vector2(0.5f, 0.5f), resolution);
    }
}
