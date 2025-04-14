using UnityEngine;

public class DraggedItem : MonoBehaviour
{
    public bool isDragged = false;

    private Transform target;
    [SerializeField]
    private float followSpeed = 3f;
    [SerializeField]
    private Vector3 offset = new Vector3(0, 1f, 2f);
    [SerializeField]
    private float minDistanceToWhale = 2f;


    private Rigidbody rb;
    private ConstantForce constForce;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        constForce = GetComponent<ConstantForce>();
    }

    void Update()
    {
        if (isDragged && target != null)
        {
            rb.useGravity = false;
            constForce.force = new Vector3(0, 0, 0);

            Vector3 targetPos = target.position + target.TransformDirection(offset);
            float distance = Vector3.Distance(transform.position, target.position);

            // �ʹ� ������ ��ġ �̵����� ���� (���� �� ���� ����)
            if (distance > minDistanceToWhale)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            }
            else
            {
                // ���� ��ġ�� �ʰ� �ణ �о (���� ����)
                Vector3 pushDir = (transform.position - target.position).normalized;
                transform.position = target.position + pushDir * minDistanceToWhale;
            }
        }
        else
        {
            rb.useGravity = true;
            constForce.force = new Vector3(0, -1.1f, 0);
        }
    }

    public void StartDragging(Transform targetTransform)
    {
        isDragged = true;
        target = targetTransform;
    }

    public void StopDragging()
    {
        isDragged = false;
        target = null;
    }
}
