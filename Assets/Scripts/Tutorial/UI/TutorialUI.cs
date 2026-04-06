using System.Collections;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI text;

    [SerializeField] private float fadeDuration = 0.15f;

    private Coroutine fadeCo;

    public void SetMessage(string message)
    {
        text.text = message;
    }

    public void SetVisible(bool visible, bool instant = false)
    {
        if (fadeCo != null) StopCoroutine(fadeCo);

        if (instant)
        {
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = visible;
            return;
        }

        fadeCo = StartCoroutine(FadeTo(visible ? 1f : 0f));
    }

    private IEnumerator FadeTo(float target)
    {
        float start = canvasGroup.alpha;
        float t = 0f;

        /*canvasGroup.blocksRaycasts = target > 0.5f;
        canvasGroup.interactable = target > 0.5f;*/

        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(start, target, t / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = target;
    }
}
