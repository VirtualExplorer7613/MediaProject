using UnityEngine;
using UnityEngine.Splines;

public class QuestSequenceTarget : MonoBehaviour
{
    [Header("Sequence ID")]
    [SerializeField] private string objectiveId;

    [Header("Navigation Point")]
    [SerializeField] private Transform navigationPoint;

    [Header("Spline Route")]
    [SerializeField] private SplineContainer routeSpline;

    [Header("Debug")]
    [SerializeField] private bool completed = false;

    [Header("Navigator")]
    [SerializeField] private bool showNavigator = true;

    [Header("Destination VFX")]
    [SerializeField] private GameObject destinationVfxPrefab;
    [SerializeField] private Transform destinationVfxPoint;
    [SerializeField] private bool showDestinationVfx = true;
    [SerializeField] private bool hideDestinationVfxWhenPlayerNear = true;
    [SerializeField] private float destinationVfxHideDistance = 6f;

    public string ObjectiveId => objectiveId;
    public bool Completed => completed;
    public bool ShowNavigator => showNavigator;
    public SplineContainer RouteSpline => routeSpline;

    public GameObject DestinationVfxPrefab => destinationVfxPrefab;
    public bool ShowDestinationVfx => showDestinationVfx;
    public bool HideDestinationVfxWhenPlayerNear => hideDestinationVfxWhenPlayerNear;
    public float DestinationVfxHideDistance => destinationVfxHideDistance;

    public Transform NavigationPoint
    {
        get
        {
            if (navigationPoint != null)
                return navigationPoint;

            return transform;
        }
    }

    public Transform DestinationVfxPoint
    {
        get
        {
            if (destinationVfxPoint != null)
                return destinationVfxPoint;

            return NavigationPoint;
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