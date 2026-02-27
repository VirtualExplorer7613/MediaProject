using UnityEngine;

public class ItemUsedCondition : TutorialCondition
{
    [Tooltip("특정 아이템만 체크하려면 입력. 비우면 아무 아이템 사용이나 OK")]
    [SerializeField] private string requiredItemId = "";

    private ItemUseBroadcaster broadcaster;

    public override void Activate(TutorialContext context)
    {
        base.Activate(context);

        broadcaster = context.itemUseBroadcaster;
        if (broadcaster == null)
        {
            Debug.LogError("[ItemUsedCondition] ItemUseBroadcaster is null");
            Deactivate();
            return;
        }

        broadcaster.ItemUsed += OnItemUsed;
    }

    public override void Deactivate()
    {
        if (broadcaster != null)
            broadcaster.ItemUsed -= OnItemUsed;

        base.Deactivate();
    }

    private void OnItemUsed(string itemId)
    {
        if (!isActive || isCompleted) return;

        if (string.IsNullOrEmpty(requiredItemId) || requiredItemId == itemId)
        {
            Complete();
        }
    }
}
