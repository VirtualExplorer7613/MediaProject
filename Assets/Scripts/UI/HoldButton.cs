using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public enum HoldDirection
{
    Down = -1,
    Up = 1
}

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [Tooltip("+1 = Up, -1 = Down")]
    public HoldDirection direction;

    public bool IsPressed { get; private set; }

    [Header("Events")]
    public UnityEvent onPressed;
    public UnityEvent onReleased;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (IsPressed) return;          // 중복 방지 (멀티터치/중복 호출 대비)
        IsPressed = true;
        onPressed?.Invoke();
        
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        if (!IsPressed) return;         // 중복 방지
        IsPressed = false;
        onReleased?.Invoke();
    }

    // 손가락이 버튼 밖으로 나가면 false
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!IsPressed) return;
        IsPressed = false;
        onReleased?.Invoke();
    }
}
