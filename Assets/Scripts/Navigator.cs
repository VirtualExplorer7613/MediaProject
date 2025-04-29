using UnityEngine;

public class Navigator3D : MonoBehaviour
{
    [Header("Navigation Settings")]
    public Transform player;                  // 플레이어 Transform
    public string[] targetTags;                // 찾을 타겟 태그 목록
    public GameObject arrowPrefab;             // 3D 화살표 프리팹
    public float arrowHeight = 2f;              // 플레이어 머리 위 화살표 높이
    public float searchRadius = 50f;            // 탐색 반경
    public float arrivalThreshold = 2f;         // 도착 판단 거리
    public float arrowSmoothRotateSpeed = 5f;   // 화살표 회전 부드러움

    private GameObject currentTarget = null;
    private GameObject arrowInstance = null;

    void Start()
    {
        if (arrowPrefab != null)
        {
            arrowInstance = Instantiate(arrowPrefab);
            arrowInstance.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            FindNewTarget();
        }
        UpdateNavigation();
    }

    void UpdateNavigation()
    {
        UpdateArrowPosition(); // ⭐ 항상 플레이어 따라 화살표 위치 갱신

        if (currentTarget != null)
        {
            if (!TargetIsInvalid(currentTarget) && !IsArrived(currentTarget))
            {
                UpdateArrowRotation(currentTarget);

                if (!arrowInstance.activeSelf)
                    arrowInstance.SetActive(true);
            }
            else
            {
                // 도착했거나 무효: 타겟 해제 + 화살표 끄기
                currentTarget = null;
                if (arrowInstance.activeSelf)
                    arrowInstance.SetActive(false);
            }
        }
        else
        {
            if (arrowInstance.activeSelf)
                arrowInstance.SetActive(false);
        }
    }

    public void FindNewTarget()
    {
        currentTarget = null;

        currentTarget = FindNearestTarget();

        if (currentTarget == null && arrowInstance != null)
        {
            arrowInstance.SetActive(false);

            // 필요하면 방향 초기화까지
            // arrowInstance.transform.rotation = Quaternion.identity;
        }
    }

    GameObject FindNearestTarget()
    {
        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var tag in targetTags)
        {
            GameObject[] foundTargets = GameObject.FindGameObjectsWithTag(tag);

            foreach (var target in foundTargets)
            {
                if (target == null || !target.activeInHierarchy)
                    continue; // 죽은 오브젝트는 무시

                if (target.GetComponent<Collider>() == null)
                    continue;

                float dist = Vector3.Distance(player.position, target.transform.position);

                if (dist < minDist && dist <= searchRadius)
                {
                    minDist = dist;
                    nearest = target;
                }
            }
        }

        return nearest;
    }

    bool TargetIsInvalid(GameObject target)
    {
        return target == null || !target.activeInHierarchy;
    }

    bool IsArrived(GameObject target)
    {
        if (target == null) return false;

        float dist = Vector3.Distance(player.position, target.transform.position);
        return dist <= arrivalThreshold;
    }

    void UpdateArrowPosition()
    {
        if (arrowInstance == null || player == null) return;

        Vector3 arrowPos = player.position + Vector3.up * arrowHeight;
        arrowInstance.transform.position = arrowPos;
    }

    void UpdateArrowRotation(GameObject target)
    {
        if (arrowInstance == null || target == null) return;

        Vector3 dir = target.transform.position - player.position;
        dir.y = 0f; // y축 제거해서 기울지 않게
        dir.Normalize();

        if (dir != Vector3.zero)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
            arrowInstance.transform.rotation = Quaternion.Slerp(
                arrowInstance.transform.rotation,
                targetRot,
                Time.deltaTime * arrowSmoothRotateSpeed
            );
        }
    }
}
