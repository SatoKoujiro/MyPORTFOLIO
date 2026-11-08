
using UnityEngine;
using TMPro;
using System.Collections;

public class NextIndicator : MonoBehaviour
{
    [SerializeField] private TMP_Text indicatorText;
    [SerializeField] private float blinkSpeed = 0.6f;

    private Coroutine blinkCoroutine;

    public void Show()
    {
        gameObject.SetActive(true);

        if (blinkCoroutine != null)
            StopCoroutine(blinkCoroutine);

        blinkCoroutine = StartCoroutine(Blink());
    }

    public void Hide()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }

        gameObject.SetActive(false);
    }

    private IEnumerator Blink()
    {
        while (true)
        {
            float alpha = Mathf.PingPong(
                Time.unscaledTime / blinkSpeed, 1f
            );

            Color color = indicatorText.color;
            color.a = alpha;

            indicatorText.color = color;

            yield return null;
        }
    }

    private void OnDisable()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }
}
