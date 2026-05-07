//using Unity.VisualScripting;
using UnityEngine;
//using UnityEngine.EventSystems;

public class PullTrashStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;

    private PullableTrash currentTarget;

   /* private int activeFingerId = -1;
    private bool isDragging = false;*/

    private const float AcquireRadius = 0.9f;

    // Visual 위치 보정
    [Header("Visual Tuning")]
    /*private const float VisualForwardOffset = 1.2f;
    private const float VisualTargetOffset = 0.15f;*/
    private const float PullVisualBackOffset = 0.28f;
    private const float PullVisualSideOffset = 0.0f;
    private const float PullVisualUpOffset = 0.1f;

    public PullTrashStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;
    public float CooldownSeconds => def.cooldownSeconds;

    public void OnSelected(InteractionContext ctx)
    {
        ClearCurrent(resetProgress: false);
        ctx?.SetPullVisual(false);
    }

    public void OnDeselected(InteractionContext ctx)
    {
        ClearCurrent(resetProgress: true);
        ctx?.SetPullVisual(false);
    }

    public void OnPress(InteractionContext ctx)
    {

        if (ctx == null || ctx.cam == null || ctx.player == null)
            return;

        // 버튼을 누르는 순간 카메라 중앙 기준으로 대상 탐색
        if (TryAcquireTarget(ctx))
        {
            currentTarget.BeginPull();
            ctx.SetPullVisual(true);
            UpdatePullVisual(ctx);
        }
        else
        {
            ctx.SetPullVisual(false);
        }
    }

    public void OnHold(InteractionContext ctx, float deltaTime)
    {
        Debug.Log("[Pull] OnHold");
        if (ctx == null || ctx.cam == null || ctx.player == null)
            return;

        /*#if UNITY_EDITOR || UNITY_STANDALONE
                HandleMouseInput(ctx);
        #else*/
        // HandleTouchInput(ctx);
        /*#endif
        */
        // Press 시점에 대상 탐색 실패했더라도,
        // Hold 중 조준점이 맞으면 다시 잡을 수 있게 함
        if (currentTarget == null)
        {
            if (!TryAcquireTarget(ctx))
            {
                ctx.SetPullVisual(false);
                return;
            }

            currentTarget.BeginPull();
            ctx.SetPullVisual(true);
        }

        UpdatePullVisual(ctx);

        // 진행도에 따른 저항감.
        // 초반에는 느리고 후반에는 조금 더 잘 뽑히는 느낌.
        /*float progress = currentTarget.Progress01;
        float resistanceMultiplier = Mathf.Lerp(
            MinResistanceMultiplier,
            MaxResistanceMultiplier,
            progress
        );*/

        // 기존 PullableTrash.ApplyDrag를 재사용하기 위해
        // "가상의 드래그 delta"를 만들어서 넣는다.
        // 즉, 손가락 드래그 대신 Hold 시간이 drag 입력처럼 작동한다.
       /* Vector2 autoDragDelta = Vector2.up * PullPixelsPerSecond * resistanceMultiplier * deltaTime;*/

        bool completed = currentTarget.ApplyHoldPull(deltaTime);

        if (completed)
        {
            CompleteCurrentTarget(ctx);
        }
    }

    public void OnRelease(InteractionContext ctx)
    {
        if (currentTarget != null)
            currentTarget.EndPull();

        currentTarget = null;
        ctx?.SetPullVisual(false);

        /*ClearCurrent(resetProgress: true);
        ctx?.SetPullVisual(false);*/
        // 기존 방식
        /*activeFingerId = -1;
        isDragging = false;*/
    }

    public float GetProgress01()
    {
        return currentTarget != null ? currentTarget.Progress01 : 0f;
    }

    /* private void HandleTouchInput(InteractionContext ctx)
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
                             // 퀘스트 쓰레기인지 체크
                             QuestTarget qt = currentTarget.GetComponent<QuestTarget>();
                             if (qt != null)
                             {
                                 qt.CompleteQuestTarget();
                             }

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
                 QuestTarget qt = currentTarget.GetComponent<QuestTarget>();
                 if (qt != null)
                 {
                     qt.CompleteQuestTarget();
                 }
                 ClearCurrent(resetProgress: false);
                 isDragging = false;
             }
         }

         if (Input.GetMouseButtonUp(0))
         {
             ClearCurrent(resetProgress: true);
             isDragging = false;
         }
     }*/

    /*private bool TryAcquireTarget(InteractionContext ctx, Vector2 screenPos)
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

            QuestTarget qt = target.GetComponent<QuestTarget>();
            if (qt != null)
            {
                if (!qt.CanBeProcessedBy(QuestItemType.Pull))
                    return false;
            }

            if (!target.CanStartPull(ctx.player))
                return false;

            currentTarget = target;
            return true;
        }

        return false;
    }*/

    /*private bool IsTouchOverUI(int fingerId)
    {
        if (EventSystem.current == null)
            return false;

        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }*/

    private bool TryAcquireTarget(InteractionContext ctx)
    {
        Ray ray = ctx.cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawRay(ray.origin, ray.direction * def.pullMaxDistance, Color.green, 0.2f);

        if (!Physics.SphereCast(
                ray,
                AcquireRadius,
                out RaycastHit hit,
                def.pullMaxDistance,
                ctx.trashLayerMask,
                QueryTriggerInteraction.Collide))
        {
            return false;
        }

        PullableTrash target = hit.collider.GetComponentInParent<PullableTrash>();
        if (target == null)
            target = hit.collider.GetComponent<PullableTrash>();

        if (target == null)
            return false;

        QuestTarget qt = target.GetComponent<QuestTarget>();
        if (qt != null && !qt.CanBeProcessedBy(QuestItemType.Pull))
            return false;

        if (!target.CanStartPull(ctx.player))
            return false;

        currentTarget = target;
        return true;
    }

    private void CompleteCurrentTarget(InteractionContext ctx)
    {
        if (currentTarget == null)
            return;

        QuestTarget qt = currentTarget.GetComponent<QuestTarget>();
        if (qt != null)
        {
            qt.CompleteQuestTarget();
        }

        ctx.itemUseBroadcaster?.RaiseItemUsed("pull_used");

        ClearCurrent(resetProgress: false);
        ctx.SetPullVisual(false);
    }

    #region 기존 pull 
    /*private void UpdatePullVisual(InteractionContext ctx)
    {
        if (ctx.pullVisual == null || currentTarget == null || ctx.cam == null)
            return;

        Transform camTr = ctx.cam.transform;

        // 카메라 앞에 핀셋/빨대 visual 배치
        Vector3 basePos = camTr.position + camTr.forward * VisualForwardOffset;

        // 대상 방향을 바라보게 함
        Vector3 targetPos = currentTarget.transform.position;
        Vector3 dirToTarget = targetPos - basePos;

        if (dirToTarget.sqrMagnitude < 0.001f)
            dirToTarget = camTr.forward;

        ctx.pullVisual.transform.position = basePos + dirToTarget.normalized * VisualTargetOffset;
        ctx.pullVisual.transform.rotation = Quaternion.LookRotation(dirToTarget.normalized, Vector3.up);
    }*/
    #endregion

    private void UpdatePullVisual(InteractionContext ctx)
    {
        if (ctx.pullVisual == null || currentTarget == null)
            return;

        Vector3 pullDir = currentTarget.PullDirection.normalized;

        // "쓰레기 아이템 앞" = 빠지는 방향 쪽 앞.
        // 쓰레기가 뽑혀 나올 방향에 핀셋을 둔다.
        Vector3 basePos =
            currentTarget.transform.position +
            pullDir * PullVisualBackOffset;

        // 필요하면 카메라 기준 좌우/상하 오프셋도 줄 수 있음
        if (ctx.cam != null)
        {
            basePos += ctx.cam.transform.right * PullVisualSideOffset;
            basePos += ctx.cam.transform.up * PullVisualUpOffset;
        }

        ctx.pullVisual.transform.position = basePos;

        // 핀셋의 앞방향이 쓰레기를 향하도록 배치
        // 모델 축이 반대로 되어 있으면 아래 LookRotation 방향을 -pullDir로 바꾸면 됨.
        ctx.pullVisual.transform.rotation = Quaternion.LookRotation(pullDir, Vector3.up);
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