using UnityEngine;

public static class PlatformerLevelBuilder
{
    private const float PlayerHeight = 0.96f;

    public static PlatformerLevelData Build()
    {
        PlatformerLevelData level = new PlatformerLevelData();

        AddPlatform(level, -1f, 0f, 13f);
        AddPlatform(level, 14.5f, 0f, 9f);
        AddPlatform(level, 26.5f, 0f, 9f);
        AddPlatform(level, 39f, 0f, 10f);
        AddPlatform(level, 53f, 0f, 11f);
        AddPlatform(level, 7f, 1.35f, 3.4f);
        AddPlatform(level, 18f, 1.45f, 3f);
        AddPlatform(level, 29f, 1.45f, 3.8f);
        AddPlatform(level, 41f, 1.45f, 3.3f);
        AddPlatform(level, 51f, 1.45f, 3.5f);

        AddHazard(level, 4.4f, 0f, 1.2f);
        AddHazard(level, 15.4f, 0f, 1.15f);
        AddHazard(level, 32.4f, 0f, 1.2f);
        AddHazard(level, 44f, 0f, 1.2f);
        AddHazard(level, 54f, 0f, 1.2f);

        AddCoin(level, 6.9f, 2.25f);
        AddCoin(level, 10.6f, 1.25f);
        AddCoin(level, 17.9f, 2.25f);
        AddCoin(level, 22.3f, 1.35f);
        AddCoin(level, 29f, 2.5f);
        AddCoin(level, 36f, 1.25f);
        AddCoin(level, 41f, 2.25f);
        AddCoin(level, 49f, 1.35f);
        AddCoin(level, 51f, 2.55f);

        level.SpawnPoint = new Vector2(-3.2f, PlayerHeight * 0.5f + 0.05f);
        CreateGoal(level);
        return level;
    }

    private static void AddPlatform(PlatformerLevelData level, float centerX, float topY, float width)
    {
        const float height = 0.62f;
        Vector2 center = new Vector2(centerX, topY - height * 0.5f);
        PlatformerVisuals.CreateBox("Platform", center, new Vector2(width, height),
            new Color(0.2f, 0.3f, 0.4f), 2);
        PlatformerVisuals.CreateBox("Grass top", new Vector2(centerX, topY - 0.02f),
            new Vector2(width, 0.1f), new Color(0.36f, 0.72f, 0.58f), 3);
        level.Platforms.Add(Rect.MinMaxRect(centerX - width * 0.5f, topY - height,
            centerX + width * 0.5f, topY));
    }

    private static void AddHazard(PlatformerLevelData level, float centerX, float surfaceY, float width)
    {
        const float height = 0.62f;
        int count = Mathf.Max(2, Mathf.RoundToInt(width / 0.48f));
        for (int i = 0; i < count; i++)
        {
            float spikeWidth = width / count;
            float x = centerX - width * 0.5f + spikeWidth * (i + 0.5f);
            PlatformerVisuals.CreateTriangle("Spike", new Vector2(x, surfaceY + height * 0.5f),
                new Vector2(spikeWidth, height), new Color(0.91f, 0.38f, 0.4f), 7);
        }

        level.Hazards.Add(Rect.MinMaxRect(centerX - width * 0.5f, surfaceY + 0.05f,
            centerX + width * 0.5f, surfaceY + height * 0.82f));
    }

    private static void AddCoin(PlatformerLevelData level, float x, float y)
    {
        GameObject coin = PlatformerVisuals.CreateCircle("Coin", new Vector2(x, y),
            new Vector2(0.52f, 0.52f), new Color(1f, 0.72f, 0.16f), 8);
        GameObject glint = PlatformerVisuals.CreateCircle("Coin glint", new Vector2(x - 0.06f, y + 0.07f),
            new Vector2(0.17f, 0.25f), new Color(1f, 0.94f, 0.58f), 9);
        glint.transform.SetParent(coin.transform, true);
        level.Coins.Add(new PlatformerCoin { Object = coin, Position = new Vector2(x, y) });
    }

    private static void CreateGoal(PlatformerLevelData level)
    {
        const float goalX = 59f;
        PlatformerVisuals.CreateBox("Goal post", new Vector2(goalX, 2.35f),
            new Vector2(0.16f, 4.7f), new Color(0.96f, 0.94f, 0.79f), 4);
        PlatformerVisuals.CreateCircle("Goal orb", new Vector2(goalX, 4.83f),
            new Vector2(0.34f, 0.34f), new Color(1f, 0.82f, 0.35f), 5);
        GameObject flag = PlatformerVisuals.CreateBox("Goal flag", new Vector2(goalX - 0.65f, 4.32f),
            new Vector2(1.05f, 0.62f), new Color(0.91f, 0.38f, 0.4f), 5);
        level.GoalFlag = flag.GetComponent<SpriteRenderer>();
    }
}
