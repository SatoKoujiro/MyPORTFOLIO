using UnityEngine;
using UnityEngine.InputSystem;

public sealed class PlatformerGameManager : MonoBehaviour
{
    private const float GoalX = 59f;
    private const float FallLimit = -8f;

    private PlatformerLevelData level;
    private PlayerMove player;
    private int collectedCoins;
    private int deaths;
    private bool finished;

    public int CollectedCoins => collectedCoins;
    public int CoinCount => level == null ? 0 : level.Coins.Count;
    public int Deaths => deaths;
    public bool IsFinished => finished;

    private void Start()
    {
        level = PlatformerLevelBuilder.Build();

        GameObject playerObject = new GameObject("Player");
        player = playerObject.AddComponent<PlayerMove>();
        player.Initialize(level.Platforms, level.SpawnPoint);

        Camera camera = Camera.main;
        if (camera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera");
            cameraObject.tag = "MainCamera";
            camera = cameraObject.AddComponent<Camera>();
            cameraObject.AddComponent<AudioListener>();
        }

        PlatformerCameraFollow cameraFollow = camera.GetComponent<PlatformerCameraFollow>();
        if (cameraFollow == null)
            cameraFollow = camera.gameObject.AddComponent<PlatformerCameraFollow>();
        cameraFollow.Initialize(camera, player.transform);
        gameObject.AddComponent<PlatformerHud>().Initialize(this);
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
            Restart();

        AnimateCoins();
    }

    private void LateUpdate()
    {
        if (level == null || finished)
            return;

        CollectCoins();
        if (HitHazard())
        {
            Respawn();
            return;
        }

        if (player.Position.y < FallLimit)
        {
            Respawn();
            return;
        }

        if (player.Position.x >= GoalX && collectedCoins == level.Coins.Count)
        {
            finished = true;
            player.enabled = false;
        }
    }

    private void Restart()
    {
        if (level == null)
            return;

        deaths = 0;
        collectedCoins = 0;
        finished = false;
        foreach (PlatformerCoin coin in level.Coins)
        {
            coin.Collected = false;
            coin.Object.SetActive(true);
        }

        player.enabled = true;
        player.Respawn();
    }

    private void CollectCoins()
    {
        Rect playerBounds = player.Bounds;
        foreach (PlatformerCoin coin in level.Coins)
        {
            if (coin.Collected || !playerBounds.Overlaps(coin.Bounds))
                continue;

            coin.Collected = true;
            coin.Object.SetActive(false);
            collectedCoins++;
        }
    }

    private bool HitHazard()
    {
        Rect playerBounds = player.Bounds;
        foreach (Rect hazard in level.Hazards)
        {
            if (playerBounds.Overlaps(hazard))
                return true;
        }

        return false;
    }

    private void Respawn()
    {
        deaths++;
        player.Respawn();
    }

    private void AnimateCoins()
    {
        if (level == null)
            return;

        for (int i = 0; i < level.Coins.Count; i++)
        {
            PlatformerCoin coin = level.Coins[i];
            if (coin.Collected)
                continue;

            float bob = Mathf.Sin(Time.time * 3f + i) * 0.09f;
            coin.Object.transform.position = new Vector3(coin.Position.x, coin.Position.y + bob, 0f);
        }

        level.GoalFlag.color = collectedCoins == level.Coins.Count
            ? new Color(0.42f, 0.94f, 0.64f)
            : new Color(1f, 0.42f, 0.37f);
    }
}
