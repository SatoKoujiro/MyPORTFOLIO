using UnityEngine;

public sealed class PlatformerHud : MonoBehaviour
{
    private PlatformerGameManager gameManager;
    private GUIStyle hudStyle;
    private GUIStyle smallStyle;
    private GUIStyle titleStyle;
    private GUIStyle hintStyle;

    public void Initialize(PlatformerGameManager manager)
    {
        gameManager = manager;
    }

    private void OnGUI()
    {
        if (gameManager == null)
            return;

        EnsureStyles();
        Color previous = GUI.color;
        GUI.color = new Color(0.08f, 0.13f, 0.22f, 0.76f);
        GUI.Box(new Rect(18f, 18f, 300f, 82f), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(34f, 25f, 270f, 36f),
            "COINS  " + gameManager.CollectedCoins + " / " + gameManager.CoinCount, hudStyle);
        GUI.Label(new Rect(34f, 61f, 260f, 28f), "FALLS  " + gameManager.Deaths, smallStyle);

        GUI.color = new Color(0.08f, 0.13f, 0.22f, 0.68f);
        GUI.Box(new Rect(Screen.width - 360f, 18f, 342f, 82f), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width - 344f, 25f, 320f, 34f), "ARROWS / A D   MOVE", smallStyle);
        GUI.Label(new Rect(Screen.width - 344f, 59f, 320f, 30f),
            "SPACE / W / UP   JUMP     R   RESTART", smallStyle);

        if (gameManager.IsFinished)
            DrawCompletionPanel();

        GUI.color = previous;
    }

    private void EnsureStyles()
    {
        if (hudStyle != null)
            return;

        hudStyle = new GUIStyle(GUI.skin.label)
        {
            fontSize = Mathf.Max(18, Screen.height / 34),
            fontStyle = FontStyle.Bold,
            normal = { textColor = Color.white }
        };
        smallStyle = new GUIStyle(hudStyle)
        {
            fontSize = Mathf.Max(13, Screen.height / 55),
            fontStyle = FontStyle.Normal
        };
        titleStyle = new GUIStyle(hudStyle)
        {
            fontSize = Mathf.Max(34, Screen.height / 12),
            alignment = TextAnchor.MiddleCenter
        };
        hintStyle = new GUIStyle(smallStyle)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Max(16, Screen.height / 42)
        };
    }

    private void DrawCompletionPanel()
    {
        GUI.color = new Color(0.04f, 0.09f, 0.17f, 0.83f);
        GUI.Box(new Rect(Screen.width * 0.2f, Screen.height * 0.28f,
            Screen.width * 0.6f, Screen.height * 0.42f), GUIContent.none);
        GUI.color = Color.white;
        GUI.Label(new Rect(Screen.width * 0.22f, Screen.height * 0.32f,
            Screen.width * 0.56f, Screen.height * 0.2f), "LEVEL CLEAR!", titleStyle);
        GUI.Label(new Rect(Screen.width * 0.24f, Screen.height * 0.54f,
            Screen.width * 0.52f, Screen.height * 0.09f), "You found every coin. Nice run!", hintStyle);
        GUI.Label(new Rect(Screen.width * 0.24f, Screen.height * 0.63f,
            Screen.width * 0.52f, Screen.height * 0.07f), "Press R to play again", hintStyle);
    }
}
