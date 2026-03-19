using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Terresquall;


public class WhaleController : MonoBehaviour
{

    private enum MobileControlMode
    {
        Legacy,
        ImprovedLegacy
    }


    [SerializeField] private MobileControlMode mobileControlMode = MobileControlMode.ImprovedLegacy;
    [SerializeField] private GameObject whaleVisualRoot;

    [Header("view")]
    [SerializeField]
    private Texture2D crosshair;
    [SerializeField]
    [Tooltip("上 기울기 조절(-90 ~ 0)")]
    [Range(-90f, 0f)] private float minPitch = -60f;
    [SerializeField]
    [Tooltip("下 시야각 조절(0 ~ 90)")]
    [Range(0f, 90f)] private float maxPitch = 20f;

    [Header("Movement")]
    [SerializeField]
    float moveSpeed = 10f;
    [SerializeField] 
    float verticalSpeed = 10f;   // 수직 이동 속도
    [SerializeField]
    float mouseSensitivity = 100f;

    [SerializeField] private bool forceMobileInput = false; // 디버그용 플래그

    [Header("Mobile Tuning")]
    [SerializeField] private float turnSpeed = 120f;   // 조이스틱 X로 회전 속도(도/초)

    [SerializeField] private float mobileDeadZone = 0.18f;
    [SerializeField] private float inputSmoothTime = 0.08f;
    [SerializeField] private float backwardTurnMultiplier = 0.75f; // 후진 중 회전 감도 감소
    [SerializeField] private float maxForwardSpeedMultiplier = 1f;
    [SerializeField] private float maxBackwardSpeedMultiplier = 0.75f;


    [Header("Mobile Vertical Buttons")]
    [SerializeField] private HoldButton upButton;
    [SerializeField] private HoldButton downButton;

    [Header("Shoot")]
    [SerializeField]
    GameObject ultrasoundPrefab;
    [SerializeField]
    Transform shootPoint;

    [Header("Dragging")]
    [SerializeField]
    float dragDetectionRadius = 10f;

    [SerializeField] private CapsuleCollider capsule;
    [SerializeField] private float skinWidth = 0.02f;
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float groundCheckDistance = 0.3f;
    [SerializeField] private float groundProbeRadius = 0.4f;

    // Dialogue snap
    bool isFrozen = false;
    Coroutine snapCo;

    private DraggedItem currentDraggedItem;
    DialogueManager dialogueManager;


    float yaw = 0f;   // 좌우 회전
    float pitch = 0f; // 상하 기울기

    private Rigidbody rb;
    private Vector3 moveInput;

    [Header("Vertical Collision")]
    [SerializeField] private LayerMask verticalBlockMask;     // 상하 이동 막을 레이어

    private Vector2 mobileInput;
    private Vector2 mobileInputVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        //Cursor.visible = false;   // 커서 안보이게

#if UNITY_EDITOR || UNITY_STANDALONE
        Cursor.lockState = CursorLockMode.Locked;
#endif

        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = (e.x > 180f) ? e.x - 360f : e.x;   // -180~180 보정

        // pitch 제한 적용
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        dialogueManager = FindObjectOfType<DialogueManager>();

