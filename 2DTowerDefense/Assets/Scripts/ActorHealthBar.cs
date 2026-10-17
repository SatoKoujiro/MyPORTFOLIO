using UnityEngine;

public class ActorHealthBar : MonoBehaviour
{
    private const float BarWidth = 46f;
    private const float BarHeight = 6f;
    private const float WorldOffset = 0.55f;

    private UnitActor unit;
    private EnemyActor enemy;

    public static void Attach(GameObject actor)
    {
        if (actor.GetComponent<ActorHealthBar>() == null)
        {
            actor.AddComponent<ActorHealthBar>();
        }
    }

    private void Awake()
    {
        unit = GetComponent<UnitActor>();
        enemy = GetComponent<EnemyActor>();
    }

    private void OnGUI()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            return;
        }

        int currentHealth = unit != null ? unit.CurrentHealth : enemy != null ? enemy.CurrentHealth : 0;
        int maxHealth = unit != null ? unit.MaxHealth : enemy != null ? enemy.MaxHealth : 0;
        if (maxHealth <= 0)
        {
            return;
        }

        Vector3 screenPosition = camera.WorldToScreenPoint(transform.position + Vector3.up * WorldOffset);
        if (screenPosition.z <= 0f)
        {
            return;
        }

        float healthRatio = Mathf.Clamp01((float)currentHealth / maxHealth);
        float guiY = Screen.height - screenPosition.y;
        Rect background = new Rect(screenPosition.x - BarWidth * 0.5f - 1f, guiY - BarHeight * 0.5f - 1f, BarWidth + 2f, BarHeight + 2f);
        Rect bar = new Rect(screenPosition.x - BarWidth * 0.5f, guiY - BarHeight * 0.5f, BarWidth * healthRatio, BarHeight);

        GUI.color = new Color(0.04f, 0.05f, 0.06f, 0.9f);
        GUI.DrawTexture(background, Texture2D.whiteTexture);
        GUI.color = enemy != null
            ? Color.Lerp(new Color(0.55f, 0.12f, 0.1f), new Color(0.96f, 0.38f, 0.22f), healthRatio)
            : Color.Lerp(new Color(0.72f, 0.16f, 0.18f), new Color(0.24f, 0.84f, 0.48f), healthRatio);
        GUI.DrawTexture(bar, Texture2D.whiteTexture);
        GUI.color = Color.white;
    }
}