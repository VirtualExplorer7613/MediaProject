using System;
using System.Collections.Generic;
using UnityEngine;

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

    [SerializeField] private List<VfxBinding> bindings = new();

    private readonly Dictionary<string, VfxBinding> map = new(StringComparer.OrdinalIgnoreCase);

    private void Awake()
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

    public void Show(string id)
    {
        if (!map.TryGetValue(id, out var binding))
        {
            Debug.LogWarning($"[TutorialWorldVfxController] Unknown VFX id: {id}");
            return;
        }

        if (binding.target == null || binding.vfxPrefab == null)
        {
            Debug.LogWarning($"[TutorialWorldVfxController] Missing target or prefab: {id}");
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

    public void Hide(string id)
    {
        if (!map.TryGetValue(id, out var binding))
            return;

        if (binding.instance != null)
            binding.instance.SetActive(false);
    }

    public void HideAll()
    {
        foreach (var pair in map)
        {
            if (pair.Value.instance != null)
                pair.Value.instance.SetActive(false);
        }
    }
}