using UnityEngine;
using UnityEngine.UI;

public class ActorHealthBar : MonoBehaviour
{
    private const float BarWidth = 48f;
    private const float BarHeight = 8f;
    private const float WorldOffset = 0.55f;

    private UnitActor unit;
    private EnemyActor enemy;
    private Image fillImage;

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
        CreateHealthBar();
    }

    private void LateUpdate()
    {
        int currentHealth = unit != null ? unit.CurrentHealth : enemy != null ? enemy.CurrentHealth : 0;
        int maxHealth = unit != null ? unit.MaxHealth : enemy != null ? enemy.MaxHealth : 0;
        if (fillImage == null || maxHealth <= 0)
        {
            return;
        }

        float healthRatio = Mathf.Clamp01((float)currentHealth / maxHealth);
        fillImage.fillAmount = healthRatio;
        fillImage.color = enemy != null
            ? Color.Lerp(new Color(0.55f, 0.12f, 0.1f), new Color(0.96f, 0.38f, 0.22f), healthRatio)
            : Color.Lerp(new Color(0.72f, 0.16f, 0.18f), new Color(0.24f, 0.84f, 0.48f), healthRatio);
    }

    private void CreateHealthBar()
    {
        GameObject canvasObject = new GameObject("Health Bar Canvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = Vector3.up * WorldOffset;
        canvasObject.transform.localScale = Vector3.one * 0.01f;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.overrideSorting = true;
        canvas.sortingOrder = 100;
        canvas.worldCamera = Camera.main;

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(BarWidth, BarHeight);
        canvasRect.localRotation = Quaternion.identity;

        Image background = CreateImage(canvasRect, "Background");
        background.color = new Color(0.04f, 0.05f, 0.06f, 0.9f);

        fillImage = CreateImage(canvasRect, "Health");
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;
        RectTransform fillRect = fillImage.rectTransform;
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = new Vector2(1f, 1f);
        fillRect.offsetMax = new Vector2(-1f, -1f);
    }

    private static Image CreateImage(RectTransform parent, string name)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        RectTransform rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return imageObject.GetComponent<Image>();
    }
}
