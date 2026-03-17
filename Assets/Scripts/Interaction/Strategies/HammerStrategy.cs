using UnityEngine;

public class HammerStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;

    // 내부 연타 제한(너무 빠른 중복 입력 방지) - 필요하면 사용
    private float lastHitTime;
    private const float MinTapInterval = 0.12f;

    // 튜닝(원하면 ItemDefinition에 빼도 됨)
    private const float Range = 20.0f;      // 망치 유효 거리
    private const float Radius = 1f;     // 판정 반경(크면 쉬워짐)
    private const int DamagePerTap = 1;

    public HammerStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;

    // 연타해야 하니 0 추천 (InteractionController가 Release마다 쿨타임 걸기 때문)
    public float CooldownSeconds => 0f;

    public void OnSelected(InteractionContext ctx)
    {
        Debug.Log("[Hammer] OnSelected");
        ctx.hammerVisual?.SetVisible(true);
    }

    public void OnDeselected(InteractionContext ctx)
    {
        Debug.Log("[Hammer] OnDeselected");
        ctx.hammerVisual?.SetVisible(false);
    }

    public void OnPress(InteractionContext ctx)
    {
        Debug.Log("[Hammer] OnPress");
        if (ctx == null || ctx.cam == null) return;

        // (선택) 너무 빠른 입력 방지
        if (Time.time - lastHitTime < MinTapInterval)
        {
            Debug.Log("[Hammer] blocked by MinTapInterval");
            return;

        }
        lastHitTime = Time.time;

        // 1) 휘두름 연출
        ctx.hammerVisual?.PlaySwing();

        // 2) 히트 판정 (카메라 기준)
        TryHitTrash(ctx);
        ctx.itemUseBroadcaster?.RaiseItemUsed("hammer"); // 있으면 로그/퀘스트용
    }

    public void OnHold(InteractionContext ctx, float deltaTime)
    {
        // 망치는 "홀드"로 계속 때리는 것보다 "탭 연타"가 핵심이니 비워두는 게 안전
    }

    public void OnRelease(InteractionContext ctx)
    {
        // Release 때 할 일 없음 (쿨타임은 0으로)
    }

    public float GetProgress01() => -1f;

    private void TryHitTrash(InteractionContext ctx)
    {
        Transform camTr = ctx.cam.transform;

        Vector3 origin = camTr.position;
        Vector3 dir = camTr.forward;

        Debug.DrawLine(origin, origin + dir * Range, Color.red, 1.0f);

        // 레이어 마스크를 이미 ctx.trashLayerMask로 받고 있으니 그걸 사용
        // SphereCast로 근접 타격 느낌
        if (Physics.SphereCast(origin, Radius, dir, out RaycastHit hit, Range, ctx.trashLayerMask, QueryTriggerInteraction.Ignore))
        {
            // 특정 쓰레기만: HammerableTrash 있는 애만 반응
            if (hit.collider.TryGetComponent<HammerableTrash>(out var hammerable))
            {
                QuestTarget qt = hammerable.GetComponent<QuestTarget>();

                //퀘스트 쓰레기면서 Hammer로 부술 없는 경우
                if (qt != null && !qt.CanBeProcessedBy(QuestItemType.Hammer))
                {
                    return;
                }

                // 여기까지 오면 해머로 부서지는 일반 OR 퀘스트 쓰레기
                bool broke = hammerable.ApplyHit(DamagePerTap);

                if (broke && qt != null)
                {
                    qt.CompleteQuestTarget();
                }

                // 원하면 부서짐 사운드/이펙트 분기
                // if (broke) Managers.Sound.Play("TrashBreak");
            }
        }
    }
}