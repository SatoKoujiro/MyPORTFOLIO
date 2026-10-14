using UnityEngine;

public static class PlatformerBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateGameManager()
    {
        if (Object.FindFirstObjectByType<PlatformerGameManager>() != null)
            return;

        new GameObject("Platformer Game").AddComponent<PlatformerGameManager>();
    }
}
