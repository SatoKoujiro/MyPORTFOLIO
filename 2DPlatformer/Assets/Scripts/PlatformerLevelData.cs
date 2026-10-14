using System.Collections.Generic;
using UnityEngine;

public sealed class PlatformerLevelData
{
    public readonly List<Rect> Platforms = new List<Rect>();
    public readonly List<Rect> Hazards = new List<Rect>();
    public readonly List<PlatformerCoin> Coins = new List<PlatformerCoin>();
    public Vector2 SpawnPoint;
    public SpriteRenderer GoalFlag;
}

public sealed class PlatformerCoin
{
    public GameObject Object;
    public Vector2 Position;
    public bool Collected;

    public Rect Bounds => Rect.MinMaxRect(
        Position.x - 0.34f, Position.y - 0.34f,
        Position.x + 0.34f, Position.y + 0.34f);
}
