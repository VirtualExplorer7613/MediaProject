using UnityEngine;

public class SpongeStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;

    private Vector3 lastWorldPos;
    private bool hasLastWorldPos = false;

    private OilStain currentStain;

    private const float VisualYOffset = 0.05f;   // 표면과 살짝 띄우기
    private const float MinMoveThreshold = 0.005f; // 너무 미세한 떨림 무시

    public SpongeStrategy(ItemDefinition def)
    {
        this.def = def;
    }

    public Sprite Icon => def.icon;
    public float CooldownSeconds => 0f;

    public void OnSelected(InteractionContext ctx)
    {
        ctx?.SetSpongeVisual(false);
        hasLastWorldPos = false;
        currentStain = null;
    }

    public void OnDeselected(InteractionContext ctx)
    {
        ctx?.SetSpongeVisual(false);
        hasLastWorldPos = false;
        currentStain = null;
    }

    public void OnPress(InteractionContext ctx)
    {
        if (ctx == null || ctx.cam == null || ctx.spongeVisual == null)
            return;

        ctx.SetSpongeVisual(true);

        if (TryGetPointerWorldPoint(ctx, out Vector3 worldPos, out OilStain stain))
        {
            MoveSpongeVisual(ctx.spongeVisual, worldPos);
            lastWorldPos = worldPos;
            hasLastWorldPos = true;
            currentStain = stain;
        }
        else
        {
            hasLastWorldPos = false;
            currentStain = null;
        }
    }

    public void OnHold(InteractionContext ctx, float deltaTime)
    {
        if (ctx == null || ctx.cam == null || ctx.spongeVisual == null)
            return;

        if (!TryGetPointerWorldPoint(ctx, out Vector3 worldPos, out OilStain stain))
            return;

        MoveSpongeVisual(ctx.spongeVisual, worldPos);

        float movedDistance = 0f;
        if (hasLastWorldPos)
        {
            movedDistance = Vector3.Distance(lastWorldPos, worldPos);
        }

        if (movedDistance >= MinMoveThreshold && stain != null)
        {
            QuestTarget qt = stain.GetComponent<QuestTarget>();
            if (qt != null && !qt.CanBeProcessedBy(QuestItemType.Sponge))
            {
                lastWorldPos = worldPos;
                hasLastWorldPos = true;
                currentStain = stain;
                return;
            }

            stain.AddScrub(movedDistance, deltaTime);

            if (stain.IsCleaned && qt != null)
            {
                qt.CompleteQuestTarget();
            }
        }

        lastWorldPos = worldPos;
        hasLastWorldPos = true;
        currentStain = stain;
    }

    public void OnRelease(InteractionContext ctx)
    {
        ctx?.SetSpongeVisual(false);
        hasLastWorldPos = false;
        currentStain = null;
    }

    public float GetProgress01() => -1f;

    private bool TryGetPointerWorldPoint(
        InteractionContext ctx,
        out Vector3 worldPos,
        out OilStain stain)
    {
        worldPos = default;
        stain = null;

        Ray ray = ctx.cam.ScreenPointToRay(GetPointerScreenPosition());

        // 1순위: 기름때 레이어 맞추기
        if (Physics.Raycast(ray, out RaycastHit oilHit, 200f, ctx.oilStainMask))
        {
            worldPos = oilHit.point;
            stain = oilHit.collider.GetComponentInParent<OilStain>();
            return true;
        }

        // 2순위: 청소 가능한 표면
        if (Physics.Raycast(ray, out RaycastHit surfaceHit, 200f, ctx.spongeSurfaceMask))
        {
            worldPos = surfaceHit.point;
            stain = surfaceHit.collider.GetComponentInParent<OilStain>();
            return true;
        }

        return false;
    }

    private Vector3 GetPointerScreenPosition()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        return Input.mousePosition;
#else
        if (Input.touchCount > 0)
            return Input.GetTouch(0).position;

        return Input.mousePosition;
#endif
    }

    private void MoveSpongeVisual(GameObject spongeVisual, Vector3 worldPos)
    {
        spongeVisual.transform.position = worldPos + Vector3.up * VisualYOffset;
    }
}