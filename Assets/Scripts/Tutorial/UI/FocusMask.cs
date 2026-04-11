using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasRenderer))]
public class FocusMask : MaskableGraphic
{
    public enum HoleShape
    {
        Rectangle,
        Circle
    }

    [Header("Hole Rect (screen space target)")]
    [SerializeField] private RectTransform target;
    [SerializeField] private Vector2 padding = new Vector2(20f, 20f);

    [Header("Shape")]
    [SerializeField] private HoleShape holeShape = HoleShape.Circle;

    [Header("Circle Settings")]
    [SerializeField] private int circleSegments = 64;
    [SerializeField] private float softEdgeWidth = 20f;

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

    public void SetHoleShape(HoleShape shape)
    {
        holeShape = shape;
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

        if (holeShape == HoleShape.Rectangle)
        {
            DrawRectangleHole(vh, rect);
        }
        else
        {
            DrawCircleHole(vh, rect);
        }

        
    }

    private void DrawRectangleHole(VertexHelper vh, Rect rect)
    {
        float innerLeft = Mathf.Clamp(holeRectCanvasSpace.xMin, rect.xMin, rect.xMax);
        float innerRight = Mathf.Clamp(holeRectCanvasSpace.xMax, rect.xMin, rect.xMax);
        float innerBottom = Mathf.Clamp(holeRectCanvasSpace.yMin, rect.yMin, rect.yMax);
        float innerTop = Mathf.Clamp(holeRectCanvasSpace.yMax, rect.yMin, rect.yMax);

        float edge = Mathf.Max(softEdgeWidth, 0.01f);

        float outerLeft = Mathf.Clamp(innerLeft - edge, rect.xMin, rect.xMax);
        float outerRight = Mathf.Clamp(innerRight + edge, rect.xMin, rect.xMax);
        float outerBottom = Mathf.Clamp(innerBottom - edge, rect.yMin, rect.yMax);
        float outerTop = Mathf.Clamp(innerTop + edge, rect.yMin, rect.yMax);

        Color32 baseColor = color;
        Color32 transparent = new Color32(baseColor.r, baseColor.g, baseColor.b, 0);

        // 1) soft edge 바깥 완전 dim 영역
        if (outerTop < rect.yMax)
            AddQuad(vh, new Vector2(rect.xMin, outerTop), new Vector2(rect.xMax, rect.yMax), baseColor);

        if (outerBottom > rect.yMin)
            AddQuad(vh, new Vector2(rect.xMin, rect.yMin), new Vector2(rect.xMax, outerBottom), baseColor);

        if (outerLeft > rect.xMin)
            AddQuad(vh, new Vector2(rect.xMin, outerBottom), new Vector2(outerLeft, outerTop), baseColor);

        if (outerRight < rect.xMax)
            AddQuad(vh, new Vector2(outerRight, outerBottom), new Vector2(rect.xMax, outerTop), baseColor);

        // 2) 위 soft strip
        AddGradientQuad(
            vh,
            new Vector2(innerLeft, innerTop),
            new Vector2(innerRight, outerTop),
            transparent, transparent, baseColor, baseColor
        );

        // 3) 아래 soft strip
        AddGradientQuad(
            vh,
            new Vector2(innerLeft, outerBottom),
            new Vector2(innerRight, innerBottom),
            baseColor, baseColor, transparent, transparent
        );

        // 4) 왼쪽 soft strip
        AddGradientQuad(
            vh,
            new Vector2(outerLeft, innerBottom),
            new Vector2(innerLeft, innerTop),
            baseColor, transparent, transparent, baseColor
        );

        // 5) 오른쪽 soft strip
        AddGradientQuad(
            vh,
            new Vector2(innerRight, innerBottom),
            new Vector2(outerRight, innerTop),
            transparent, baseColor, baseColor, transparent
        );

        // 6) 코너 soft 영역 4개
        // 좌상
        AddGradientQuad(
            vh,
            new Vector2(outerLeft, innerTop),
            new Vector2(innerLeft, outerTop),
            baseColor, transparent, baseColor, baseColor
        );

        // 우상
        AddGradientQuad(
            vh,
            new Vector2(innerRight, innerTop),
            new Vector2(outerRight, outerTop),
            transparent, baseColor, baseColor, baseColor
        );

        // 좌하
        AddGradientQuad(
            vh,
            new Vector2(outerLeft, outerBottom),
            new Vector2(innerLeft, innerBottom),
            baseColor, baseColor, transparent, baseColor
        );

        // 우하
        AddGradientQuad(
            vh,
            new Vector2(innerRight, outerBottom),
            new Vector2(outerRight, innerBottom),
            baseColor, baseColor, baseColor, transparent
        );
    }

