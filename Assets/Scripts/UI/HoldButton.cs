using UnityEngine;
using UnityEngine.EventSystems;

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

    public void OnPointerDown(PointerEventData eventData) => IsPressed = true;
    public void OnPointerUp(PointerEventData eventData) => IsPressed = false;

    // 손가락이 버튼 밖으로 나가면 false
    public void OnPointerExit(PointerEventData eventData) => IsPressed = false;
}
