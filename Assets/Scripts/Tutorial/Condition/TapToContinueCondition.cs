using UnityEngine;
using UnityEngine.EventSystems;

public class TapToContinueCondition : TutorialCondition
{
    [Header("Input Options")]
    [Tooltip("모바일 터치(탭)로 진행")]
    [SerializeField] private bool allowTouch = true;

    [Tooltip("PC 마우스 클릭으로 진행")]
    [SerializeField] private bool allowMouseClick = true;

    [Tooltip("키보드(예: Space/Enter)로 진행")]
    [SerializeField] private bool allowKeyboard = true;

    [Tooltip("키보드 진행 키")]
    [SerializeField] private KeyCode[] keys = new[] { KeyCode.Space, KeyCode.Return };

    [Header("UI Ignore")]
    [Tooltip("UI 위를 탭/클릭한 입력은 무시할지 여부")]
    [SerializeField] private bool ignoreWhenPointerOverUI = false;

    public override void Activate(TutorialContext context)
    {
        base.Activate(context);
    }

    private void Update()
    {
        if (!isActive || isCompleted) return;

        if (ignoreWhenPointerOverUI && IsPointerOverUI())
            return;

        if (allowTouch && IsTouchTap())
        {
            Complete();
            return;
        }

        if (allowMouseClick && Input.GetMouseButtonDown(0))
        {
            Complete();
            return;
        }

        if (allowKeyboard && IsKeyPressed())
        {
            Complete();
            return;
        }
    }

    private bool IsTouchTap()
    {
        if (Input.touchCount <= 0) return false;

        // 첫 터치가 "Began"이면 탭으로 간주
        var t = Input.GetTouch(0);
        return t.phase == TouchPhase.Began;
    }

    private bool IsKeyPressed()
    {
        if (keys == null) return false;

        for (int i = 0; i < keys.Length; i++)
        {
            if (Input.GetKeyDown(keys[i]))
                return true;
        }
        return false;
    }

    private bool IsPointerOverUI()
    {
        // EventSystem이 없으면 UI 체크 불가 → false
        if (EventSystem.current == null) return false;

        // 마우스(PC)
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // 터치(모바일): fingerId로 체크
        if (Input.touchCount > 0)
        {
            int fingerId = Input.GetTouch(0).fingerId;
            if (EventSystem.current.IsPointerOverGameObject(fingerId))
                return true;
        }

        return false;
    }
}
