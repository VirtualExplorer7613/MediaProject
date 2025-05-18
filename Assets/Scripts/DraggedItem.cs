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

    [Tooltip("����Ʈ ������ ���� �Ÿ�")]
    [SerializeField] private float questClearDistance = 5f;
    private bool hasLeftQuestArea = false;

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

            //TalkableNPC �������� �Ÿ� ��� �� ���� ó��
            TryRemoveFromTalkableNPC();
        }
        else
        {
            //rb.useGravity = true;
            constForce.force = new Vector3(0, 0f, 0);
        }
    }

    private void TryRemoveFromTalkableNPC()
    {
        if (hasLeftQuestArea) return;

        TalkableNPC npc = DialogueManager.Instance?.CurrentTalkingNPC;
        if (npc == null) return;

        if (npc.requiredObjects.Contains(gameObject))
        {
            float distToNPC = Vector3.Distance(transform.position, npc.transform.position);
            if (distToNPC >= questClearDistance)
            {
                Debug.Log("������ �巡�� �Ϸ� �� Trash�� ��ȯ");
                gameObject.tag = "Trash";
                npc.requiredObjects.Remove(gameObject);
                npc.CheckQuestCondition();
                hasLeftQuestArea = true;
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
