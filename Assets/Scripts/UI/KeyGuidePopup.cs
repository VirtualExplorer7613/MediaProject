using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
///  - 게임 시작 직후(페이드-인 완료 후) 호출
///  - Time.timeScale = 0 으로 ‘일시 정지’
///  - SPACE, 마우스 클릭, 또는 n초 경과 시 자동 닫힘
/// </summary>


public class KeyGuidePopup : MonoBehaviour
{
    public static KeyGuidePopup Instance { get; private set; }

    [Header("UI")]
    [SerializeField] CanvasGroup group;   // 알파 조절용
    [SerializeField] Image guideImg;  // 키 가이드 이미지

    [Header("Behaviour")]
    [SerializeField] float fadeSpeed = 3f;   // 알파/초
    [SerializeField] float autoCloseTime = 4f;   // 미입력 시 자동 닫힘까지 대기

    bool showing;

    public bool IsShowing => showing;
    private CrosshairUI crosshairUI;

    void Awake()
    {
        Debug.Log($"[KeyGuidePopup Awake] {GetInstanceID()}");
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);   // 중복 보호
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

        Time.timeScale = 0f;         // ★ 일시 정지
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
        while (t < autoCloseTime && !Input.GetKey(KeyCode.Space))   // SPACE, 클릭 등 아무 입력
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
        Time.timeScale = 1f;        // ★ 재개
        showing = false;

        if (crosshairUI != null)
            crosshairUI.IsVisible = true;

        gameObject.SetActive(false);
    }
}
