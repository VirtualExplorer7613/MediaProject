using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  - List&lt;VisualTextStep&gt; 을 받아 타이핑 + 일시정지 팝업을 띄운다.
///  - 종료 시 Time.timeScale 복원, 이동/시야 잠금 해제.
/// </summary>
public class MonologuePopup : MonoBehaviour
{
    public static MonologuePopup Instance { get; private set; }

    [Header("UI")]
    [SerializeField] CanvasGroup panelGroup;       // 반투명 패널(알파 0.6~0.8)
    [SerializeField] Image visualImage;      // 선택사항
    [SerializeField] TextMeshProUGUI dialogueText;

    [Header("Typing")]
    [SerializeField] float typingSpeed = 0.08f;    // 초당 글자 수 조절

    /* 런타임 */
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

    /* ---------- 외부 호출용 ---------- */
    public void Show(List<VisualTextStep> data)
    {
        if (data == null || data.Count == 0) return;

        steps = data;
        cur = 0;

        // 게임 일시 정지
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;

        // UI 켜기
        gameObject.SetActive(true);
        panelGroup.alpha = 1f;
        panelGroup.blocksRaycasts = true;

        // 크로스 헤어 숨기기
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

    /* ---------- 내부 ---------- */
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

        // Time.timeScale = 0 일 때도 타이핑이 흘러가도록 WaitForSecondsRealtime 사용
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
        // UI 끄기
        panelGroup.alpha = 0f;
        panelGroup.blocksRaycasts = false;
        gameObject.SetActive(false);

        // 게임 재개
        Time.timeScale = savedTimeScale;

        // 상태 리셋
        steps = null;
        dialogueText.text = "";

        if (crosshairUI != null)
            crosshairUI.IsVisible = true;
    }
}
