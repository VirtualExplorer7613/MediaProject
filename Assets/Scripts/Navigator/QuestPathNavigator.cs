using UnityEngine;
using UnityEngine.Splines;

public class QuestPathNavigator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject pathVfxPrefab;

    [Header("Spline VFX")]
    [SerializeField] private int sparkleCount = 8;
    [SerializeField] private float flowSpeed = 0.2f;
    [SerializeField] private float yOffset = 0.1f;

    [Header("Arrival")]
    [SerializeField] private float arrivalDistance = 6f;

    #region Variables for NavMeshMethod
    /*
        [Header("Path")]
        [SerializeField] private float recalculateInterval = 1.0f;
        [SerializeField] private float playerMoveRefreshDistance = 3f;
        [SerializeField] private float arrivalDistance = 6f;



        [Header("VFX Placement")]
        [SerializeField] private float vfxDistanceFromPlayer = 6f;
        [SerializeField] private float vfxGroundRayHeight = 8f;
        [SerializeField] private float vfxGroundRayDistance = 30f;
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private float vfxYOffset = 0.05f;
        [SerializeField] private float rotateSmoothSpeed = 10f;
        [SerializeField] private float moveSmoothSpeed = 12f;

        [Header("Discrete VFX Update")]
        [SerializeField] private bool useDiscreteVfxUpdate = true;
        // 플레이어가 VFX에 이 거리 이하로 가까워지면 다음 지점으로 갱신
        [SerializeField] private float advanceRefreshDistance = 2.0f;
        // 플레이어가 VFX에서 이 거리 이상 멀어지면 경로 이탈/후퇴로 보고 갱신
        [SerializeField] private float retreatRefreshDistance = 10.0f;
        // VFX를 새로 배치할 때 부드럽게 이동하지 않고 즉시 이동할지 여부
        [SerializeField] private bool snapVfxOnRefresh = true;
    */
    #endregion

    [Header("Ground")]
    [SerializeField] private bool projectToGround = true;
    [SerializeField] private float groundRayHeight = 8f;
    [SerializeField] private float groundRayDistance = 30f;
    [SerializeField] private LayerMask groundMask;

    [Header("View Direction")]
    [SerializeField] private Transform viewReference;
    [SerializeField] private bool hideVfxWhenLookingAway = true;
    [SerializeField] private float minViewDotToShowVfx = -0.1f;

    /* private Vector3 currentVfxPosition;
     private bool hasPlacedVfx = false;*/

    private QuestSequenceTarget currentTarget;
    private Transform target;
    private SplineContainer currentSpline;

    private GameObject[] pathVfxInstances;
    private GameObject destinationVfxInstance;

    private float flowTime;


    /*private GameObject vfxInstance;
    private NavMeshPath path;*/

    /*private float nextRecalculateTime;
    private Vector3 lastPlayerPosition;
    private bool hasValidPath;*/

    private void Awake()
    {
        CreatePathVfxInstances();
        HideAllVfx();
    }

    private void Update()
    {
        if (player == null || currentTarget == null || target == null)
        {
            HideAllVfx();
            return;
        }

        UpdateDestinationVfx();

        UpdatePathVfx();
    }

    public void SetTarget(QuestSequenceTarget newTarget)
    {
        currentTarget = newTarget;
        if (currentTarget == null)
        {
            ClearTarget();
            return;
        }

        target = currentTarget.NavigationPoint;
        currentSpline = currentTarget.RouteSpline;
        flowTime = 0f;

        SetupDestinationVfx();

        if (currentSpline == null)
        {
            HidePathVfx();
            Debug.LogWarning("[QuestPathNavigator] Spline이 지정되지 않았습니다.");
        }
    }

    public void ClearTarget()
    {
        currentTarget = null;
        target = null;
        currentSpline = null;

        HideAllVfx();

        if (destinationVfxInstance != null)
        {
            Destroy(destinationVfxInstance);
            destinationVfxInstance = null;
        }
    }

    /*private void ForceRecalculate()
    {
        nextRecalculateTime = 0f;
        lastPlayerPosition = player != null ? player.position : Vector3.zero;
        RecalculatePath();
    }*/

    /*    private void RecalculatePath()
        {
            if (player == null || target == null)
                return;

            nextRecalculateTime = Time.time + recalculateInterval;
            lastPlayerPosition = player.position;

            NavMeshHit startHit;
            NavMeshHit targetHit;

            bool startFound = NavMesh.SamplePosition(player.position, out startHit, 5f, NavMesh.AllAreas);
            bool targetFound = NavMesh.SamplePosition(target.position, out targetHit, 8f, NavMesh.AllAreas);

            if (!startFound || !targetFound)
            {
                hasValidPath = false;
                HideVfx();
                return;
            }

            hasValidPath = NavMesh.CalculatePath(
                startHit.position,
                targetHit.position,
                NavMesh.AllAreas,
                path
            );

            if (!hasValidPath || path.corners == null || path.corners.Length < 2)
            {
                HideVfx();
                return;
            }
        }

        private void UpdateVfxAlongPath()
        {
            if (!hasValidPath || vfxInstance == null || path.corners.Length < 2)
            {
                HideVfx();
                return;
            }

            Vector3 guidePos = GetGuidePositionOnPath();
            Vector3 groundPos = ProjectToGround(guidePos);

            Vector3 nextDir = GetDirectionFromPlayerToGuide(groundPos);

            if (!vfxInstance.activeSelf)
            {
                vfxInstance.transform.position = groundPos;
                vfxInstance.SetActive(true);
            }
            else
            {
                vfxInstance.transform.position = Vector3.Lerp(
                    vfxInstance.transform.position,
                    groundPos,
                    Time.deltaTime * moveSmoothSpeed
                );
            }

            if (nextDir.sqrMagnitude > 0.0001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(nextDir, Vector3.up);
                vfxInstance.transform.rotation = Quaternion.Slerp(
                    vfxInstance.transform.rotation,
                    targetRot,
                    Time.deltaTime * rotateSmoothSpeed
                );
            }
        }

        private Vector3 GetGuidePositionOnPath()
        {
            Vector3 current = player.position;
            float remaining = vfxDistanceFromPlayer;

            for (int i = 1; i < path.corners.Length; i++)
            {
                Vector3 from = current;
                Vector3 to = path.corners[i];

                Vector3 segment = to - from;
                segment.y = 0f;

                float segmentLength = segment.magnitude;

                if (segmentLength <= 0.001f)
                {
                    current = to;
                    continue;
                }

                if (remaining <= segmentLength)
                {
                    return from + segment.normalized * remaining;
                }

                remaining -= segmentLength;
                current = to;
            }

            return path.corners[path.corners.Length - 1];
        }*/

    private void CreatePathVfxInstances()
    {
        if (pathVfxPrefab == null)
            return;

        pathVfxInstances = new GameObject[sparkleCount];

        for (int i = 0; i < sparkleCount; i++)
        {
            pathVfxInstances[i] = Instantiate(pathVfxPrefab, transform);
            pathVfxInstances[i].SetActive(false);
        }
    }

    private void UpdatePathVfx()
    {
        if (currentSpline == null)
        {
            HidePathVfx();
            return;
        }

        float distToTarget = Vector3.Distance(player.position, target.position);

        if (distToTarget <= arrivalDistance)
        {
            HidePathVfx();
            return;
        }

        if (!IsPathDirectionVisible())
        {
            HidePathVfx();
            return;
        }

        ShowPathVfx();
        UpdateSplineVfx();
    }

    private void UpdateSplineVfx()
    {
        if (pathVfxInstances == null || currentSpline == null)
            return;

        flowTime += Time.deltaTime * flowSpeed;

        for (int i = 0; i < pathVfxInstances.Length; i++)
        {
            if (pathVfxInstances [i] == null)
                continue;

            float offset = i / (float)pathVfxInstances.Length;
            float t = (flowTime + offset) % 1f;

            Vector3 pos = currentSpline.EvaluatePosition(0, t);

            if (projectToGround)
                pos = ProjectToGround(pos);
            else
                pos += Vector3.up * yOffset;

            pathVfxInstances[i].transform.position = pos;

            Vector3 forwardPos = currentSpline.EvaluatePosition(0, (t + 0.02f) % 1f);
            Vector3 dir = forwardPos - pos;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                pathVfxInstances[i].transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }

    private void SetupDestinationVfx()
    {
        if (destinationVfxInstance != null)
        {
            Destroy(destinationVfxInstance);
            destinationVfxInstance = null;
        }

        if (currentTarget == null)
            return;

        if (!currentTarget.ShowDestinationVfx)
            return;

        if (currentTarget.DestinationVfxPrefab == null)
            return;

        Transform vfxPoint = currentTarget.DestinationVfxPoint;
        if (vfxPoint == null)
            return;

        destinationVfxInstance = Instantiate(
            currentTarget.DestinationVfxPrefab,
            vfxPoint.position,
             GetDestinationVfxRotation(vfxPoint)
        );

        destinationVfxInstance.transform.localScale = currentTarget.DestinationVfxScale;
        destinationVfxInstance.SetActive(true);
    }

    private void UpdateDestinationVfx()
    {
        if (destinationVfxInstance == null || currentTarget == null)
            return;

        Transform vfxPoint = currentTarget.DestinationVfxPoint;
        if (vfxPoint == null)
            return;

        if (!currentTarget.ShowDestinationVfx)
        {
            destinationVfxInstance.SetActive(false);
            return;
        }

        if (!currentTarget.HideDestinationVfxWhenPlayerNear)
        {
            if (!destinationVfxInstance.activeSelf)
                destinationVfxInstance.SetActive(true);
            return;
        }

        if (currentTarget.FollowDestinationVfxPoint)
        {
            destinationVfxInstance.transform.position = vfxPoint.position;
            destinationVfxInstance.transform.rotation = GetDestinationVfxRotation(vfxPoint);
        }

        float distance = Vector3.Distance(player.position, vfxPoint.position);

        bool shouldShow = distance > currentTarget.DestinationVfxHideDistance;
        //destinationVfxInstance.SetActive(shouldShow);

        if (destinationVfxInstance.activeSelf != shouldShow)
            destinationVfxInstance.SetActive(shouldShow);
    }

    private Vector3 ProjectToGround(Vector3 position)
    {
        Vector3 origin = position + Vector3.up * groundRayHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, groundRayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * yOffset;
        }

        return position + Vector3.up * yOffset;
    }

    private bool IsPathDirectionVisible()
    {
        if (!hideVfxWhenLookingAway)
            return true;

        Transform reference = viewReference != null ? viewReference : player;
        if (reference == null || target == null)
            return true;

        Vector3 pathDir = target.position - player.position;
        Vector3 viewDir = reference.forward;

        pathDir.y = 0f;
        viewDir.y = 0f;

        if (pathDir.sqrMagnitude <= 0.0001f || viewDir.sqrMagnitude <= 0.0001f)
            return true;

        pathDir.Normalize();
        viewDir.Normalize();

        float dot = Vector3.Dot(viewDir, pathDir);
        return dot >= minViewDotToShowVfx;
    }

    /*private Vector3 GetDirectionFromPlayerToGuide(Vector3 guidePos)
    {
        Vector3 dir = guidePos - player.position;
        dir.y = 0f;
        return dir.normalized;
    }*/

    private void ShowPathVfx()
    {
        if (pathVfxInstances == null)
            return;

        foreach (var vfx in pathVfxInstances)
        {
            if (vfx != null && !vfx.activeSelf)
                vfx.SetActive(true);
        }
    }

    private void HidePathVfx()
    {
        if (pathVfxInstances == null)
            return;

        foreach (var vfx in pathVfxInstances)
        {
            if (vfx != null && vfx.activeSelf)
                vfx.SetActive(false);
        }
    }

    private void HideDestinationVfx()
    {
        if (destinationVfxInstance != null)
            destinationVfxInstance.SetActive(false);
    }

    private void HideAllVfx()
    {
        HidePathVfx();
        HideDestinationVfx();
    }

    #region 이산적인 경로 VFX
    /*private void UpdateVfxDiscrete()
    {
        if (!hasValidPath || vfxInstance == null || path.corners.Length < 2)
        {
            HideVfx();
            hasPlacedVfx = false;
            return;
        }

        if (!IsPathDirectionVisible())
        {
            HideVfx();
            return;
        }

        if (!hasPlacedVfx)
        {
            PlaceVfxAtNextGuidePoint();
            return;
        }

        //float distanceToVfx = Vector3.Distance(player.position, currentVfxPosition);
        float distanceToVfx = GetHorizontalDistance(player.position, currentVfxPosition);

        bool playerReachedVfx = distanceToVfx <= advanceRefreshDistance;
        bool playerMovedAwayFromVfx = distanceToVfx >= retreatRefreshDistance;

        if (playerReachedVfx || playerMovedAwayFromVfx)
        {
            PlaceVfxAtNextGuidePoint();
            return;
        }

        if (!vfxInstance.activeSelf)
            vfxInstance.SetActive(true);
    }

    private void PlaceVfxAtNextGuidePoint()
    {
        Vector3 guidePos = GetGuidePositionOnPath();
        Vector3 groundPos = ProjectToGround(guidePos);

        currentVfxPosition = groundPos;
        hasPlacedVfx = true;

        vfxInstance.transform.position = groundPos;

        Vector3 nextDir = GetDirectionFromPlayerToGuide(groundPos);

        if (nextDir.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(nextDir, Vector3.up);
            vfxInstance.transform.rotation = targetRot;
        }

        if (!vfxInstance.activeSelf)
            vfxInstance.SetActive(true);
    }

    private bool IsPathDirectionVisible()
    {
        if (!hideVfxWhenLookingAway)
            return true;

        Transform reference = viewReference != null ? viewReference : player;
        if (reference == null)
            return true;

        Vector3 pathDir = GetCurrentPathDirectionFromPlayer();
        Vector3 viewDir = reference.forward;

        pathDir.y = 0f;
        viewDir.y = 0f;

        if (pathDir.sqrMagnitude <= 0.0001f || viewDir.sqrMagnitude <= 0.0001f)
            return true;

        pathDir.Normalize();
        viewDir.Normalize();

        float dot = Vector3.Dot(viewDir, pathDir);

        return dot >= minViewDotToShowVfx;
    }

    private Vector3 GetCurrentPathDirectionFromPlayer()
    {
        if (path == null || path.corners == null || path.corners.Length < 2)
            return target != null ? target.position - player.position : Vector3.zero;

        Vector3 from = player.position;

        for (int i = 1; i < path.corners.Length; i++)
        {
            Vector3 to = path.corners[i];
            Vector3 dir = to - from;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.25f)
                return dir.normalized;

            from = to;
        }

        return target != null ? target.position - player.position : Vector3.zero;
    }*/

    #endregion

    private float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    // 회전값 계산 함수
    private Quaternion GetDestinationVfxRotation(Transform vfxPoint)
    {
        if (currentTarget != null && currentTarget.UseWorldRotationForDestinationVfx)
        {
            return Quaternion.Euler(currentTarget.DestinationVfxWorldEuler);
        }

        return vfxPoint.rotation;
    }
}