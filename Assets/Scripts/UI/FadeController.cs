using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeController : MonoBehaviour
{
    public Image fadeImage;

    public IEnumerator FadeOut(float duration)
    {
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
    }
}
