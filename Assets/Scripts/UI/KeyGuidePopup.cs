using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  - ���� ���� ����(���̵�-�� �Ϸ� ��) ȣ��
///  - Time.timeScale = 0 ���� ���Ͻ� ������
///  - SPACE, ���콺 Ŭ��, �Ǵ� n�� ��� �� �ڵ� ����
/// </summary>


public class KeyGuidePopup : MonoBehaviour
{
    public static KeyGuidePopup Instance { get; private set; }

    [Header("UI")]
    [SerializeField] CanvasGroup group;   // ���� ������
    [SerializeField] Image guideImg;  // Ű ���̵� �̹���

    [Header("Behaviour")]
    [SerializeField] float fadeSpeed = 3f;   // ����/��
    [SerializeField] float autoCloseTime = 4f;   // ���Է� �� �ڵ� �������� ���

    bool showing;

    public bool IsShowing => showing;
    private CrosshairUI crosshairUI;

    void Awake()
    {
        Debug.Log($"[KeyGuidePopup Awake] {GetInstanceID()}");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);   // �ߺ� ��ȣ
            return;
        }
        Instance = this;

        if (crosshairUI == null)
            crosshairUI = FindObjectOfType<CrosshairUI>();

    }

    public void Show()
    {
        Debug.Log($"[KeyGuidePopup Show] {Time.frameCount}");
        if (showing) return;
        showing = true;

        Time.timeScale = 0f;         // �� �Ͻ� ����
        group.alpha = 0;
        guideImg.enabled = true;
        gameObject.SetActive(true);

        if (crosshairUI != null)
            crosshairUI.IsVisible = false;

        StartCoroutine(FadeIn());
    }

    IEnumerator FadeIn()
    {
        while (group.alpha < 1f)
        {
            group.alpha += Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }

        float t = 0f;
        while (t < autoCloseTime && !Input.GetKey(KeyCode.Space))   // SPACE, Ŭ�� �� �ƹ� �Է�
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        while (group.alpha > 0f)
        {
            group.alpha -= Time.unscaledDeltaTime * fadeSpeed;
            yield return null;
        }
        guideImg.enabled = false;
        Time.timeScale = 1f;        // �� �簳
        showing = false;

        if (crosshairUI != null)
            crosshairUI.IsVisible = true;

        gameObject.SetActive(false);
    }
}
