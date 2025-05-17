using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;
    private Canvas fadeCanvas;

    private void Awake()
    {
        fadeCanvas = GetComponent<Canvas>();
        if (fadeCanvas != null)
            fadeCanvas.sortingOrder = 0; // 평상시엔 낮게 유지
    }

    public IEnumerator FadeOut(float duration)
    {
        if (fadeCanvas != null)
            fadeCanvas.sortingOrder = 999;
        float time = 0f;
        Color c = fadeImage.color;
        while (time < duration)
        {
            c.a = Mathf.Lerp(0, 1, time / duration);
            fadeImage.color = c;
            time += Time.deltaTime;
            yield return null;
        }
        c.a = 1;
        fadeImage.color = c;
    }

    public IEnumerator FadeIn(float duration)
    {
        float time = 0f;
        Color c = fadeImage.color;
        while (time < duration)
        {
            c.a = Mathf.Lerp(1, 0, time / duration);
            fadeImage.color = c;
            time += Time.deltaTime;
            yield return null;
        }
        c.a = 0;
        fadeImage.color = c;

        if (fadeCanvas != null)
            fadeCanvas.sortingOrder = 0;
    }
}
