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

            // 너무 가까우면 위치 이동하지 않음 (몸통 안 들어가게 막음)
            if (distance > minDistanceToWhale)
            {
                transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
            }
            else
            {
                // 고래와 겹치지 않게 약간 밀어냄 (방향 유지)
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
