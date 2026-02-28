using UnityEngine;

public class MoveDistanceCondition : TutorialCondition
{
    [SerializeField] private float requiredDistance = 3f;

    private Vector3 startPos;
    private Transform player;

    public override void Activate(TutorialContext context)
    {
        base.Activate(context);
        player = context.player;

        if (player == null)
        {
            Debug.LogError("[MoveDistanceCondition] player is null");
            Deactivate();
            return;
        }

        startPos = player.position;
    }

    private void Update()
    {
        if (!isActive || isCompleted || player == null) return;

        float dist = Vector3.Distance(startPos, player.position);
        if (dist >= requiredDistance)
        {
            Complete();
        }
    }
}
