using System;
using UnityEngine;

public class KeyStrategy : IInteractionStrategy
{
    private readonly ItemDefinition def;

    public KeyStrategy(ItemDefinition def) => this.def = def;

    public Sprite Icon => def.icon;
    public float CooldownSeconds => def.cooldownSeconds;

    public void OnSelected(InteractionContext ctx) { }
    public void OnDeselected(InteractionContext ctx) { }
    public void OnHold(InteractionContext ctx, float deltaTime) { }
    public void OnRelease(InteractionContext ctx) { }
    public float GetProgress01() => -1f;

    public void OnPress(InteractionContext ctx)
    {
        Debug.Log("Before chest. Key pressed!");
        var chest = ctx.currentChest;
        if (chest == null) return;

        Debug.Log("Chest is not Null! Key pressed!");


        if (chest.State == TreasureChestController.ChestState.Closed)
        {
            chest.TryUseKey(ctx.player);
            return;
        }

        if (chest.State == TreasureChestController.ChestState.Opened)
        {
            chest.TryCollect(ctx.player);
            return;
        }
    }

}