    private void DrawCircleHole(VertexHelper vh, Rect rect)
    {
        Vector2 center = holeRectCanvasSpace.center;

        float innerRadius = Mathf.Max(holeRectCanvasSpace.width, holeRectCanvasSpace.height) * 0.5f;
        float outerRadius = innerRadius + Mathf.Max(softEdgeWidth, 0.01f);

        Color32 baseColor = color;
        Color32 transparent = new Color32(baseColor.r, baseColor.g, baseColor.b, 0);

        /*// 화면 전체 배경 먼저 깔기
        AddQuad(vh,
            new Vector2(rect.xMin, rect.yMin),
            new Vector2(rect.xMax, rect.yMax),
            color);*/

        int seg = Mathf.Max(circleSegments, 24);

        // inner circle + outer soft ring를 "지우는" 대신
        // 가운데 투명 원과 가장자리 페이드 링을 덮어쓴다.
        // Unity UI는 알파 블렌딩이므로, 배경 위에 투명 버텍스를 그리면
        // 완전 erase는 안 되므로 여기서는 ring만 따로 만들기 위해
        // 실제론 원형 주변을 세분화한 사다리꼴들로 구성한다.
        //
        // 방법:
        // - 투명 원 내부는 따로 geometry를 만들지 않음
        // - 배경 전체 위에, 원 외곽을 제외한 부분만 triangulation 하긴 복잡하므로
        //   soft edge 포함 원형 mask는 "구멍" 방식 대신
        //   4방향 큰 quad + 원형 ring으로 표현한다.
        //
        // 그래서 아래처럼:
        // 1) soft edge 바깥 기준 bounding rect 밖 4방향 quad
        // 2) outerRadius ~ innerRadius 사이 ring (outer=color, inner=transparent)

        // 화면 4코너 중 center에서 가장 먼 거리
        float farRadius = 0f;
        farRadius = Mathf.Max(farRadius, Vector2.Distance(center, new Vector2(rect.xMin, rect.yMin)));
        farRadius = Mathf.Max(farRadius, Vector2.Distance(center, new Vector2(rect.xMin, rect.yMax)));
        farRadius = Mathf.Max(farRadius, Vector2.Distance(center, new Vector2(rect.xMax, rect.yMin)));
        farRadius = Mathf.Max(farRadius, Vector2.Distance(center, new Vector2(rect.xMax, rect.yMax)));

        // 1) 안쪽 투명 -> 바깥 dim 으로 부드럽게 넘어가는 링
        AddRing(vh, center, innerRadius, outerRadius, transparent, baseColor, seg);

        // 2) soft edge 바깥부터 화면 끝까지는 완전 dim
        AddRing(vh, center, outerRadius, farRadius, baseColor, baseColor, seg);
    }

    private void AddRing(VertexHelper vh, Vector2 center, float innerRadius, float outerRadius, Color32 innerColor, Color32 outerColor, int segments)
    {
        int startIndex = vh.currentVertCount;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float angle = t * Mathf.PI * 2f;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);

            Vector2 inner = center + new Vector2(cos, sin) * innerRadius;
            Vector2 outer = center + new Vector2(cos, sin) * outerRadius;

            vh.AddVert(inner, innerColor, Vector2.zero);
            vh.AddVert(outer, outerColor, Vector2.zero);

            if (i < segments)
            {
                int idx = startIndex + i * 2;

                vh.AddTriangle(idx, idx + 1, idx + 3);
                vh.AddTriangle(idx, idx + 3, idx + 2);
            }
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

    private void AddGradientQuad(
    VertexHelper vh,
    Vector2 min,
    Vector2 max,
    Color32 bottomLeft,
    Color32 bottomRight,
    Color32 topRight,
    Color32 topLeft)
    {
        int startIndex = vh.currentVertCount;

        vh.AddVert(new Vector3(min.x, min.y), bottomLeft, new Vector2(0f, 0f));
        vh.AddVert(new Vector3(min.x, max.y), topLeft, new Vector2(0f, 1f));
        vh.AddVert(new Vector3(max.x, max.y), topRight, new Vector2(1f, 1f));
        vh.AddVert(new Vector3(max.x, min.y), bottomRight, new Vector2(1f, 0f));

        vh.AddTriangle(startIndex + 0, startIndex + 1, startIndex + 2);
        vh.AddTriangle(startIndex + 2, startIndex + 3, startIndex + 0);
    }
}