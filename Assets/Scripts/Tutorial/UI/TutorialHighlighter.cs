using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHighlighter : MonoBehaviour
{
    [Serializable]
    public class HighlightTarget
    {
        public string id;
        public GameObject targetObject; // 예: 조이스틱 UI 루트, 아이템 버튼 루트
    }

    [SerializeField] private List<HighlightTarget> targets = new();
    [SerializeField] private bool disableOthers = true;

    private Dictionary<string, GameObject> map;

    private void Awake()
    {
        map = new Dictionary<string, GameObject>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in targets)
        {
            if (t != null && !string.IsNullOrEmpty(t.id) && t.targetObject != null)
                map[t.id] = t.targetObject;
        }
    }

    public void Highlight(string id)
    {
        if (disableOthers)
        {
            foreach (var kv in map)
                kv.Value.SetActive(false);
        }

        if (map.TryGetValue(id, out var go))
            go.SetActive(true);
    }

    public void Clear()
    {
        if (!disableOthers) return;
        foreach (var kv in map)
            kv.Value.SetActive(false);
    }
}
