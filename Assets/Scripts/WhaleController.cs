using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhaleController : MonoBehaviour
{
    [SerializeField] private GameObject whaleVisualRoot;

    [Header("view")]
    [SerializeField]
    private Texture2D crosshair;
    [SerializeField]
    [Tooltip("߾ ���� ����(-90 ~ 0)")]
    [Range(-90f, 0f)] private float minPitch = -60f;
    [SerializeField]
    [Tooltip("�� �þ߰� ����(0 ~ 90)")]
    [Range(0f, 90f)] private float maxPitch = 20f;

    [Header("Movement")]
    [SerializeField]
    float moveSpeed = 10f;
    [SerializeField] 
    float verticalSpeed = 10f;   // ���� �̵� �ӵ�
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

    // Dialogue snap
    bool isFrozen = false;
    Coroutine snapCo;

    private DraggedItem currentDraggedItem;
    DialogueManager dialogueManager;


    float yaw = 0f;   // �¿� ȸ��
    float pitch = 0f; // ���� ����

    private void Start()
    {
        //Cursor.visible = false;   // Ŀ�� �Ⱥ��̰�

        Cursor.lockState = CursorLockMode.Locked;
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        dialogueManager = FindObjectOfType<DialogueManager>();
    }

    void Update()
    {

        if (isFrozen || dialogueManager != null && dialogueManager.IsDialoguePlaying)
        {
            // ��ȭ ���̸� �ƹ� ���۵� ���ϰ� ����
            return;
        }

        HandleItemDragging();

        if (Input.GetKeyDown(KeyCode.F))
        {
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            Vector3 shootDir = ray.direction.normalized;

            //GameObject wave = Instantiate(ultrasoundPrefab, shootPoint.position, transform.rotation);
            GameObject wave = Instantiate(ultrasoundPrefab, shootPoint.position, Quaternion.LookRotation(shootDir));

            Managers.Sound.Play("Underwater_Impact_02");
        }

        HandleRotation();

        // �̵� �Է�
        float h = Input.GetAxis("Horizontal"); // A, D
        float v = Input.GetAxis("Vertical");   // W, S

        float y = 0f; // Q, E
        if (Input.GetKey(KeyCode.E)) y = 1f;   // ���
        if (Input.GetKey(KeyCode.Q)) y = -1f;   // �ϰ�

        // ���� ���� �������� �̵�
        //Vector3 moveDir = transform.forward * v + transform.right * h;
        Vector3 velocity = (transform.forward * v + transform.right * h) * moveSpeed + transform.up * y * verticalSpeed;
        transform.position += velocity * Time.deltaTime;
    }

    void HandleRotation()
    {
        if (dialogueManager != null && dialogueManager.IsDialoguePlaying)
            return;

        // ���콺 �Է�
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        yaw += mouseX;
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch); // �ʹ� ���Ʒ��� ���� �ʵ��� ����

        // �� ȸ�� ����
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
            if (item != null && !item.isDragged && !item.isDragged)
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

    #region NPC ��ȭ ��Ŀ �ڵ�
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
        pitch = (e.x > 180f) ? e.x - 360f : e.x;   // -180��~180�� ������ ����ȭ
    }
    #endregion
}
