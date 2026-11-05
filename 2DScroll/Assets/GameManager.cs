using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private const float GroundY = -3.35f;
    private const float PlayerX = -5f;
    private const float PlayerWidth = 0.78f;
    private const float PlayerHeight = 1.1f;
    private const float JumpDuration = 0.78f;
    private const float JumpHeight = 2.5f;
    private const int ObstaclePoolSize = 6;

    private static Sprite squareSprite;
    private static Sprite backgroundSprite;
    private static Material spriteMaterial;

    private readonly List<ScrollItem> backgroundItems = new List<ScrollItem>();
    private readonly List<Obstacle> obstaclePool = new List<Obstacle>();

    private Transform player;
    private Transform playerFace;
    private float cameraHalfWidth;
    private float jumpTimer;
    private float spawnTimer;
    private float score;
    private float worldSpeed;
    private bool isJumping;
    private bool isGameOver;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateGameManager()
    {
        if (FindFirstObjectByType<GameManager>() != null)
        {
            return;
        }

        GameObject manager = new GameObject("Game Manager");
        manager.AddComponent<GameManager>();
    }

    private void Start()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            mainCamera = cameraObject.AddComponent<Camera>();
        }

        mainCamera.orthographic = true;
        mainCamera.orthographicSize = 5f;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.transform.position = new Vector3(0f, 0f, -10f);
        mainCamera.backgroundColor = new Color(0.78f, 0.88f, 0.93f);
        cameraHalfWidth = mainCamera.orthographicSize * mainCamera.aspect;

        CreateWorld();
        ResetRun();
    }

    private void Update()
    {
        if (WasJumpPressed())
        {
            if (isGameOver)
            {
                ResetRun();
            }
            else if (!isJumping)
            {
                isJumping = true;
                jumpTimer = 0f;
            }
        }

        if (isGameOver)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        score += deltaTime * worldSpeed;
        worldSpeed = Mathf.Min(10f, 6f + score * 0.012f);

        UpdateJump(deltaTime);
        UpdateScrolling(deltaTime);
        UpdateObstacles(deltaTime);
        CheckCollision();
    }

    private void CreateWorld()
    {
        CreateSpriteAsset();

        CreateBlock("Road", new Vector2(0f, GroundY - 0.22f), new Vector2(2000f, 0.44f),
            new Color(0.24f, 0.39f, 0.39f), 1);
        CreateBlock("Road edge", new Vector2(0f, GroundY + 0.005f), new Vector2(2000f, 0.07f),
            new Color(0.68f, 0.77f, 0.64f), 2);

        CreateBackgroundImage();
        const float backgroundTileWidth = 16f;
        int tileCount = Mathf.CeilToInt(cameraHalfWidth * 2f / backgroundTileWidth) + 2;
        float firstTileX = -(tileCount - 1) * backgroundTileWidth * 0.5f;
        for (int i = 0; i < tileCount; i++)
        {
            GameObject tile = new GameObject("Looping background " + (i + 1));
            tile.transform.position = new Vector3(firstTileX + i * backgroundTileWidth, 0f, 0f);
            SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = backgroundSprite;
            renderer.sharedMaterial = spriteMaterial;
            renderer.sortingOrder = 0;
            backgroundItems.Add(new ScrollItem(tile.transform, backgroundTileWidth, 0.22f));
        }

        player = CreateBlock("Runner", new Vector2(PlayerX, GroundY + PlayerHeight * 0.5f),
            new Vector2(PlayerWidth, PlayerHeight), new Color(0.98f, 0.48f, 0.27f), 5).transform;
        GameObject face = CreateBlock("Runner face", new Vector2(0.16f, 0.13f),
            new Vector2(0.12f, 0.12f), new Color(0.18f, 0.24f, 0.29f), 6);
        playerFace = face.transform;
        playerFace.SetParent(player, false);

        for (int i = 0; i < ObstaclePoolSize; i++)
        {
            GameObject root = new GameObject("Pooled obstacle " + (i + 1));
            SpriteRenderer renderer = root.AddComponent<SpriteRenderer>();
            renderer.sprite = squareSprite;
            renderer.sharedMaterial = spriteMaterial;
            renderer.sortingOrder = 4;
            renderer.color = new Color(0.29f, 0.55f, 0.42f);
            root.SetActive(false);
            obstaclePool.Add(new Obstacle(root.transform, renderer));
        }
    }

    private void CreateBackgroundImage()
    {
        if (backgroundSprite != null)
        {
            return;
        }

        const int width = 512;
        const int height = 320;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.name = "Generated seamless runner background";
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Repeat;
        Color[] pixels = new Color[width * height];

        for (int x = 0; x < width; x++)
        {
            float u = (float)x / width;
            float backRidge = 0.24f
                              + 0.035f * Mathf.Sin(u * Mathf.PI * 2f * 2f)
                              + 0.018f * Mathf.Sin(u * Mathf.PI * 2f * 5f);
            float frontRidge = 0.19f
                               + 0.028f * Mathf.Sin(u * Mathf.PI * 2f * 3f + 0.7f)
                               + 0.012f * Mathf.Sin(u * Mathf.PI * 2f * 7f);

            for (int y = 0; y < height; y++)
            {
                float v = (float)y / height;
                Color color = Color.Lerp(new Color(0.62f, 0.81f, 0.89f), new Color(0.87f, 0.93f, 0.91f), v);

                if (v < backRidge)
                {
                    color = new Color(0.65f, 0.78f, 0.75f);
                }

                if (v < frontRidge)
                {
                    color = new Color(0.55f, 0.71f, 0.66f);
                }

                float cloudAlpha = Mathf.Max(
                    CloudShape(u, v, 0.18f, 0.72f, 0.055f, 0.018f),
                    CloudShape(u, v, 0.57f, 0.83f, 0.075f, 0.022f));
                if (cloudAlpha > 0f && v > backRidge)
                {
                    color = Color.Lerp(color, new Color(0.97f, 0.98f, 0.94f), cloudAlpha * 0.9f);
                }

                pixels[y * width + x] = color;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, true);
        backgroundSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f), 32f);
        backgroundSprite.name = "Generated seamless runner background";
    }

    private float CloudShape(float u, float v, float centerX, float centerY, float radiusX, float radiusY)
    {
        float dx = Mathf.Abs(u - centerX);
        dx = Mathf.Min(dx, 1f - dx);
        float normalizedX = dx / radiusX;
        float normalizedY = Mathf.Abs(v - centerY) / radiusY;
        float distance = normalizedX * normalizedX + normalizedY * normalizedY;
        return 1f - Mathf.SmoothStep(0.72f, 1f, distance);
    }

    private void CreateSpriteAsset()
    {
        if (squareSprite == null)
        {
            squareSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f),
                new Vector2(0.5f, 0.5f), 1f);
            squareSprite.name = "Default white square";
        }

        if (spriteMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            if (shader == null)
            {
                Debug.LogError("Could not find a sprite shader for the runner game.");
                return;
            }

            spriteMaterial = new Material(shader);
        }
    }

    private GameObject CreateBlock(string objectName, Vector2 position, Vector2 size, Color color, int sortingOrder)
    {
        GameObject block = new GameObject(objectName);
        block.transform.position = position;
        block.transform.localScale = new Vector3(size.x, size.y, 1f);

        SpriteRenderer renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = squareSprite;
        renderer.sharedMaterial = spriteMaterial;
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return block;
    }

    private void ResetRun()
    {
        score = 0f;
        worldSpeed = 6f;
        spawnTimer = 1.1f;
        jumpTimer = 0f;
        isJumping = false;
        isGameOver = false;
        player.gameObject.SetActive(true);
        player.position = new Vector3(PlayerX, GroundY + PlayerHeight * 0.5f, 0f);

        for (int i = 0; i < obstaclePool.Count; i++)
        {
            obstaclePool[i].Transform.gameObject.SetActive(false);
        }
    }

    private void UpdateJump(float deltaTime)
    {
        if (!isJumping)
        {
            return;
        }

        jumpTimer += deltaTime;
        float progress = Mathf.Clamp01(jumpTimer / JumpDuration);
        float height = 4f * JumpHeight * progress * (1f - progress);
        player.position = new Vector3(PlayerX, GroundY + PlayerHeight * 0.5f + height, 0f);

        if (progress >= 1f)
        {
            isJumping = false;
            player.position = new Vector3(PlayerX, GroundY + PlayerHeight * 0.5f, 0f);
        }
    }

    private void UpdateScrolling(float deltaTime)
    {
        for (int i = 0; i < backgroundItems.Count; i++)
        {
            ScrollItem item = backgroundItems[i];
            item.Transform.position += Vector3.left * (worldSpeed * item.SpeedMultiplier * deltaTime);
            if (item.Transform.position.x + item.Width * 0.5f < -cameraHalfWidth)
            {
                float rightmostX = backgroundItems[0].Transform.position.x;
                for (int tileIndex = 1; tileIndex < backgroundItems.Count; tileIndex++)
                {
                    rightmostX = Mathf.Max(rightmostX, backgroundItems[tileIndex].Transform.position.x);
                }

                item.Transform.position = new Vector3(rightmostX + item.Width, item.Transform.position.y,
                    item.Transform.position.z);
            }
        }
    }

    private void UpdateObstacles(float deltaTime)
    {
        spawnTimer -= deltaTime;
        if (spawnTimer <= 0f)
        {
            SpawnObstacle();
            spawnTimer = Random.Range(1.25f, 1.85f) * (6f / worldSpeed);
        }

        for (int i = 0; i < obstaclePool.Count; i++)
        {
            Obstacle obstacle = obstaclePool[i];
            if (!obstacle.Transform.gameObject.activeSelf)
            {
                continue;
            }

            obstacle.Transform.position += Vector3.left * (worldSpeed * deltaTime);
            if (obstacle.Transform.position.x < -cameraHalfWidth - 2f)
            {
                obstacle.Transform.gameObject.SetActive(false);
            }
        }
    }

    private void SpawnObstacle()
    {
        Obstacle obstacle = null;
        for (int i = 0; i < obstaclePool.Count; i++)
        {
            if (!obstaclePool[i].Transform.gameObject.activeSelf)
            {
                obstacle = obstaclePool[i];
                break;
            }
        }

        if (obstacle == null)
        {
            return;
        }

        float width = Random.Range(0.55f, 0.85f);
        float height = Random.Range(0.8f, 1.45f);
        obstacle.Width = width;
        obstacle.Height = height;
        obstacle.Transform.localScale = new Vector3(width, height, 1f);
        obstacle.Transform.position = new Vector3(cameraHalfWidth + 1.5f, GroundY + height * 0.5f, 0f);
        obstacle.Renderer.color = Random.value > 0.5f
            ? new Color(0.29f, 0.55f, 0.42f)
            : new Color(0.42f, 0.62f, 0.48f);
        obstacle.Transform.gameObject.SetActive(true);
    }

    private void CheckCollision()
    {
        float playerBottom = player.position.y - PlayerHeight * 0.5f;
        float playerTop = player.position.y + PlayerHeight * 0.5f;

        for (int i = 0; i < obstaclePool.Count; i++)
        {
            Obstacle obstacle = obstaclePool[i];
            if (!obstacle.Transform.gameObject.activeSelf)
            {
                continue;
            }

            bool overlapsX = Mathf.Abs(obstacle.Transform.position.x - PlayerX)
                             < (obstacle.Width + PlayerWidth) * 0.5f;
            float obstacleBottom = obstacle.Transform.position.y - obstacle.Height * 0.5f;
            float obstacleTop = obstacle.Transform.position.y + obstacle.Height * 0.5f;
            bool overlapsY = playerBottom < obstacleTop && playerTop > obstacleBottom;

            if (overlapsX && overlapsY)
            {
                isGameOver = true;
                player.gameObject.SetActive(false);
                return;
            }
        }
    }

    private bool WasJumpPressed()
    {
        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;
        Touchscreen touchscreen = Touchscreen.current;
        return (keyboard != null && (keyboard.spaceKey.wasPressedThisFrame || keyboard.upArrowKey.wasPressedThisFrame))
               || (mouse != null && mouse.leftButton.wasPressedThisFrame)
               || (touchscreen != null && touchscreen.primaryTouch.press.wasPressedThisFrame);
    }

    private void OnGUI()
    {
        GUIStyle label = new GUIStyle(GUI.skin.label);
        label.fontSize = Mathf.Max(18, Screen.height / 32);
        label.normal.textColor = new Color(0.13f, 0.2f, 0.24f);
        GUI.Label(new Rect(24f, 18f, 300f, 42f), "SCORE  " + Mathf.FloorToInt(score).ToString("00000"), label);

        GUIStyle hint = new GUIStyle(label);
        hint.fontSize = Mathf.Max(14, Screen.height / 48);
        hint.normal.textColor = new Color(0.22f, 0.32f, 0.34f);
        GUI.Label(new Rect(24f, 62f, 420f, 36f), "SPACE / UP / CLICK  -  JUMP", hint);

        if (!isGameOver)
        {
            return;
        }

        GUIStyle message = new GUIStyle(GUI.skin.label);
        message.alignment = TextAnchor.MiddleCenter;
        message.fontSize = Mathf.Max(28, Screen.height / 16);
        message.fontStyle = FontStyle.Bold;
        message.normal.textColor = new Color(0.13f, 0.2f, 0.24f);
        GUI.Label(new Rect(0f, Screen.height * 0.38f, Screen.width, 60f), "GAME OVER", message);

        message.fontSize = Mathf.Max(16, Screen.height / 38);
        message.fontStyle = FontStyle.Normal;
        GUI.Label(new Rect(0f, Screen.height * 0.49f, Screen.width, 40f), "PRESS SPACE OR CLICK TO RUN AGAIN", message);
    }

    private sealed class ScrollItem
    {
        public readonly Transform Transform;
        public readonly float Width;
        public readonly float SpeedMultiplier;

        public ScrollItem(Transform transform, float width, float speedMultiplier)
        {
            Transform = transform;
            Width = width;
            SpeedMultiplier = speedMultiplier;
        }
    }

    private sealed class Obstacle
    {
        public readonly Transform Transform;
        public readonly SpriteRenderer Renderer;
        public float Width;
        public float Height;

        public Obstacle(Transform transform, SpriteRenderer renderer)
        {
            Transform = transform;
            Renderer = renderer;
        }
    }
}
