using UnityEngine;

public class QuestSequenceTarget : MonoBehaviour
{
    [Header("Sequence ID")]
    [SerializeField] private string objectiveId;

    [Header("Navigation Point")]
    [SerializeField] private Transform navigationPoint;

    [Header("Debug")]
    [SerializeField] private bool completed = false;

    [Header("Navigator")]
    [SerializeField] private bool showNavigator = true;

    public string ObjectiveId => objectiveId;
    public bool Completed => completed;
    public bool ShowNavigator => showNavigator;

    public Transform NavigationPoint
    {
        get
        {
            if (navigationPoint != null)
                return navigationPoint;

            return transform;
        }
    }

    public bool CanInteractHere()
    {
        if (QuestSequenceManager.Instance == null)
            return true;

        return QuestSequenceManager.Instance.CanInteract(objectiveId);
    }

    public bool IsCurrentTarget()
    {
        if (QuestSequenceManager.Instance == null)
            return false;

        return QuestSequenceManager.Instance.IsCurrentObjective(objectiveId);
    }

    public void NotifyCompleted()
    {
        if (completed)
            return;

        completed = true;

        if (QuestSequenceManager.Instance != null)
            QuestSequenceManager.Instance.CompleteObjective(objectiveId);
    }

    public void ForceSetCompleted(bool value)
    {
        completed = value;
    }
}