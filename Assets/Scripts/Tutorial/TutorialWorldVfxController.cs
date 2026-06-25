using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class TutorialWorldVfxController : MonoBehaviour
{
    [Serializable]
    public class VfxBinding
    {
        public string id;
        public Transform target;
        public GameObject vfxPrefab;

        [Header("Placement")]
        public Vector3 localOffset = Vector3.zero;
        public bool parentToTarget = true;

        [HideInInspector] public GameObject instance;
    }

    [Serializable]
    public class SplineVfxBinding
    {
        public string id;

        [Header("Route")]
        public SplineContainer routeSpline;
        public Transform target;

        [Header("VFX")]
        public GameObject pathVfxPrefab;
        public int sparkleCount = 8;
        public float flowSpeed = 0.2f;
        public float yOffset = 0.1f;

        [Header("Arrival")]
        public float arrivalDistance = 6f;

        [Header("Ground")]
        public bool projectToGround = true;
        public float groundRayHeight = 8f;
        public float groundRayDistance = 30f;
        public LayerMask groundMask;

        [Header("View Direction")]
        public Transform viewReference;
        public bool hideVfxWhenLookingAway = true;
        public float minViewDotToShowVfx = -0.1f;

        [HideInInspector] public GameObject[] instances;
        [HideInInspector] public bool active;
        [HideInInspector] public float flowTime;
    }

    [Header("References")]
    [SerializeField] private Transform player;

    [Header("Normal World VFX")]
    [SerializeField] private List<VfxBinding> bindings = new();

    [Header("Spline World VFX")]
    [SerializeField] private List<SplineVfxBinding> splineBindings = new();

    private readonly Dictionary<string, VfxBinding> map = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, SplineVfxBinding> splineMap = new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
    {
        BuildNormalMap();
        BuildSplineMap();
        HideAll();
    }

    private void Update()
    {
        foreach (var pair in splineMap)
        {
            UpdateSplineVfx(pair.Value);
        }
    }

    private void BuildNormalMap()
    {
        map.Clear();

        foreach (var binding in bindings)
        {
            if (binding == null || string.IsNullOrWhiteSpace(binding.id))
                continue;

            map[binding.id] = binding;

            if (binding.instance != null)
                binding.instance.SetActive(false);
        }
    }

    private void BuildSplineMap()
    {
        splineMap.Clear();

        foreach (var binding in splineBindings)
        {
            if (binding == null || string.IsNullOrWhiteSpace(binding.id))
                continue;

            splineMap[binding.id] = binding;
            binding.active = false;
            binding.flowTime = 0f;
        }
    }

    public void Show(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (map.TryGetValue(id, out var normalBinding))
        {
            ShowNormal(normalBinding);
            return;
        }

        if (splineMap.TryGetValue(id, out var splineBinding))
        {
            ShowSpline(splineBinding);
            return;
        }

        Debug.LogWarning($"[TutorialWorldVfxController] Unknown VFX id: {id}");
    }

    public void Hide(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
            return;

        if (map.TryGetValue(id, out var normalBinding))
        {
            HideNormal(normalBinding);
            return;
        }

        if (splineMap.TryGetValue(id, out var splineBinding))
        {
            HideSpline(splineBinding);
            return;
        }
    }

    public void HideAll()
    {
        foreach (var pair in map)
            HideNormal(pair.Value);

        foreach (var pair in splineMap)
            HideSpline(pair.Value);
    }

    private void ShowNormal(VfxBinding binding)
    {
        if (binding.target == null || binding.vfxPrefab == null)
        {
            Debug.LogWarning($"[TutorialWorldVfxController] Missing target or prefab: {binding.id}");
            return;
        }

        if (binding.instance == null)
        {
            Transform parent = binding.parentToTarget ? binding.target : null;
            binding.instance = Instantiate(binding.vfxPrefab, parent);
        }

        if (binding.parentToTarget)
        {
            binding.instance.transform.localPosition = binding.localOffset;
            binding.instance.transform.localRotation = Quaternion.identity;
        }
        else
        {
            binding.instance.transform.position = binding.target.position + binding.localOffset;
        }

        binding.instance.SetActive(true);
    }

    private void HideNormal(VfxBinding binding)
    {
        if (binding.instance != null)
            binding.instance.SetActive(false);
    }

    private void ShowSpline(SplineVfxBinding binding)
    {
        if (player == null)
        {
            Debug.LogWarning($"[TutorialWorldVfxController] Player is missing. id={binding.id}");
            return;
        }

        if (binding.routeSpline == null || binding.target == null || binding.pathVfxPrefab == null)
        {
            Debug.LogWarning($"[TutorialWorldVfxController] Missing spline, target, or prefab. id={binding.id}");
            return;
        }

        CreateSplineInstancesIfNeeded(binding);

        binding.flowTime = 0f;
        binding.active = true;

        SetSplineInstancesVisible(binding, true);
    }

    private void HideSpline(SplineVfxBinding binding)
    {
        binding.active = false;
        SetSplineInstancesVisible(binding, false);
    }

    private void CreateSplineInstancesIfNeeded(SplineVfxBinding binding)
    {
        int count = Mathf.Max(1, binding.sparkleCount);

        if (binding.instances != null && binding.instances.Length == count)
            return;

        if (binding.instances != null)
        {
            foreach (var instance in binding.instances)
            {
                if (instance != null)
                    Destroy(instance);
            }
        }

        binding.instances = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            binding.instances[i] = Instantiate(binding.pathVfxPrefab, transform);
            binding.instances[i].SetActive(false);
        }
    }

    private void UpdateSplineVfx(SplineVfxBinding binding)
    {
        if (!binding.active)
            return;

        if (player == null || binding.routeSpline == null || binding.target == null)
        {
            SetSplineInstancesVisible(binding, false);
            return;
        }

        float distToTarget = Vector3.Distance(player.position, binding.target.position);

        if (distToTarget <= binding.arrivalDistance)
        {
            SetSplineInstancesVisible(binding, false);
            return;
        }

        if (!IsPathDirectionVisible(binding))
        {
            SetSplineInstancesVisible(binding, false);
            return;
        }

        SetSplineInstancesVisible(binding, true);

        if (binding.instances == null || binding.instances.Length == 0)
            return;

        binding.flowTime += Time.deltaTime * binding.flowSpeed;

        for (int i = 0; i < binding.instances.Length; i++)
        {
            GameObject instance = binding.instances[i];
            if (instance == null)
                continue;

            float offset = i / (float)binding.instances.Length;
            float t = (binding.flowTime + offset) % 1f;

            Vector3 pos = binding.routeSpline.EvaluatePosition(0, t);

            if (binding.projectToGround)
                pos = ProjectToGround(pos, binding);
            else
                pos += Vector3.up * binding.yOffset;

            instance.transform.position = pos;

            float nextT = (t + 0.02f) % 1f;
            Vector3 forwardPos = binding.routeSpline.EvaluatePosition(0, nextT);

            if (binding.projectToGround)
                forwardPos = ProjectToGround(forwardPos, binding);
            else
                forwardPos += Vector3.up * binding.yOffset;

            Vector3 dir = forwardPos - pos;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                instance.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }
    }

    private Vector3 ProjectToGround(Vector3 position, SplineVfxBinding binding)
    {
        Vector3 origin = position + Vector3.up * binding.groundRayHeight;

        if (Physics.Raycast(
                origin,
                Vector3.down,
                out RaycastHit hit,
                binding.groundRayDistance,
                binding.groundMask,
                QueryTriggerInteraction.Ignore))
        {
            return hit.point + Vector3.up * binding.yOffset;
        }

        return position + Vector3.up * binding.yOffset;
    }

    private bool IsPathDirectionVisible(SplineVfxBinding binding)
    {
        if (!binding.hideVfxWhenLookingAway)
            return true;

        Transform reference = binding.viewReference != null ? binding.viewReference : player;
        if (reference == null || binding.target == null)
            return true;

        Vector3 pathDir = binding.target.position - player.position;
        Vector3 viewDir = reference.forward;

        pathDir.y = 0f;
        viewDir.y = 0f;

        if (pathDir.sqrMagnitude <= 0.0001f || viewDir.sqrMagnitude <= 0.0001f)
            return true;

        pathDir.Normalize();
        viewDir.Normalize();

        float dot = Vector3.Dot(viewDir, pathDir);
        return dot >= binding.minViewDotToShowVfx;
    }

    private void SetSplineInstancesVisible(SplineVfxBinding binding, bool visible)
    {
        if (binding.instances == null)
            return;

        foreach (var instance in binding.instances)
        {
            if (instance != null && instance.activeSelf != visible)
                instance.SetActive(visible);
        }
    }
}