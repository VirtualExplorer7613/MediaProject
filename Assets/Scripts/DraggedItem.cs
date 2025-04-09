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

    void Update()
    {
        if (isDragged && target != null)
        {
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
