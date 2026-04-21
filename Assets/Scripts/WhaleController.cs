using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Terresquall;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.EnhancedTouch;
#endif


public class WhaleController : MonoBehaviour
{

    private enum MobileControlMode
    {
        Legacy,
        ImprovedLegacy
    }

    private struct MobileTouch
    {
        public Vector2 position;
        public int fingerId;
        public TouchPhase phase;
    }

    private int GetMobileTouchCount()
    {
#if ENABLE_INPUT_SYSTEM
#if !ENABLE_LEGACY_INPUT_MANAGER
    return UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count;
#else
        return Input.touchCount;
#endif
#else
    return Input.touchCount;
#endif
    }

    private MobileTouch GetMobileTouch(int index)
    {
#if ENABLE_INPUT_SYSTEM
#if !ENABLE_LEGACY_INPUT_MANAGER
    var t = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches[index];
    return new MobileTouch
    {
        position = t.screenPosition,
        fingerId = t.finger.index,
        phase = (TouchPhase)Enum.Parse(typeof(TouchPhase), t.phase.ToString())
    };
#else
        var t = Input.GetTouch(index);
        return new MobileTouch
        {
            position = t.position,
            fingerId = t.fingerId,
            phase = t.phase
        };
#endif
#else
    var t = Input.GetTouch(index);
    return new MobileTouch
    {
        position = t.position,
        fingerId = t.fingerId,
        phase = t.phase
    };
#endif
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

    [Header("Mobile Move")]
    [SerializeField] private float mobileDeadZone = 0.18f;
    [SerializeField] private float maxMoveMagnitude = 1f;

    [Header("Mobile Look")]
    [SerializeField] private float lookSensitivityX = 0.15f;
    [SerializeField] private float lookSensitivityY = 0.12f;
    [SerializeField] private bool invertY = false;


    [SerializeField] private float inputSmoothTime = 0.08f;
    [SerializeField] private float backwardTurnMultiplier = 0.75f; // 후진 중 회전 감도 감소
    [SerializeField] private float maxForwardSpeedMultiplier = 1f;
    [SerializeField] private float maxBackwardSpeedMultiplier = 0.75f;

    [Header("Camera")]
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera playerCamera;

    [Header("Slope")]
    [SerializeField] private float hoverHeight = 3f;          // 바닥과 유지할 목표 거리
    [SerializeField] private float hoverAdjustSpeed = 4f;       // 높이 보정 속도
    [SerializeField] private float hoverProbeDistance = 5f;     // 바닥 탐색 거리
    [SerializeField] private float maxHoverCorrection = 0.1f;   // 한 프레임 최대 보정량
    [SerializeField] private float slopeLimit = 45f;            // 오를 수 있는 최대 경사각

    [Header("OptionController")]
    [SerializeField] private OptionUIController optionUIController;


    #region Mobile Vertical Buttons(Legacy)
    /*
    [Header("Mobile Vertical Buttons")]
    [SerializeField] private HoldButton upButton;
    [SerializeField] private HoldButton downButton;
    */
    #endregion

    [Header("Shoot")]
    [SerializeField] GameObject ultrasoundPrefab;
    [SerializeField] Transform shootPoint;

    [Header("Dragging")]
    [SerializeField] float dragDetectionRadius = 10f;

    [Header("Collision")]
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

    private float yaw = 0f;   // 좌우 회전
    private float pitch = 0f; // 상하 기울기

    private Rigidbody rb;
    private Vector3 moveInput;

    [Header("Vertical Collision")]
    [SerializeField] private LayerMask verticalBlockMask;     // 상하 이동 막을 레이어

    // 모바일/에디터 look 입력 추적
    private int lookFingerId = -1;
    private Vector2 lastLookTouchPos;
    private bool isMouseLookDragging = false;
    private Vector2 lastMouseLookPos;

    private Quaternion targetRotation;
    /*private Vector2 mobileInput;
    private Vector2 mobileInputVelocity;*/

    private bool movementLocked = false;
    private bool lookLocked = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        //Cursor.visible = false;   // 커서 안보이게

#if UNITY_EDITOR || UNITY_STANDALONE
        //Cursor.lockState = CursorLockMode.Locked;
#endif

        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = (e.x > 180f) ? e.x - 360f : e.x;   // -180~180 보정

        // pitch 제한 적용
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        dialogueManager = FindObjectOfType<DialogueManager>();

        if (verticalBlockMask.value == 0)
            verticalBlockMask = groundMask;

        targetRotation = Quaternion.Euler(0f, yaw, 0f);
        rb.rotation = targetRotation;
        transform.rotation = targetRotation;

        if (optionUIController == null)
            optionUIController = FindAnyObjectByType<OptionUIController>();

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    void Update()
    {
        bool dialogueLocked = dialogueManager != null && dialogueManager.IsDialoguePlaying;

        if (isFrozen || dialogueLocked)
        {
            // 대화 중이면 아무 조작도 못하게 막음
            moveInput = Vector3.zero;
            ClearLookState();
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
            // HandleMobileRotationAndMove();
            if (!movementLocked) 
                HandleMobileMove();
            else 
                moveInput = Vector3.zero;

            if (!lookLocked) 
                HandleMobileLook();
            else 
                ClearLookState();
        }
        else
        {
            if (!lookLocked) 
                HandleRotationPC();

            if (!movementLocked) 
                HandleMovePC();
            else 
                moveInput = Vector3.zero;
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
        //transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        targetRotation = Quaternion.Euler(0, yaw, 0f);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }

    #region Mobile Movement(legacy)
    void HandleMobileRotationAndMove()
    {
        float rawX = VirtualJoystick.GetAxis("Horizontal");
        float rawY = VirtualJoystick.GetAxis("Vertical");

        if (mobileControlMode == MobileControlMode.Legacy)
        {
            float verticalMove = 0f;
            // if (upButton != null && upButton.IsPressed) verticalMove += 1f;
            // if (downButton != null && downButton.IsPressed) verticalMove -= 1f;

            yaw += rawX * turnSpeed * Time.deltaTime;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

            moveInput = new Vector3(0f, verticalMove, rawY);
            return;
        }

        /*// ImprovedLegacy
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

        // X만 살짝 스무딩
        mobileInput.x = Mathf.SmoothDamp(
            mobileInput.x,
            raw.x,
            ref mobileInputVelocity.x,
            inputSmoothTime
        );

        mobileInput.y = raw.y;

        float absX = Mathf.Abs(raw.x);
        float absY = Mathf.Abs(raw.y);

        float turnInput = 0f;
        float forwardInput = 0f;

        const float axisBias = 0.08f;
        *//*
                // 전진/후진 입력이 강할수록 회전을 줄임
                float forwardAbs = Mathf.Abs(forwardInput);

                // forwardAbs가 커질수록 turnWeight가 작아짐
                // 전진 중 회전 100% 허용이 아니라 20~30% 정도만 남김
                float turnWeight = Mathf.Lerp(1f, 0.15f, forwardAbs);

                // 조이스틱을 거의 위/아래로 밀었을 땐 잔여 X 제거
                *//*if (Mathf.Abs(rawX) < 0.08f && forwardAbs > 0.5f)
                    turnInput = 0f;*//*

                // 거의 위/아래로 밀고 있으면 좌우 잔류 제거
                if (Mathf.Abs(raw.x) < 0.08f && forwardAbs > 0.45f)
                    turnInput = 0f;*//*

        if (absX > absY + axisBias)
        {
            // 회전 우선
            turnInput = mobileInput.x;
            forwardInput = 0f;
        }
        else if (absY > absX + axisBias)
        {
            // 이동 우선
            turnInput = 0f;
            forwardInput = mobileInput.y;
        }
        else
        {
            // 애매한 대각선 구간: 직전 의도 유지 대신
            // 여기서는 이동 우선으로 두는 편이 체감상 더 안정적
            turnInput = 0f;
            forwardInput = mobileInput.y;
        }


        // 3. 뒤로 갈 때 회전 감도 약간 감소
        float turnMultiplier = (forwardInput < -0.05f) ? backwardTurnMultiplier : 1f;

        // 4. 작은 입력에서는 천천히, 큰 입력에서는 빠르게 회전
        float curvedTurn = Mathf.Sign(turnInput) * turnInput * turnInput;

        //yaw += curvedTurn * turnSpeed * turnMultiplier * Time.deltaTime;
        // yaw += curvedTurn * turnSpeed * turnMultiplier * turnWeight * Time.deltaTime;
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

        moveInput = new Vector3(0f, vertical, adjustedForward);*/

        // 축별 데드존: X/Y를 따로 처리
        if (Mathf.Abs(rawX) < mobileDeadZone) rawX = 0f;
        if (Mathf.Abs(rawY) < mobileDeadZone) rawY = 0f;

        // X는 회전 전용
        yaw += rawX * turnSpeed * Time.deltaTime;

        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // Y는 무조건 전진/후진
        float adjustedForward =
            rawY >= 0f
            ? rawY * maxForwardSpeedMultiplier
            : rawY * maxBackwardSpeedMultiplier;

        float vertical = 0f;
        //if (upButton != null && upButton.IsPressed) vertical += 1f;
        //if (downButton != null && downButton.IsPressed) vertical -= 1f;

        moveInput = new Vector3(0f, vertical, adjustedForward);
    }

    #endregion

    private void HandleMobileMove()
    {
        float rawX = VirtualJoystick.GetAxis("Horizontal");
        float rawY = VirtualJoystick.GetAxis("Vertical");

        if (Mathf.Abs(rawX) < mobileDeadZone) rawX = 0f;
        if (Mathf.Abs(rawY) < mobileDeadZone) rawY = 0f;

        Vector2 raw = new Vector2(rawX, rawY);
        raw = Vector2.ClampMagnitude(raw, maxMoveMagnitude);

        // 모바일에서는 수직 이동 제거
        moveInput = new Vector3(raw.x, 0f, raw.y);
    }

    private void HandleMobileLook()
    {
        if (optionUIController != null && optionUIController.IsOptionOpen)
        {
            ClearLookState();
            return;
        }
        int touchCount = GetMobileTouchCount();

        // 모바일이거나, 에디터 테스트 중인데 실제 터치가 들어오면 터치 회전 사용
        if (touchCount > 0 || Application.isMobilePlatform || forceMobileInput)
        {
            isMouseLookDragging = false;
            HandleTouchLook();
        }
       /* else if (forceMobileInput)
        {
            HandleMouseLookForEditorTest();
        }*/
    }

    private void HandleTouchLook()
    {
        int touchCount = GetMobileTouchCount();

        // 이미 회전용으로 잡은 손가락이 있으면 그 손가락만 추적
        if (lookFingerId != -1)
        {
            for (int i = 0; i < touchCount; i++)
            {
                var touch = GetMobileTouch(i);

                if (touch.fingerId != lookFingerId)
                    continue;

                if (touch.phase == TouchPhase.Moved)
                {
                    Vector2 delta = touch.position - lastLookTouchPos;
                    lastLookTouchPos = touch.position;
                    ApplyLookDelta(delta);
                }
                else if (touch.phase == TouchPhase.Stationary)
                {
                    // stationary는 회전 적용 X
                    lastLookTouchPos = touch.position;
                }
                else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                {
                    Debug.Log($"[LOOK END BEFORE] yaw={yaw}, pitch={pitch}, rot={transform.eulerAngles}");

                    lookFingerId = -1;

                    //transform.rotation = Quaternion.Euler(0, yaw, 0f);
                    Debug.Log($"[LOOK END AFTER] yaw={yaw}, pitch={pitch}, rot={transform.eulerAngles}");
                }

                return;
            }

            lookFingerId = -1;
            return;

        }

        // 회전 손가락이 없을 때만 새로 잡음
        // 반드시 "오른쪽 절반에서 새로 Began한 터치"만 회전용으로 인정
        for (int i = 0; i < touchCount; i++)
        {
            var touch = GetMobileTouch(i);

            if (touch.phase != TouchPhase.Began)
                continue;

            if (touch.position.x < Screen.width * 0.5f)
                continue;

            if (EventSystem.current != null &&
                EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                continue;

            lookFingerId = touch.fingerId;
            lastLookTouchPos = touch.position;
            return;
        }
    }

    private void HandleMouseLookForEditorTest()
    {
        // forceMobileInput일 때 에디터에서 오른쪽 절반 마우스 드래그로 회전 테스트 가능
        if (Input.GetMouseButtonDown(0))
        {
            DebugUIRaycastHits();
            Debug.Log($"MouseDown pos={Input.mousePosition}, overUI={IsPointerOverUI()}");

            if (Input.mousePosition.x >= Screen.width * 0.5f && !IsPointerOverUI())
            {
                isMouseLookDragging = true;
                lastMouseLookPos = Input.mousePosition;
            }
        }

        if (Input.GetMouseButton(0) && isMouseLookDragging)
        {
            Vector2 currentMousePos = Input.mousePosition;
            Vector2 delta = currentMousePos - lastMouseLookPos;
            lastMouseLookPos = currentMousePos;

            ApplyLookDelta(delta);
        }

        if (Input.GetMouseButtonUp(0))
        {
            isMouseLookDragging = false;
        }
    }

    private void ApplyLookDelta(Vector2 delta)
    {
        float ySign = invertY ? 1f : -1f;

        yaw += delta.x * lookSensitivityX;
        pitch += delta.y * lookSensitivityY * ySign;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        targetRotation = Quaternion.Euler(0f, yaw, 0f);

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        //transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
        //targetRotation = Quaternion.Euler(pitch, yaw, 0f);
        //Debug.Log($"[LOOK MOVE] delta={delta}, yaw={yaw}, pitch={pitch}, rot={transform.eulerAngles}");
    }

    private void ClearLookState()
    {
        lookFingerId = -1;
        isMouseLookDragging = false;

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

        if (movementLocked)
        {
            moveInput = Vector3.zero;
        }

        #region Movememt(legacy)
        /*
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
        */
        #endregion

        #region (수평이동, 전진/후진 + 터치 화면 회전)
        /*
                Vector3 flatForward = Vector3.ProjectOnPlane(transform.forward, Vector3.up).normalized;
                Vector3 flatRight = Vector3.ProjectOnPlane(transform.right, Vector3.up).normalized;

                if (flatForward.sqrMagnitude < 0.0001f) flatForward = Vector3.forward;
                if (flatRight.sqrMagnitude < 0.0001f) flatRight = Vector3.right;

                // 모바일/PC 공통: 좌우 + 전후 이동 모두 허용
                Vector3 horizontalMove = (flatForward * moveInput.z + flatRight * moveInput.x) * moveSpeed;
                Vector3 horizontalDelta = horizontalMove * Time.fixedDeltaTime;

                Vector3 posAfterHorizontal = ResolveCapsuleMove(currentPos, horizontalDelta);
                posAfterHorizontal = ResolveOverlaps(posAfterHorizontal);

                // 모바일에서는 moveInput.y = 0, PC에서는 Q/E 사용 가능
                float verticalAmount = moveInput.y * verticalSpeed * Time.fixedDeltaTime;
                Vector3 verticalDelta = Vector3.up * verticalAmount;

                Vector3 finalPos = ResolveVerticalMove(posAfterHorizontal, verticalDelta);
                finalPos = ResolveOverlaps(finalPos);

                rb.MoveRotation(targetRotation);
                rb.MovePosition(finalPos);*/

        #endregion

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

        #region 경사 반영

        // 1. 경사 반영 이동 방향
        Vector3 moveDir = GetSlopeProjectedMove();
        Vector3 horizontalDelta = moveDir * moveSpeed * Time.fixedDeltaTime;

        // 2. 수평 이동
        Vector3 posAfterHorizontal = ResolveCapsuleMove(currentPos, horizontalDelta);
        posAfterHorizontal = ResolveOverlaps(posAfterHorizontal);

        // 3. 플레이어 수동 상하 이동(PC의 Q/E 같은 것)
        float verticalAmount = moveInput.y * verticalSpeed * Time.fixedDeltaTime;
        Vector3 verticalDelta = Vector3.up * verticalAmount;

        Vector3 posAfterVertical = ResolveVerticalMove(posAfterHorizontal, verticalDelta);
        posAfterVertical = ResolveOverlaps(posAfterVertical);

        // 4. 바닥과 일정 거리 유지
        Vector3 finalPos = ApplyHoverHeight(posAfterVertical);
        finalPos = ResolveOverlaps(finalPos);

        rb.MoveRotation(targetRotation);
        rb.MovePosition(finalPos);

        #endregion
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

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null) return false;

        if (Application.isMobilePlatform && Input.touchCount > 0)
            return EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId);

        return EventSystem.current.IsPointerOverGameObject();
    }

    public void SetVisible(bool visible)
    {
        if (whaleVisualRoot != null)
            whaleVisualRoot.SetActive(visible);
    }

    public void SetMovementLocked(bool locked)
    {
        movementLocked = locked;

        if (locked)
            moveInput = Vector3.zero;
    }

    public void SetLookLocked(bool locked)
    {
        lookLocked = locked;

        if (locked)
            ClearLookState();
    }

    public void SetControlLocked(bool movement, bool look)
    {
        SetMovementLocked(movement);
        SetLookLocked(look);
    }

    public void SetFrozen(bool frozen)
    {
        isFrozen = frozen;

        if (frozen)
        {
            moveInput = Vector3.zero;
            ClearLookState();
        }
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
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        targetRotation = Quaternion.Euler(0f, yaw, 0f);
        transform.rotation = targetRotation;
        rb.rotation = targetRotation;

        if (cameraPivot != null)
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
    }
    #endregion


    private void DebugUIRaycastHits()
    {
        if (EventSystem.current == null)
        {
            Debug.Log("[UI Raycast] EventSystem.current == null");
            return;
        }

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        Debug.Log($"[UI Raycast] mouse={Input.mousePosition}, hitCount={results.Count}");

        for (int i = 0; i < results.Count; i++)
        {
            GameObject go = results[i].gameObject;

            Image image = go.GetComponent<Image>();
            Button button = go.GetComponent<Button>();
            ScrollRect scrollRect = go.GetComponent<ScrollRect>();
            Scrollbar scrollbar = go.GetComponent<Scrollbar>();
            Slider slider = go.GetComponent<Slider>();
            TMP_Text tmpText = go.GetComponent<TMP_Text>();
            CanvasGroup canvasGroup = go.GetComponent<CanvasGroup>();

            string path = GetHierarchyPath(go.transform);

            Debug.Log(
                $"[UI Raycast] #{i} " +
                $"name={go.name}, path={path}, layer={LayerMask.LayerToName(go.layer)}, " +
                $"Image={(image != null ? $"Y(rayTarget={image.raycastTarget})" : "N")}, " +
                $"Button={(button != null ? "Y" : "N")}, " +
                $"ScrollRect={(scrollRect != null ? "Y" : "N")}, " +
                $"Scrollbar={(scrollbar != null ? "Y" : "N")}, " +
                $"Slider={(slider != null ? "Y" : "N")}, " +
                $"TMP_Text={(tmpText != null ? "Y" : "N")}, " +
                $"CanvasGroup={(canvasGroup != null ? $"Y(blocksRaycasts={canvasGroup.blocksRaycasts})" : "N")}"
            );
        }
    }

    private string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
    #region 경사면 관련 코드

    private bool TryGetGround(Vector3 basePosition, out RaycastHit groundHit)
    {
        Vector3 origin = basePosition + Vector3.up * 0.1f;

        return Physics.SphereCast(
            origin,
            groundProbeRadius,
            Vector3.down,
            out groundHit,
            hoverProbeDistance,
            groundMask,
            QueryTriggerInteraction.Ignore
        );
    }

    private Vector3 GetSlopeProjectedMove()
    {
        // pitch가 들어간 transform.forward/right 대신
        // yaw만 사용하는 수평 기준 방향 생성
        Quaternion yawRotation = Quaternion.Euler(0f, yaw, 0f);

        Vector3 forward = yawRotation * Vector3.forward;
        Vector3 right = yawRotation * Vector3.right;

        Vector3 inputDir = forward * moveInput.z + right * moveInput.x;

        if (inputDir.sqrMagnitude > 1f)
            inputDir.Normalize();

        if (TryGetGround(rb.position, out RaycastHit groundHit))
        {
            float slopeAngle = Vector3.Angle(groundHit.normal, Vector3.up);

            if (slopeAngle <= slopeLimit)
            {
                Vector3 projected = Vector3.ProjectOnPlane(inputDir, groundHit.normal);
                if (projected.sqrMagnitude > 0.0001f)
                    return projected.normalized;
            }
        }

        return inputDir;
    }

    private Vector3 ApplyHoverHeight(Vector3 position)
    {
        if (!TryGetGround(position, out RaycastHit groundHit))
            return position;

        float currentGroundDistance = groundHit.distance;
        float error = hoverHeight - currentGroundDistance;

        if (Mathf.Abs(error) < 0.02f)
            return position;

        // error > 0  : 바닥이 너무 가까움 -> 위로 올림
        // error < 0  : 바닥이 너무 멂   -> 아래로 내림

        float correction = error * hoverAdjustSpeed * Time.fixedDeltaTime;
        correction = Mathf.Clamp(correction, -maxHoverCorrection, maxHoverCorrection);

        return position + Vector3.up * correction;
    }
    #endregion
}
