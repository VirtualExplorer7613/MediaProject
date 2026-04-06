using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class FocusMask : MaskableGraphic
{
    [Header("Hole Rect (screen space target)")]
    [SerializeField] private RectTransform target;
    [SerializeField] private Vector2 padding = new Vector2(20f, 20f);

    [Header("Behavior")]
    [SerializeField] private bool followTarget = true;
    [SerializeField] private bool maskRaycast = false;

    private Canvas rootCanvas;
    private Rect holeRectCanvasSpace;
    private bool hasHole = false;

    protected override void Awake()
    {
        base.Awake();
        raycastTarget = maskRaycast;
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        SetVerticesDirty();
    }

    public void SetTarget(RectTransform newTarget, Vector2 newPadding)
    {
        target = newTarget;
        padding = newPadding;
        hasHole = target != null;
        UpdateHoleRect();
        SetVerticesDirty();
    }

    public void ClearTarget()
    {
        target = null;
        hasHole = false;
        SetVerticesDirty();
    }

    public void SetPadding(Vector2 newPadding)
    {
        padding = newPadding;
        UpdateHoleRect();
        SetVerticesDirty();
    }

    private void LateUpdate()
    {
        if (!followTarget || target == null || !hasHole)
            return;

        UpdateHoleRect();
        SetVerticesDirty();
    }

    private void UpdateHoleRect()
    {
        if (target == null || rootCanvas == null)
        {
            hasHole = false;
            return;
        }

        Vector3[] worldCorners = new Vector3[4];
        target.GetWorldCorners(worldCorners);

        Camera cam = null;
        if (rootCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = rootCanvas.worldCamera;

        RectTransform canvasRect = transform as RectTransform;
        if (canvasRect == null)
        {
            hasHole = false;
            return;
        }

        Vector2 min;
        Vector2 max;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(cam, worldCorners[0]),
            cam,
            out min);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(cam, worldCorners[2]),
            cam,
            out max);

        holeRectCanvasSpace = Rect.MinMaxRect(
            min.x - padding.x,
            min.y - padding.y,
            max.x + padding.x,
            max.y + padding.y
        );

        hasHole = true;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();

        Rect rect = rectTransform.rect;

        if (!hasHole)
        {
            AddQuad(vh,
                new Vector2(rect.xMin, rect.yMin),
                new Vector2(rect.xMax, rect.yMax),
                color);
            return;
        }

        float left = Mathf.Clamp(holeRectCanvasSpace.xMin, rect.xMin, rect.xMax);
        float right = Mathf.Clamp(holeRectCanvasSpace.xMax, rect.xMin, rect.xMax);
        float bottom = Mathf.Clamp(holeRectCanvasSpace.yMin, rect.yMin, rect.yMax);
        float top = Mathf.Clamp(holeRectCanvasSpace.yMax, rect.yMin, rect.yMax);

        // 위
        if (top < rect.yMax)
        {
            AddQuad(vh,
                new Vector2(rect.xMin, top),
                new Vector2(rect.xMax, rect.yMax),
                color);
        }

        // 아래
        if (bottom > rect.yMin)
        {
            AddQuad(vh,
                new Vector2(rect.xMin, rect.yMin),
                new Vector2(rect.xMax, bottom),
                color);
        }

        // 왼쪽
        if (left > rect.xMin)
        {
            AddQuad(vh,
                new Vector2(rect.xMin, bottom),
                new Vector2(left, top),
                color);
        }

        // 오른쪽
        if (right < rect.xMax)
        {
            AddQuad(vh,
                new Vector2(right, bottom),
                new Vector2(rect.xMax, top),
                color);
        }
    }

    private void AddQuad(VertexHelper vh, Vector2 min, Vector2 max, Color32 col)
    {
        int startIndex = vh.currentVertCount;

        vh.AddVert(new Vector3(min.x, min.y), col, new Vector2(0f, 0f));
        vh.AddVert(new Vector3(min.x, max.y), col, new Vector2(0f, 1f));
        vh.AddVert(new Vector3(max.x, max.y), col, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(max.x, min.y), col, new Vector2(1f, 0f));

        vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);
    }
}