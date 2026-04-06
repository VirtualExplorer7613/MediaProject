using UnityEngine;

public class DistanceToTargetCondition : TutorialCondition
{
    [SerializeField] private Transform target;
    [SerializeField] private float requiredDistance = 3f;

    private Transform player;

    public override void Activate(TutorialContext context)
    {
        base.Activate(context);
        player = context.player;
        target = context.orcaTarget;

        if (player == null || target == null)
        {
            Debug.LogError("[DistanceToTargetCondition] player or target is null");
            Deactivate();
        }
    }

    private void Update()
    {
        if (!isActive || isCompleted || player == null || target == null) return;

        float dist = Vector3.Distance(player.position, target.position);
        if (dist <= requiredDistance)
            Complete();
    }
}