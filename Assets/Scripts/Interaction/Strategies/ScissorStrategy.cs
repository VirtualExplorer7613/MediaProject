using UnityEngine;

public class ScissorStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;

    private ScissorableTrash currentTarget;
    private float holdTimer;

    private const float SearchRange = 15.0f;
    private const float SearchRadius = 2.0f;
    private const float ForwardOffset = 0.7f;

    public ScissorStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;
    public float CooldownSeconds => 0f;

    public void OnSelected(InteractionContext ctx)
    {
        ctx.scissorVisual?.SetVisible(false);
        currentTarget = null;
        holdTimer = 0f;
    }

    public void OnDeselected(InteractionContext ctx)
    {
        ctx.scissorVisual?.StopCutLoop();
        ctx.scissorVisual?.SetVisible(false);
        currentTarget = null;
        holdTimer = 0f;
    }

    public void OnPress(InteractionContext ctx)
    {
        currentTarget = FindTarget(ctx);

        if (currentTarget == null)
        {
            ctx.scissorVisual?.StopCutLoop();
            ctx.scissorVisual?.SetVisible(false);
            holdTimer = 0f;
            return;
        }

        ctx.scissorVisual?.SetTarget(currentTarget.transform);
        ctx.scissorVisual?.SetVisible(true);
        ctx.scissorVisual?.StartCutLoop();
    }

    public void OnHold(InteractionContext ctx, float deltaTime)
    {
        if (currentTarget == null)
        {
            currentTarget = FindTarget(ctx);

            if (currentTarget == null)
            {
                ctx.scissorVisual?.StopCutLoop();
                ctx.scissorVisual?.SetVisible(false);
                holdTimer = 0f;
                return;
            }

            ctx.scissorVisual?.SetTarget(currentTarget.transform);
            ctx.scissorVisual?.SetVisible(true);
            ctx.scissorVisual?.StartCutLoop();
        }

        // 타겟이 여전히 유효한지 재확인
        if (!IsTargetStillValid(ctx, currentTarget))
        {
            ctx.scissorVisual?.StopCutLoop();
            ctx.scissorVisual?.SetVisible(false);
            currentTarget = null;
            holdTimer = 0f;
            return;
        }

        holdTimer += deltaTime;

        if (holdTimer >= currentTarget.RequiredHoldSeconds)
        {
            currentTarget.CutComplete();
            ctx.itemUseBroadcaster?.RaiseItemUsed("scissor");

            ctx.scissorVisual?.StopCutLoop();
            ctx.scissorVisual?.SetVisible(false);

            currentTarget = null;
            holdTimer = 0f;
        }
    }

    public void OnRelease(InteractionContext ctx)
    {
        ctx.scissorVisual?.StopCutLoop();
        ctx.scissorVisual?.SetVisible(false);
        currentTarget = null;
        holdTimer = 0f;
    }

    public float GetProgress01()
    {
        if (currentTarget == null || currentTarget.RequiredHoldSeconds <= 0f) return 0f;
        return Mathf.Clamp01(holdTimer / currentTarget.RequiredHoldSeconds);
    }

    private ScissorableTrash FindTarget(InteractionContext ctx)
    {
        if (ctx == null || ctx.cam == null) return null;

        Transform camTr = ctx.cam.transform;
        Vector3 origin = camTr.position + camTr.forward * ForwardOffset;
        Vector3 dir = camTr.forward;

        Debug.DrawLine(origin, origin + dir * SearchRange, Color.cyan, 0.1f);

        if (Physics.SphereCast(origin, SearchRadius, dir, out RaycastHit hit, SearchRange,
            ctx.trashLayerMask, QueryTriggerInteraction.Ignore))
        {
            return hit.collider.GetComponentInParent<ScissorableTrash>();
        }

        return null;
    }

    private bool IsTargetStillValid(InteractionContext ctx, ScissorableTrash target)
    {
        if (ctx == null || ctx.cam == null || target == null) return false;

        Vector3 camPos = ctx.cam.transform.position;
        Vector3 targetPos = target.transform.position;

        float dist = Vector3.Distance(camPos, targetPos);
        return dist <= SearchRange + 1.0f;
    }
}