using UnityEngine;

public class UltrasoundStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;

    public UltrasoundStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;
    public float CooldownSeconds => def.cooldownSeconds;

    public void OnSelected(InteractionContext ctx) { }
    public void OnDeselected(InteractionContext ctx) { }

    private bool isFirstPressed = true;

    public void OnPress(InteractionContext ctx)
    {
        if (ctx.ultrasoundPrefab == null || ctx.shootPoint == null || ctx.cam == null) return;

        Ray ray = ctx.cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 dir = ray.direction.normalized;

        var go = Object.Instantiate(ctx.ultrasoundPrefab, ctx.shootPoint.position, Quaternion.LookRotation(dir));
        Managers.Sound.Play("Underwater_Impact_02");
        var ultrasound = go.GetComponent<Ultrasound>();

        if (ultrasound != null)
        {
            ultrasound.SetBroadcaster(ctx.itemUseBroadcaster);
        }


        // 파도(초음파) 사용 이벤트 발행
        //ctx.itemUseBroadcaster?.RaiseItemUsed("wave");

    }

    public void OnHold(InteractionContext ctx, float deltaTime) { }
    public void OnRelease(InteractionContext ctx) { }

    public float GetProgress01() => -1f;
}
