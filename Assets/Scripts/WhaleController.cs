using UnityEngine;

public class WhaleController : MonoBehaviour
{
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

    [Header("Shoot")]
    [SerializeField]
    GameObject ultrasoundPrefab;
    [SerializeField]
    Transform shootPoint;

    [Header("Dragging")]
    [SerializeField]
    float dragDetectionRadius = 10f;




    private DraggedItem currentDraggedItem;
    DialogueManager dialogueManager;


    float yaw = 0f;   // 좌우 회전
    float pitch = 0f; // 상하 기울기

    private void Start()
    {
        //Cursor.visible = false;   // 커서 안보이게

        Cursor.lockState = CursorLockMode.Locked;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {

        if (dialogueManager != null && dialogueManager.IsDialoguePlaying)
        {
            // 대화 중이면 아무 조작도 못하게 막음
            return;
        }

        HandleItemDragging();

        if (Input.GetMouseButtonDown(0) && !(Input.GetMouseButton(1)))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 shootDir = ray.direction.normalized;

            //GameObject wave = Instantiate(ultrasoundPrefab, shootPoint.position, transform.rotation);
            GameObject wave = Instantiate(ultrasoundPrefab, shootPoint.position, Quaternion.LookRotation(shootDir));

            Managers.Sound.Play("Underwater_Impact_02");
        }

        HandleRotation();

        // 이동 입력
        float h = Input.GetAxis("Horizontal"); // A, D
        float v = Input.GetAxis("Vertical");   // W, S

        float y = 0f; // Q, E
        if (Input.GetKey(KeyCode.E)) y = 1f;   // 상승
        if (Input.GetKey(KeyCode.Q)) y = -1f;   // 하강

        // 고래의 방향 기준으로 이동
        //Vector3 moveDir = transform.forward * v + transform.right * h;
        Vector3 velocity = (transform.forward * v + transform.right * h) * moveSpeed + transform.up * y * verticalSpeed;
        transform.position += velocity * Time.deltaTime;
    }

    void HandleRotation()
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

    public void SetVisible(bool visible)
    {
        if (whaleVisualRoot != null)
            whaleVisualRoot.SetActive(visible);
    }
}
