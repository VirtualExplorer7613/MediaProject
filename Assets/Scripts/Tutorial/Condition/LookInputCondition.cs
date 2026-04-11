using UnityEngine;
using UnityEngine.EventSystems;

public class LookInputCondition : TutorialCondition
{
    [SerializeField] private float requiredLookAmount = 300f;

    [Header("Touch Area")]
    [SerializeField] private bool onlyRightHalf = true;
    [SerializeField] private bool ignoreUITouch = true;

    private float accumulated;

    private int trackingFingerId = -1;
    private bool isTracking = false;

    public override void Activate(TutorialContext context)
    {
        base.Activate(context);
        accumulated = 0f;
        trackingFingerId = -1;
        isTracking = false;
    }

    private void Update()
    {
        if (!isActive || isCompleted) return;

        /*#if UNITY_EDITOR || UNITY_STANDALONE
                float mouseX = Mathf.Abs(Input.GetAxis("Mouse X"));
                float mouseY = Mathf.Abs(Input.GetAxis("Mouse Y"));
                accumulated += (mouseX + mouseY) * 100f * Time.unscaledDeltaTime;
        #endif*/

        HandleTouchLookInput();

       if (accumulated >= requiredLookAmount)
            Complete();
       /*
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Moved)
                accumulated += t.deltaPosition.magnitude;
        }

        if (accumulated >= requiredLookAmount)
            Complete();*/
    }

    private void HandleTouchLookInput()
    {
        if (Input.touchCount <= 0)
        {
            isTracking = false;
            trackingFingerId = -1;
            return;
        }

        // 아직 추적 중인 손가락이 없다면,
        // 오른쪽 절반에서 시작한 손가락만 추적 시작
        if (!isTracking)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase != TouchPhase.Began)
                    continue;

                if (onlyRightHalf && touch.position.x < Screen.width * 0.5f)
                    continue;

                if (ignoreUITouch &&
                    EventSystem.current != null &&
                    EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                    continue;

                trackingFingerId = touch.fingerId;
                isTracking = true;
                break;
            }
        }

        // 추적 중인 손가락이 있으면 그 손가락만 누적
        if (isTracking)
        {
            bool found = false;

            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.fingerId != trackingFingerId)
                    continue;

                found = true;

                if (touch.phase == TouchPhase.Moved)
                {
                    accumulated += touch.deltaPosition.magnitude;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    isTracking = false;
                    trackingFingerId = -1;
                }

                break;
            }

            // 추적하던 손가락이 현재 목록에 없으면 추적 해제
            if (!found)
            {
                isTracking = false;
                trackingFingerId = -1;
            }
        }
    }
}