using UnityEngine;

public class GameObjectActiveCondition : TutorialCondition
{
    [SerializeField] private GameObject target;
    [SerializeField] private bool requiredActive = true;

    private void Update()
    {
        if (!isActive || isCompleted || target == null) return;

        if (target.activeSelf == requiredActive)
            Complete();
    }
}