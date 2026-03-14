using UnityEngine;

public class PullableTrash : MonoBehaviour
{
    public enum PullAxisType
    {
        LocalUp,
        LocalDown,
        LocalRight,
        LocalLeft,
        LocalForward,
        LocalBack,
        WorldUp,
        WorldDown,
        WorldRight,
        WorldLeft,
        WorldForward,
        WorldBack
    }

    [Header("Pull Settings")]
    [Tooltip("쓰레기를 뽑기 위해 필요한 누적 거리")]
    [SerializeField] private float requiredPullAmount = 300f;
    [SerializeField] private bool useDirectionalCheck = true;
    [SerializeField] private PullAxisType pullAxis = PullAxisType.LocalUp;
    //[SerializeField] private Vector2 validScreenDirection = Vector2.up;
    [SerializeField, Range(0f, 1f)] private float requiredDirectionDot = 0.5f;
    [SerializeField] private float movementPerDragUnit = 0.002f;
    [SerializeField] private float maxVisualPullDistance = 0.5f;

    [Header("Distance")]
    [SerializeField] private float maxPlayerDistance = 4f;

    /*[Header("Feedback")]
    [SerializeField] private GameObject highlightObject;*/
    // [SerializeField] private Animator animator;

    private float currentPullAmount = 0f;
    private bool isPulledOut = false;

    private Vector3 initialPosition;
    private bool hasCachedInitialPosition = false;

    public bool IsPulledOut => isPulledOut;

    public float Progress01 => Mathf.Clamp01(currentPullAmount / requiredPullAmount);

    private void Awake()
    {
        CacheInitialPosition();
    }

    private void CacheInitialPosition()
    {
        if (hasCachedInitialPosition) return;
        initialPosition = transform.position;
        hasCachedInitialPosition = true;
    }

    public bool CanStartPull(Transform player)
    {
        if (isPulledOut) return false;
        if (player == null) return false;

        float dist = Vector3.Distance(player.position, transform.position);
        return dist <= maxPlayerDistance;
    }

    public void BeginPull()
    {
        // 당기기 시작할 때 사용할 기능 작성
    }

    // 드래그가 중간에 취소되었을 때 호출하는 함수
    public void CancelPull(bool resetProgress = true)
    {

        if (resetProgress)
        {
            currentPullAmount = 0f;
            transform.position = initialPosition;
        }  
    }

    public bool ApplyDrag(Vector2 screenDelta, Camera cam)
    {
        if (isPulledOut) return false;
        if (cam == null) return false;
        if (screenDelta.sqrMagnitude <= 0.001f) return false;

        Vector3 worldDrag = cam.transform.right * screenDelta.x +
                            cam.transform.up * screenDelta.y;

        Vector3 dragDir = worldDrag.normalized;
        Vector3 targetAxis = GetPullDirection();

        float addAmount = 0f;

        if (useDirectionalCheck)
        {
            Vector2 dir = screenDelta.normalized;
            // 현재 드래그하는 방향과 목표로 하는 방향의 차이를 계산 
            float dot = Vector3.Dot(dragDir, targetAxis);

            if (dot >= requiredDirectionDot)
            {
                // 목표 방향과 유사할수록 빨리 뽑힌다 
                addAmount = screenDelta.magnitude * dot;
            }
        }
        else
        {
            addAmount = screenDelta.magnitude;
        }

        if (addAmount <= 0f)
            return false;

        currentPullAmount += addAmount;

        UpdateVisualPosition();

        // 필요하면 살짝 흔들리게 하거나 위치 보정 가능
        // transform.position += OOO

        if (currentPullAmount >= requiredPullAmount)
        {
            CompletePull();
            return true;
        }

        return false;
    }

    private void UpdateVisualPosition()
    {
        Vector3 axis = GetPullDirection();
        float visualDistance = Mathf.Min(currentPullAmount * movementPerDragUnit, maxVisualPullDistance);
        transform.position = initialPosition + axis * visualDistance;
    }

    private Vector3 GetPullDirection()
    {
        return pullAxis switch
        {
            PullAxisType.LocalUp => transform.up,
            PullAxisType.LocalDown => -transform.up,
            PullAxisType.LocalRight => transform.right,
            PullAxisType.LocalLeft => -transform.right,
            PullAxisType.LocalForward => transform.forward,
            PullAxisType.LocalBack => -transform.forward,

            PullAxisType.WorldUp => Vector3.up,
            PullAxisType.WorldDown => Vector3.down,
            PullAxisType.WorldRight => Vector3.right,
            PullAxisType.WorldLeft => Vector3.left,
            PullAxisType.WorldForward => Vector3.forward,
            PullAxisType.WorldBack => Vector3.back,

            _ => transform.up
        };
    }

    private void CompletePull()
    {
        if (isPulledOut) return;

        isPulledOut = true;

        Debug.Log($"[PullableTrash] Pulled out: {name}");

        // TODO:
        // 점수 반영
        // 퀘스트 반영
        // 사운드 재생
        // Managers.Sound.Play(...)
        // 아이템 획득 처리
        // 비활성화 or 파괴
        gameObject.SetActive(false);
    }
}