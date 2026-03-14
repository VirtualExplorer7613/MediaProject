using UnityEngine;
using UnityEngine.EventSystems;

public class PullTrashStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;

    private PullableTrash currentTarget;
    private int activeFingerId = -1;
    private bool isDragging = false;
    private const float AcquireRadius = 0.35f;

    public PullTrashStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;
    public float CooldownSeconds => def.cooldownSeconds;

    public void OnSelected(InteractionContext ctx)
    {
        ClearCurrent(resetProgress: false);
    }

    public void OnDeselected(InteractionContext ctx)
    {
        ClearCurrent(resetProgress: true);
    }

    public void OnPress(InteractionContext ctx)
    {
        // 버튼을 눌렀다고 바로 대상 선택하지 않고,
        // OnHold 안에서 실제 터치 시작을 감지해서 선택
    }

    public void OnHold(InteractionContext ctx, float deltaTime)
    {
        Debug.Log("[Pull] OnHold");
        if (ctx == null || ctx.cam == null || ctx.player == null)
            return;

/*#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouseInput(ctx);
#else*/
        HandleTouchInput(ctx);
/*#endif
*/    }

    public void OnRelease(InteractionContext ctx)
    {
        ClearCurrent(resetProgress: true);
        activeFingerId = -1;
        isDragging = false;
    }

    public float GetProgress01()
    {
        return currentTarget != null ? currentTarget.Progress01 : 0f;
    }

    private void HandleTouchInput(InteractionContext ctx)
    {
        Debug.Log($"[Pull] touchCount = {Input.touchCount}");
        if (Input.touchCount <= 0)
            return;

        // 이미 잡은 손가락이 없으면, 새로 시작된 터치를 찾음
        if (activeFingerId == -1)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(i);

                if (touch.phase != TouchPhase.Began)
                    continue;

                if (IsTouchOverUI(touch.fingerId))
                {
                    Debug.Log("[Pull] Touch ignored because it is over UI");
                    continue;
                }
                   

                if (TryAcquireTarget(ctx, touch.position))
                {
                    activeFingerId = touch.fingerId;
                    isDragging = true;
                    currentTarget.BeginPull();
                    break;
                }
            }

            return;
        }

        // 잡은 finger 계속 추적
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.fingerId != activeFingerId)
                continue;

            switch (touch.phase)
            {
                case TouchPhase.Moved:
                case TouchPhase.Stationary:
                    {
                        if (currentTarget == null) return;

                        bool completed = currentTarget.ApplyDrag(touch.deltaPosition, ctx.cam);
                        if (completed)
                        {
                            ClearCurrent(resetProgress: false);
                            activeFingerId = -1;
                            isDragging = false;
                        }
                        return;
                    }

                case TouchPhase.Ended:
                case TouchPhase.Canceled:
                    {
                        ClearCurrent(resetProgress: true);
                        activeFingerId = -1;
                        isDragging = false;
                        return;
                    }
            }
        }
    }

    private void HandleMouseInput(InteractionContext ctx)
    {
        // 에디터 테스트용
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (TryAcquireTarget(ctx, Input.mousePosition))
            {
                isDragging = true;
                currentTarget.BeginPull();
            }
        }

        if (isDragging && currentTarget != null && Input.GetMouseButton(0))
        {
            Vector2 delta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")) * 30f;
            bool completed = currentTarget.ApplyDrag(delta, ctx.cam);

            if (completed)
            {
                ClearCurrent(resetProgress: false);
                isDragging = false;
            }
        }

        if (Input.GetMouseButtonUp(0))
        {
            ClearCurrent(resetProgress: true);
            isDragging = false;
        }
    }

    private bool TryAcquireTarget(InteractionContext ctx, Vector2 screenPos)
    {
        Debug.Log("[Pull] TryAcquireTarget called");
        Ray ray = ctx.cam.ScreenPointToRay(screenPos);

        Debug.DrawRay(ray.origin, ray.direction * def.pullMaxDistance, Color.green, 1.0f);

        if (Physics.SphereCast(ray, AcquireRadius, out RaycastHit hit, def.pullMaxDistance, ctx.trashLayerMask))
        {
            Debug.Log($"[Pull] SphereCast hit: {hit.collider.name}");

            PullableTrash target = hit.collider.GetComponentInParent<PullableTrash>();
            if (target == null)
                target = hit.collider.GetComponent<PullableTrash>();

            if (target == null)
                return false;

            if (!target.CanStartPull(ctx.player))
                return false;

            currentTarget = target;
            return true;
        }

        return false;
    }

    private bool IsTouchOverUI(int fingerId)
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    private void ClearCurrent(bool resetProgress)
    {
        if (currentTarget != null)
        {
            currentTarget.CancelPull(resetProgress);
            currentTarget = null;
        }
    }
}