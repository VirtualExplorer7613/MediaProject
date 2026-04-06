using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHighlighter : MonoBehaviour
{
    [Serializable]
    public class HighlightTarget
    {
        public string id;

        [Header("Actual UI/Object")]
        public GameObject realTargetObject;

        [Header("Rect used for focus hole")]
        public RectTransform focusTarget;

        [Header("Optional custom frame anchor")]
        public RectTransform frameTarget;

        [Header("Start State")]
        public bool hideAtStart = false;

        public GameObject targetObject; // 예: 조이스틱 UI 루트, 아이템 버튼 루트
    }

    [Header("Registered Targets")]
    [SerializeField] private List<HighlightTarget> targets = new();
    [SerializeField] private bool disableOthers = true;

    [Header("Overlay References")]
    [SerializeField] private FocusMask focusMask;
    [SerializeField] private RectTransform focusFrame;
    [SerializeField] private GameObject overlayRoot;

    [Header("Visual Tuning")]
    [SerializeField] private Vector2 padding = new Vector2(22f, 22f);
    [SerializeField] private bool keepFollowingTarget = true;

    private readonly Dictionary<string, HighlightTarget> map = new(StringComparer.OrdinalIgnoreCase);
    private HighlightTarget currentTarget;

    private void Awake()
    {
        map.Clear();

        foreach (var t in targets)
        {
            if (t == null || string.IsNullOrWhiteSpace(t.id))
                continue;

            map[t.id] = t;

            if (t.realTargetObject != null && t.hideAtStart)
            {
                t.realTargetObject.SetActive(false);
            }
        }

        Clear();
    }

    private void LateUpdate()
    {
        if (!keepFollowingTarget || currentTarget == null)
            return;

        UpdateVisuals(currentTarget);
    }

    public void Highlight(string id)
    {
        if (!map.TryGetValue(id, out var target))
        {
            Debug.LogWarning($"[TutorialHighlighter] Unknown target id: {id}");
            Clear();
            return;
        }

        currentTarget = target;

        if (target.realTargetObject != null)
            target.realTargetObject.SetActive(true);

        if (overlayRoot != null)
            overlayRoot.SetActive(true);

        UpdateVisuals(target);
    }

    public void ShowTarget(string id, bool show)
    {
        if (!map.TryGetValue(id, out var target))
            return;

        if (target.realTargetObject != null)
            target.realTargetObject.SetActive(show);
    }

    public void Clear()
    {
        currentTarget = null;

        if (overlayRoot != null)
            overlayRoot.SetActive(false);

        if (focusMask != null)
            focusMask.ClearTarget();

        if (focusFrame != null)
            focusFrame.gameObject.SetActive(false);
    }

    private void UpdateVisuals(HighlightTarget target)
    {
        if (target == null) return;

        RectTransform holeTarget = target.focusTarget;
        RectTransform frameAnchor = target.frameTarget != null ? target.frameTarget : target.focusTarget;

        if (holeTarget == null)
        {
            Debug.LogWarning($"[TutorialHighlighter] focusTarget missing: {target.id}");
            return;
        }

        if (focusMask != null)
            focusMask.SetTarget(holeTarget, padding);

        if (frameAnchor != null && focusFrame != null)
        {
            ApplyRectToOverlay(frameAnchor, focusFrame, padding);
            focusFrame.gameObject.SetActive(true);
        }
    }

    private void ApplyRectToOverlay(RectTransform source, RectTransform target, Vector2 extraPadding)
    {
        if (source == null || target == null)
            return;

        Vector3[] corners = new Vector3[4];
        source.GetWorldCorners(corners);

        Vector3 center = (corners[0] + corners[2]) * 0.5f;
        Vector2 size = corners[2] - corners[0];
        size += extraPadding * 2f;

        target.position = center;
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, size.x);
        target.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, size.y);
    }
}
