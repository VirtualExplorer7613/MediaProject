using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Floating : MonoBehaviour
{
    [SerializeField] float amplitude = 12f;   // 이동 픽셀(±)
    [SerializeField] float frequency = 1.6f;  // 초당 왕복 횟수

    RectTransform rect;
    Vector2 originalPos;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
    }

    void Update()
    {
        // unscaledTime: 타임스케일 0에서도 움직임 유지
        float offsetY = amplitude * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2f * frequency);
        rect.anchoredPosition = originalPos + Vector2.up * offsetY;
    }

    /// <summary> 외부에서 On/Off 제어할 때 사용 </summary>
    public void SetVisible(bool visible) => gameObject.SetActive(visible);
}
