using System.Collections.Generic;
using UnityEngine;

public class TutorialContext
{
    public Transform player;
    public ItemUseBroadcaster itemUseBroadcaster;

    public TutorialWorldVfxController worldVfxController;

    private Dictionary<string, Transform> targets = new();

    public void SetTargets(Dictionary<string, Transform> targets)
    {
        this.targets = targets;
    }

    public Transform GetTarget(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;

        if (targets != null && targets.TryGetValue(id, out Transform target))
            return target;

        Debug.LogWarning($"[TutorialContext] Unknown target id: {id}");
        return null;
    }

    public Transform orcaTarget;
}