using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  - List&lt;VisualTextStep&gt; �� �޾� Ÿ���� + �Ͻ����� �˾��� ����.
///  - ���� �� Time.timeScale ����, �̵�/�þ� ��� ����.
/// </summary>
public class MonologuePopup : MonoBehaviour
{
    public static MonologuePopup Instance { get; private set; }

    [Header("UI")]
    [SerializeField] CanvasGroup panelGroup;       // ������ �г�(���� 0.6~0.8)
    [SerializeField] Image visualImage;      // ���û���
    [SerializeField] TextMeshProUGUI dialogueText;

    [Header("Typing")]
    [SerializeField] float typingSpeed = 0.08f;    // �ʴ� ���� �� ����

    /* ��Ÿ�� */
    List<VisualTextStep> steps;
    int cur;
    bool isTyping;
    float savedTimeScale = 1f;

    private CrosshairUI crosshairUI;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        panelGroup.alpha = 0f;
        panelGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
        if (crosshairUI == null)
            crosshairUI = FindObjectOfType<CrosshairUI>();
    }

    /* ---------- �ܺ� ȣ��� ---------- */
    public void Show(List<VisualTextStep> data)
    {
        if (data == null || data.Count == 0) return;

        steps = data;
        cur = 0;

        // ���� �Ͻ� ����
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // UI �ѱ�
        gameObject.SetActive(true);
        panelGroup.alpha = 1f;
        panelGroup.blocksRaycasts = true;

        // ũ�ν� ��� �����
        if (crosshairUI != null)
            crosshairUI.IsVisible = false;
        ShowCurrent();
    }

    /* ---------- Update ---------- */
    void Update()
    {
        if (steps == null) return;

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !isTyping)
            Next();
    }

    /* ---------- ���� ---------- */
    void ShowCurrent()
    {
        if (cur >= steps.Count) { Close(); return; }

        var step = steps[cur];

        if (step.image)
        {
            visualImage.sprite = step.image;
            visualImage.gameObject.SetActive(true);
        }
        else visualImage.gameObject.SetActive(false);

        StartCoroutine(TypeRoutine(step.text));
    }

    IEnumerator TypeRoutine(string msg)
    {
        isTyping = true;
        dialogueText.text = "";

        // Time.timeScale = 0 �� ���� Ÿ������ �귯������ WaitForSecondsRealtime ���
        foreach (char c in msg)
        {
            dialogueText.text += c;
            float delay = (c == ' ' ? typingSpeed * 0.3f : typingSpeed);
            yield return new WaitForSecondsRealtime(delay);
        }
        isTyping = false;
    }

    void Next()
    {
        cur++;
        ShowCurrent();
    }

    void Close()
    {
        // UI ����
        panelGroup.alpha = 0f;
        panelGroup.blocksRaycasts = false;
        gameObject.SetActive(false);

        // ���� �簳
        Time.timeScale = savedTimeScale;

        // ���� ����
        steps = null;
        dialogueText.text = "";

        if (crosshairUI != null)
            crosshairUI.IsVisible = true;
    }
}
