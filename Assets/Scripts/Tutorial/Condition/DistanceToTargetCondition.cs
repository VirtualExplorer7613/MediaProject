using UnityEngine;

public class DistanceToTargetCondition : TutorialCondition
{
    [Header("Target")]
    [SerializeField] private string targetId;

    [Header("Distance")]
    [SerializeField] private float requiredDistance = 3f;
    [SerializeField] private bool horizontalOnly = true;

    private Transform player;
    private Transform target;

    public override void Activate(TutorialContext context)
    {
        base.Activate(context);

        player = context.player;
        target = context.GetTarget(targetId);

        if (player == null || target == null)
        {
            Debug.LogError($"[DistanceToTargetCondition] player or target is null. targetId={targetId}");
            Deactivate();
        }
    }

    private void Update()
    {
        if (!isActive || isCompleted || player == null || target == null)
            return;

        float dist = horizontalOnly
            ? GetHorizontalDistance(player.position, target.position)
            : Vector3.Distance(player.position, target.position);

        if (dist <= requiredDistance)
            Complete();
    }

    private float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}