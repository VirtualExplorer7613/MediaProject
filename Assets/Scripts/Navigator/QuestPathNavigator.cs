using UnityEngine;
using UnityEngine.AI;

public class QuestPathNavigator : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject pathVfxPrefab;

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

    [Header("View Direction")]
    [SerializeField] private Transform viewReference;
    [SerializeField] private bool hideVfxWhenLookingAway = true;
    [SerializeField] private float minViewDotToShowVfx = -0.1f;

    private Vector3 currentVfxPosition;
    private bool hasPlacedVfx = false;

    private Transform target;
    private GameObject vfxInstance;
    private NavMeshPath path;

    private float nextRecalculateTime;
    private Vector3 lastPlayerPosition;
    private bool hasValidPath;

    private void Awake()
    {
        path = new NavMeshPath();

        if (pathVfxPrefab != null)
        {
            vfxInstance = Instantiate(pathVfxPrefab);
            vfxInstance.SetActive(false);
        }
    }

    private void Start()
    {
        if (player != null)
            lastPlayerPosition = player.position;
    }

    private void Update()
    {
        if (player == null || target == null)
        {
            HideVfx();
            hasPlacedVfx = false;
            return;
        }

        float distToTarget = Vector3.Distance(player.position, target.position);
        if (distToTarget <= arrivalDistance)
        {
            HideVfx();
            hasPlacedVfx = false;
            return;
        }

        bool needRecalculate =
            Time.time >= nextRecalculateTime ||
            Vector3.Distance(player.position, lastPlayerPosition) >= playerMoveRefreshDistance;

        if (needRecalculate)
        {
            RecalculatePath();
        }

        if (useDiscreteVfxUpdate)
            UpdateVfxDiscrete();
        else
            UpdateVfxAlongPath();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        hasPlacedVfx = false;
        ForceRecalculate();
    }

    public void ClearTarget()
    {
        target = null;
        hasValidPath = false;
        hasPlacedVfx = false;
        HideVfx();
    }

    private void ForceRecalculate()
    {
        nextRecalculateTime = 0f;
        lastPlayerPosition = player != null ? player.position : Vector3.zero;
        RecalculatePath();
    }

    private void RecalculatePath()
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
    }


    private Vector3 ProjectToGround(Vector3 position)
    {
        Vector3 origin = position + Vector3.up * vfxGroundRayHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, vfxGroundRayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * vfxYOffset;
        }

        return position + Vector3.up * vfxYOffset;
    }

    private Vector3 GetDirectionFromPlayerToGuide(Vector3 guidePos)
    {
        Vector3 dir = guidePos - player.position;
        dir.y = 0f;
        return dir.normalized;
    }

    private void HideVfx()
    {
        if (vfxInstance != null && vfxInstance.activeSelf)
            vfxInstance.SetActive(false);
    }

    #region 이산적인 경로 VFX
    private void UpdateVfxDiscrete()
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
    }

    #endregion

    private float GetHorizontalDistance(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}