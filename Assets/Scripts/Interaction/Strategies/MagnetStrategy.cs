using System.Collections.Generic;
using UnityEngine;

public class MagnetStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;
    private readonly List<DraggedItem> activeDragged = new();
    // 쓰레기별 누적 타이머
    private readonly Dictionary<DraggedItem, float> collectTimers = new();

    private const float CollectSeconds = 2.5f;

    public MagnetStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;
    public float CooldownSeconds => def.cooldownSeconds;

    public void OnSelected(InteractionContext ctx) { }
    public void OnDeselected(InteractionContext ctx)
    {
        ctx?.SetMagnetVisual(false);
        StopAll();
    }

    public void OnPress(InteractionContext ctx)
    {
        // 누르면 자석이 보이기 시작
        ctx?.SetMagnetVisual(true);
        // Press 시 즉시 한 번 탐색해서 끌기 시작 (체감 즉시성)
        AcquireTargets(ctx);
    }

    public void OnHold(InteractionContext ctx, float deltaTime)
    {
        // Hold 중에는 주기적으로 대상 갱신(새 쓰레기 추가)
        AcquireTargets(ctx);

        if (ctx.player == null) return;

        for (int i = activeDragged.Count - 1; i >= 0; i--)
        {
            var di = activeDragged[i];

            // 이미 파괴되었거나 누락된 부분 정리
            if (di == null)
            {
                activeDragged.RemoveAt(i);
                continue;
            }

            // 타이머 초기화(처음 잡힌 경우)
            if (!collectTimers.ContainsKey(di))
                collectTimers[di] = 0f;

            float dist = Vector3.Distance(di.transform.position, ctx.player.position);
            bool closeEnough = dist <= di.MagnetCollectDistance;

            if (closeEnough)
            {
                collectTimers[di] += deltaTime;

                if (collectTimers[di] >= CollectSeconds)
                {
                    collectTimers.Remove(di);
                    activeDragged.RemoveAt(i);

                    di.StopDragging();
                    di.CollectByMagnet();
                    ctx.itemUseBroadcaster?.RaiseItemUsed("magnet");
                }
            }
            else
            {
                // 거리를 벗어나면 리셋
                collectTimers[di] = 0f;
            }

        }

        // DraggedItem 자체가 target(고래)을 따라오도록 Update에서 처리됨
        // 여기서 추가로 할 게 필요하면(속도 증가 등) DraggedItem 확장하면 됨
    }

    public void OnRelease(InteractionContext ctx)
    {
        ctx?.SetMagnetVisual(false);
        StopAll();
    }

    public float GetProgress01() => -1f;

    private void AcquireTargets(InteractionContext ctx)
    {
        Debug.Log($"[Magnet] player null? {ctx.player == null}, mask={ctx.trashLayerMask.value}");

        if (ctx.player == null) return;

        float radius = Mathf.Max(0.1f, def.magnetRadius);
        Collider[] hits = Physics.OverlapSphere(ctx.player.position, radius, ctx.trashLayerMask);

        Debug.Log($"[Magnet] hits={hits.Length}, radius={radius}");

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

            collectTimers[di] = 0f;
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
        collectTimers.Clear();
    }
}