        if (verticalBlockMask.value == 0)
            verticalBlockMask = groundMask;
    }

    void Update()
    {

        if (isFrozen || dialogueManager != null && dialogueManager.IsDialoguePlaying)
        {
            // 대화 중이면 아무 조작도 못하게 막음
            moveInput = Vector3.zero;
            return;
        }
#if UNITY_EDITOR || UNITY_STANDALONE
        //HandleItemDragging();
#endif

#if UNITY_EDITOR || UNITY_STANDALONE
        if (Input.GetKeyDown(KeyCode.F))
        {
            FireUltrasound();
        }
#endif
        // Unity remote 디버그용
        //forceMobileInput = true;

        bool useMobileInput = Application.isMobilePlatform || forceMobileInput;

        if (useMobileInput)
        {
            HandleMobileRotationAndMove();
        }
        else
        {
            HandleRotationPC();
            HandleMovePC();
        }
    }

    void HandleMovePC()
    {
        // 이동 입력
        float h = Input.GetAxis("Horizontal"); // A, D
        float v = Input.GetAxis("Vertical");   // W, S

        float y = 0f; // Q, E
        if (Input.GetKey(KeyCode.E)) y = 1f;   // 상승
        if (Input.GetKey(KeyCode.Q)) y = -1f;   // 하강

        // 고래의 방향 기준으로 이동
        //Vector3 moveDir = transform.forward * v + transform.right * h;
       /* Vector3 velocity = (transform.forward * v + transform.right * h) * moveSpeed + transform.up * y * verticalSpeed;
        transform.position += velocity * Time.deltaTime;*/

        moveInput = new Vector3(h, y, v);
    }

    void HandleRotationPC()
    {
        if (dialogueManager != null && dialogueManager.IsDialoguePlaying)
            return;

        // 마우스 입력
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // 너무 위아래로 돌지 않도록 제한

        // 고래 회전 적용
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void HandleMobileRotationAndMove()
    {
        float rawX = VirtualJoystick.GetAxis("Horizontal");
        float rawY = VirtualJoystick.GetAxis("Vertical");

        if (mobileControlMode == MobileControlMode.Legacy)
        {
            float verticalMove = 0f;
            if (upButton != null && upButton.IsPressed) verticalMove += 1f;
            if (downButton != null && downButton.IsPressed) verticalMove -= 1f;

            yaw += rawX * turnSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            moveInput = new Vector3(0f, verticalMove, rawY);
            return;
        }

        // ImprovedLegacy
        Vector2 raw = new Vector2(rawX, rawY);

        // 1. 데드존
        if (raw.magnitude < mobileDeadZone)
        {
            raw = Vector2.zero;
        }
        else
        {
            float scaledMag = Mathf.InverseLerp(mobileDeadZone, 1f, raw.magnitude);
            raw = raw.normalized * scaledMag;
        }

        // 2. 입력 스무딩
        mobileInput = Vector2.SmoothDamp(
            mobileInput,
            raw,
            ref mobileInputVelocity,
            inputSmoothTime
        );

        float turnInput = mobileInput.x;
        float forwardInput = mobileInput.y;

        // 3. 뒤로 갈 때 회전 감도 약간 감소
        float turnMultiplier = (forwardInput < -0.05f) ? backwardTurnMultiplier : 1f;

        // 4. 작은 입력에서는 천천히, 큰 입력에서는 빠르게 회전
        float curvedTurn = Mathf.Sign(turnInput) * turnInput * turnInput;

        yaw += curvedTurn * turnSpeed * turnMultiplier * Time.deltaTime;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // 5. 후진 속도 약간 감소
        float adjustedForward =
            forwardInput >= 0f
            ? forwardInput * maxForwardSpeedMultiplier
            : forwardInput * maxBackwardSpeedMultiplier;

        float vertical = 0f;
        if (upButton != null && upButton.IsPressed) vertical += 1f;
        if (downButton != null && downButton.IsPressed) vertical -= 1f;

        moveInput = new Vector3(0f, vertical, adjustedForward);
    }

    public void FireUltrasound()
    {
        if (ultrasoundPrefab == null || shootPoint == null) return;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Vector3 shootDir = ray.direction.normalized;

        Instantiate(ultrasoundPrefab, shootPoint.position, Quaternion.LookRotation(shootDir));
        Managers.Sound.Play("Underwater_Impact_02");
    }

    void HandleItemDragging()
    {
        if (dialogueManager != null && dialogueManager.IsDialoguePlaying)
            return;

        if (Input.GetMouseButtonDown(1))
        {
            TryStartDragging();
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (currentDraggedItem != null)
            {
                currentDraggedItem.StopDragging();
                currentDraggedItem = null;
            }
        }
    }

    void TryStartDragging()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, dragDetectionRadius);
        foreach (var hit in hits)
        {
            DraggedItem item = hit.GetComponent<DraggedItem>();
            if (item != null && !item.isDragged)
            {
                item.StartDragging(transform);
                currentDraggedItem = item;
                break;
            }
        }
    }

    // 플레이어 바로 아래에 지면이 가까이 있으면 true
    bool IsBlockedDownward()
    {
        Vector3 origin = rb.position;

        return Physics.SphereCast(
            origin,
            groundProbeRadius,
            Vector3.down,
            out RaycastHit hit,
            groundCheckDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private void FixedUpdate()
    {
        if (rb == null || capsule == null)
        {
            Debug.LogError("No Rigidbody or Capsule collider!");
            return;
        }

        Vector3 currentPos = rb.position;

        Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.forward;
        if (flatRight.sqrMagnitude < 0.0001f) flatRight = Vector3.right;

        bool useMobileInput = Application.isMobilePlatform || forceMobileInput;

        Vector3 horizontalMove;

        // 수평 이동
        //horizontalMove = (flatForward * moveInput.z + flatRight * moveInput.x) * moveSpeed;

        if (useMobileInput)
        {
            // 모바일은 탱크식 유지: 전진/후진만
            horizontalMove = flatForward * moveInput.z * moveSpeed;
        }
        else
        {
            // PC는 기존처럼 좌우 이동 허용
            horizontalMove = (flatForward * moveInput.z + flatRight * moveInput.x) * moveSpeed;
        }

        Vector3 horizontalDelta = horizontalMove * Time.fixedDeltaTime;

        Vector3 posAfterHorizontal = ResolveCapsuleMove(currentPos, horizontalDelta);
        posAfterHorizontal = ResolveOverlaps(posAfterHorizontal);

        // 수직 이동
        float verticalAmount = moveInput.y * verticalSpeed * Time.fixedDeltaTime;
        Vector3 verticalDelta = Vector3.up * verticalAmount;

        Vector3 finalPos = ResolveVerticalMove(posAfterHorizontal, verticalDelta);

        finalPos = ResolveOverlaps(finalPos);

        rb.MovePosition(finalPos);

        /*Vector3 move =
            (transform.forward * moveInput.z + transform.right * moveInput.x) * moveSpeed +
            transform.up * moveInput.y * verticalSpeed;*/

        /*Vector3 move = 
            (transform.forward * moveInput.z + transform.right * moveInput.x) * moveSpeed + 
            Vector3.up * moveInput.y * verticalSpeed;*/

        #region 1st renewal movement
        /*Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
        Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

        Vector3 horizontalMove = (flatForward * moveInput.z + flatRight * moveInput.x) * moveSpeed;
        Vector3 horizontalDelta = horizontalMove * Time.fixedDeltaTime;

        // 수평 이동
        MoveWithSweep(horizontalDelta);

        // 수직 이동
        float vertical = moveInput.y * verticalSpeed;
        Vector3 verticalDelta = Vector3.up * vertical * Time.fixedDeltaTime;

        if (verticalDelta.y < 0f)
        {
            // 아래로 이동하려는 경우, 바닥 가까우면 차단
            if (!IsBlockedDownward())
            {
                MoveWithSweep(verticalDelta);
            }
        }
        else if (verticalDelta.y > 0f)
        {
            MoveWithSweep(verticalDelta);
        }*/
        #endregion

        //rb.MovePosition(rb.position + move * Time.fixedDeltaTime);
    }

    private void MoveWithSweep(Vector3 delta)
    {
        if (delta.sqrMagnitude <= 0.00001f) return;

        if (rb.SweepTest(delta.normalized, out RaycastHit hit, delta.magnitude + 0.02f))
        {
            float safeDistance = Mathf.Max(hit.distance - 0.01f, 0f);
            rb.MovePosition(rb.position + delta.normalized * safeDistance);
        }
        else
        {
            rb.MovePosition(rb.position + delta);
        }
    }

    private Vector3 ResolveCapsuleMove(Vector3 startPos, Vector3 delta)
    {
        if (delta.sqrMagnitude <= 0.000001f) return startPos;

        GetCapsuleWorldPoints(startPos, out Vector3 p1, out Vector3 p2, out float radius);

        Vector3 dir = delta.normalized;
        float dist = delta.magnitude;

        if (Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, dist + skinWidth, ~0, QueryTriggerInteraction.Ignore))
        {
            float safeDist = Mathf.Max(hit.distance - skinWidth, 0f);
            return startPos + dir * safeDist;
        }

        return startPos + delta;
    }

    private Vector3 ResolveVerticalMove(Vector3 startPos, Vector3 delta)
    {
        if (Mathf.Abs(delta.y) <= 0.000001f) return startPos;

        GetCapsuleWorldPoints(startPos, out Vector3 p1, out Vector3 p2, out float radius);

        Vector3 dir = delta.y > 0f ? Vector3.up : Vector3.down;
        float dist = Mathf.Abs(delta.y);

        Debug.Log($"Vertical cast dir={dir}, dist={dist}, mask={groundMask.value}");

        LayerMask maskToUse = verticalBlockMask.value == 0 ? groundMask : verticalBlockMask;

        if (Physics.CapsuleCast(p1, p2, radius, dir, out RaycastHit hit, dist + skinWidth, maskToUse, QueryTriggerInteraction.Ignore))
        {
            Debug.Log($"Vertical hit: {hit.collider.name}, dist={hit.distance}");
            float safeDist = Mathf.Max(hit.distance - skinWidth, 0f);
            return startPos + dir * safeDist;
        }

        return startPos + delta;
    }

    private void GetCapsuleWorldPoints(Vector3 basePosition, out Vector3 p1, out Vector3 p2, out float radius)
    {
        Vector3 scale = transform.lossyScale;

        radius = capsule.radius * Mathf.Max(scale.x, scale.z);
        float height = Mathf.Max(capsule.height * scale.y, radius * 2f);

        Vector3 center = basePosition + transform.TransformDirection(capsule.center);
        float half = height * 0.5f - radius;

        p1 = center + Vector3.up * half;
        p2 = center - Vector3.up * half;
    }

    private Vector3 ResolveOverlaps(Vector3 position)
    {
        GetCapsuleWorldPoints(position, out Vector3 p1, out Vector3 p2, out float radius);

        Collider[] overlaps = Physics.OverlapCapsule(p1, p2, radius, ~0, QueryTriggerInteraction.Ignore);

        Vector3 corrected = position;

        foreach (var col in overlaps)
        {
            if (col == capsule) continue;

            if (Physics.ComputePenetration(
                capsule, corrected, transform.rotation,
                col, col.transform.position, col.transform.rotation,
                out Vector3 direction, out float distance))
            {
                corrected += direction * (distance + skinWidth);
            }
        }

        return corrected;
    }

    public void SetVisible(bool visible)
    {
        if (whaleVisualRoot != null)
            whaleVisualRoot.SetActive(visible);
    }

    #region NPC 대화 앵커 코드
    public void MoveToAnchor(Vector3 pos, Quaternion rot)
    {
        Snap(pos, rot);
    }

    void Snap(Vector3 targetPos, Quaternion targetRot)
    {  
        transform.position = targetPos;
        transform.rotation = targetRot;

        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = (e.x > 180f) ? e.x - 360f : e.x;   // -180°~180° 범위로 정규화
    }
    #endregion
}
