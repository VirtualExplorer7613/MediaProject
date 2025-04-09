using UnityEngine;

public class WhaleController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField]
    float moveSpeed = 10f;
    [SerializeField]
    float mouseSensitivity = 100f;

    [SerializeField]
    GameObject ultrasoundPrefab;
    [SerializeField]
    Transform shootPoint;

    [Header("Dragging")]
    [SerializeField]
    float dragDetectionRadius = 10f;

    private DraggedItem currentDraggedItem;


    float yaw = 0f;   // �¿� ȸ��
    float pitch = 0f; // ���� ����

    private void Start()
    {
        //Cursor.visible = false;   // Ŀ�� �Ⱥ��̰�

        Cursor.lockState = CursorLockMode.Locked;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void Update()
    {
        HandleItemDragging();
        if (Input.GetMouseButtonDown(0) && !(Input.GetMouseButton(1)))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 shootDir = ray.direction.normalized;

            //GameObject wave = Instantiate(ultrasoundPrefab, shootPoint.position, transform.rotation);
            GameObject wave = Instantiate(ultrasoundPrefab, shootPoint.position, Quaternion.LookRotation(shootDir));

        }
        // ���콺 �Է�
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, -60f, 60f); // �ʹ� ���Ʒ��� ���� �ʵ��� ����

        // �� ȸ�� ����
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        // �̵� �Է�
        float h = Input.GetAxis("Horizontal"); // A, D
        float v = Input.GetAxis("Vertical");   // W, S

        // ���� ���� �������� �̵�
        Vector3 moveDir = transform.forward * v + transform.right * h;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }


    void HandleItemDragging()
    {
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
}
