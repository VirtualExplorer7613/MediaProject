using UnityEngine;

public class UltrasoundStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;

    public UltrasoundStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;
    public float CooldownSeconds => def.cooldownSeconds;

    public void OnSelected(InteractionContext ctx) { }
    public void OnDeselected(InteractionContext ctx) { }

    public void OnPress(InteractionContext ctx)
    {
        if (ctx.ultrasoundPrefab == null || ctx.shootPoint == null || ctx.cam == null) return;

        Ray ray = ctx.cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 dir = ray.direction.normalized;

        Object.Instantiate(ctx.ultrasoundPrefab, ctx.shootPoint.position, Quaternion.LookRotation(dir));
        // »ç¿îµå
    }

    public void OnHold(InteractionContext ctx, float deltaTime) { }
    public void OnRelease(InteractionContext ctx) { }

    public float GetProgress01() => -1f;
}
