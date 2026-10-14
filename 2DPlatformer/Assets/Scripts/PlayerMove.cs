using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlayerMove : MonoBehaviour
{
    private const float Width = 0.72f;
    private const float Height = 0.96f;
    private const float HorizontalSpeed = 7.2f;
    private const float JumpSpeed = 10.2f;
    private const float Gravity = 27f;
    private const float MaximumFallSpeed = 22f;

    private List<Rect> platforms;
    private Vector2 spawnPoint;
    private Vector2 velocity;
    private bool grounded;

    public Vector2 Position => transform.position;

    public Rect Bounds
    {
        get
        {
            Vector2 position = transform.position;
            return Rect.MinMaxRect(
                position.x - Width * 0.5f, position.y - Height * 0.5f,
                position.x + Width * 0.5f, position.y + Height * 0.5f);
        }
    }

    public void Initialize(List<Rect> levelPlatforms, Vector2 startingPoint)
    {
        platforms = levelPlatforms;
        spawnPoint = startingPoint;
        transform.position = new Vector3(spawnPoint.x, spawnPoint.y, 0f);
        CreateAppearance();
    }

    private void Update()
    {
        if (platforms == null)
            return;

        Keyboard keyboard = Keyboard.current;
        bool moveLeft = keyboard != null &&
                        (keyboard.leftArrowKey.isPressed || keyboard.aKey.isPressed);
        bool moveRight = keyboard != null &&
                         (keyboard.rightArrowKey.isPressed || keyboard.dKey.isPressed);
        bool jumpPressed = keyboard != null &&
                           (keyboard.spaceKey.wasPressedThisFrame ||
                            keyboard.upArrowKey.wasPressedThisFrame ||
                            keyboard.wKey.wasPressedThisFrame);

        float deltaTime = Mathf.Min(Time.deltaTime, 0.04f);
        float input = (moveRight ? 1f : 0f) - (moveLeft ? 1f : 0f);
        velocity.x = input * HorizontalSpeed;
        if (input != 0f)
            transform.localScale = new Vector3(Mathf.Sign(input), 1f, 1f);

        if (jumpPressed && grounded)
        {
            velocity.y = JumpSpeed;
            grounded = false;
        }

        velocity.y = Mathf.Max(velocity.y - Gravity * deltaTime, -MaximumFallSpeed);
        MoveHorizontally(velocity.x * deltaTime);
        MoveVertically(velocity.y * deltaTime);
    }

    public void Respawn()
    {
        transform.position = new Vector3(spawnPoint.x, spawnPoint.y, 0f);
        transform.localScale = Vector3.one;
        velocity = Vector2.zero;
        grounded = false;
    }

    private void MoveHorizontally(float amount)
    {
        if (Mathf.Approximately(amount, 0f))
            return;

        Vector2 current = transform.position;
        float targetX = current.x + amount;
        float halfWidth = Width * 0.5f;
        float bottom = current.y - Height * 0.5f;
        float top = current.y + Height * 0.5f;

        foreach (Rect bounds in platforms)
        {
            bool verticalOverlap = top > bounds.yMin + 0.01f && bottom < bounds.yMax - 0.01f;
            if (!verticalOverlap)
                continue;

            if (amount > 0f && current.x + halfWidth <= bounds.xMin + 0.02f &&
                targetX + halfWidth > bounds.xMin)
                targetX = bounds.xMin - halfWidth;
            else if (amount < 0f && current.x - halfWidth >= bounds.xMax - 0.02f &&
                     targetX - halfWidth < bounds.xMax)
                targetX = bounds.xMax + halfWidth;
        }

        transform.position = new Vector3(targetX, current.y, transform.position.z);
    }

    private void MoveVertically(float amount)
    {
        Vector2 current = transform.position;
        float targetY = current.y + amount;
        float halfWidth = Width * 0.5f;
        float halfHeight = Height * 0.5f;
        float left = current.x - halfWidth;
        float right = current.x + halfWidth;
        grounded = false;

        foreach (Rect bounds in platforms)
        {
            bool horizontalOverlap = right > bounds.xMin + 0.01f && left < bounds.xMax - 0.01f;
            if (!horizontalOverlap)
                continue;

            if (amount <= 0f && current.y - halfHeight >= bounds.yMax - 0.04f &&
                targetY - halfHeight <= bounds.yMax)
            {
                targetY = bounds.yMax + halfHeight;
                velocity.y = 0f;
                grounded = true;
            }
            else if (amount > 0f && current.y + halfHeight <= bounds.yMin + 0.04f &&
                     targetY + halfHeight >= bounds.yMin)
            {
                targetY = bounds.yMin - halfHeight;
                velocity.y = 0f;
            }
        }

        transform.position = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    private void CreateAppearance()
    {
        Sprite square = PlatformerVisuals.SquareSprite;
        Sprite circle = PlatformerVisuals.CircleSprite;
        PlatformerVisuals.AddPart(transform, "Body", Vector2.zero, new Vector2(0.66f, 0.77f),
            new Color(0.17f, 0.86f, 0.67f), 20, circle);
        PlatformerVisuals.AddPart(transform, "Cap", new Vector2(0f, 0.31f), new Vector2(0.63f, 0.24f),
            new Color(0.12f, 0.61f, 0.56f), 21, square);
        PlatformerVisuals.AddPart(transform, "Face", new Vector2(0.15f, 0.05f), new Vector2(0.18f, 0.24f),
            new Color(1f, 0.94f, 0.76f), 22, circle);
        PlatformerVisuals.AddPart(transform, "Eye", new Vector2(0.19f, 0.09f), new Vector2(0.06f, 0.09f),
            new Color(0.16f, 0.21f, 0.3f), 23, circle);
        PlatformerVisuals.AddPart(transform, "Feet", new Vector2(0f, -0.38f), new Vector2(0.76f, 0.16f),
            new Color(0.11f, 0.43f, 0.45f), 21, square);
    }
}
