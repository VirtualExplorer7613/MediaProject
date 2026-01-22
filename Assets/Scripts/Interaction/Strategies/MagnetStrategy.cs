using System.Collections.Generic;
using UnityEngine;

public class MagnetStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;
    private readonly List<DraggedItem> activeDragged = new();

    public MagnetStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;
    public float CooldownSeconds => def.cooldownSeconds;

    public void OnSelected(InteractionContext ctx) { }
    public void OnDeselected(InteractionContext ctx)
    {
        StopAll();
    }

    public void OnPress(InteractionContext ctx)
    {
        // Press 시 즉시 한 번 탐색해서 끌기 시작 (체감 즉시성)
        AcquireTargets(ctx);
    }

    public void OnHold(InteractionContext ctx, float deltaTime)
    {
        // Hold 중에는 주기적으로 대상 갱신(새 쓰레기 추가)
        AcquireTargets(ctx);

        // DraggedItem 자체가 target(고래)을 따라오도록 Update에서 처리됨
        // 여기서 추가로 할 게 필요하면(속도 증가 등) DraggedItem 확장하면 됨
    }

    public void OnRelease(InteractionContext ctx)
    {
        StopAll();
    }

    public float GetProgress01() => -1f;

    private void AcquireTargets(InteractionContext ctx)
    {
        if (ctx.player == null) return;

        float radius = Mathf.Max(0.1f, def.magnetRadius);
        Collider[] hits = Physics.OverlapSphere(ctx.player.position, radius, ctx.trashLayerMask);

        // 이미 충분히 끌고 있으면 종료
        if (activeDragged.Count >= def.magnetMaxTargets) return;

        for (int i = 0; i < hits.Length; i++)
        {
            if (activeDragged.Count >= def.magnetMaxTargets) break;

            DraggedItem di = hits[i].GetComponent<DraggedItem>();
            if (di == null) continue;

            // 이미 다른 이유로 드래그 중이면 스킵
            if (di.isDragged) continue;

            di.StartDragging(ctx.player);
            activeDragged.Add(di);
        }
    }

    private void StopAll()
    {
        for (int i = 0; i < activeDragged.Count; i++)
        {
            if (activeDragged[i] != null)
                activeDragged[i].StopDragging();
        }
        activeDragged.Clear();
    }
}
