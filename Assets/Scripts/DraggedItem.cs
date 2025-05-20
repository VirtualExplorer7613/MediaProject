using UnityEngine;

public class DraggedItem : MonoBehaviour
{
    public bool isDragged = false;

    private Transform target;

    [Header("Follow-When-Dragged")]
    [SerializeField]
    private float followSpeed = 3f;
    [SerializeField]
    private Vector3 offset = new Vector3(0, 1f, 2f);
    [SerializeField] float minDistToWhale = 2f;




    private Rigidbody rb;
    private ConstantForce constForce;

    [Tooltip("����Ʈ ������ ���� �Ÿ�")]
    [SerializeField] private float questClearDistance = 5f;
    [SerializeField] float releaseEnableGravity = 1.5f; // ���� ��ġ���� �� ��ŭ ����� �߷� ON
    [SerializeField] float destroyDistance = 10f;  // �� �Ÿ� ������ �ı�

    Vector3 startPos;          // ���� ��ġ
    bool gravityEnabled = false;
    private bool hasLeftQuestArea = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        constForce = GetComponent<ConstantForce>();

        rb.isKinematic = true;
        rb.useGravity = false;
        constForce.force = Vector3.zero;

        startPos = transform.position;
    }

    void Update()
    {
        if (isDragged && target != null)
        {
            FollowWhale();
            TryRemoveFromTalkableNPC();
            return;
        }

        // ���������������������������������� �巡�� ���� �ƴ� �� ����������������������������������
        // �̹� �߷��� �����ٸ� �׳� ������ �ñ��
        if (gravityEnabled)
        {
            // �ָ� ���� �ڵ� �ı�
            if (Vector3.Distance(startPos, transform.position) >= destroyDistance)
            {
                Managers.Sound.Play("Bubble_Surface_Large_01");
                Destroy(gameObject);
            }
                

            return;
        }

        // ���� ���� ���� �� ���� �Ÿ� �̻� �������� ���� Ȱ��ȭ
        float distFromStart = Vector3.Distance(startPos, transform.position);
        if (distFromStart >= releaseEnableGravity)
            EnableGravity();
    }

    /* --------------------------- Drag ���� --------------------------- */
    public void StartDragging(Transform targetTransform)
    {
        isDragged = true;
        target = targetTransform;

        // �巡�״� ���� ��ġ�� �����ϹǷ�, �ٽ� kinematic ����
        rb.isKinematic = true;
        rb.useGravity = false;
        constForce.force = Vector3.zero;
    }

    public void StopDragging()
    {
        isDragged = false;
        target = null;

        // ������ ���� �̹� ����� �������� ��ٷ� �߷� ����
        if (Vector3.Distance(startPos, transform.position) >= releaseEnableGravity)
            EnableGravity();
    }

    /* --------------------------- ���� ��� --------------------------- */
    void FollowWhale()
    {
        Vector3 targetPos = target.position + target.TransformDirection(offset);
        float dist = Vector3.Distance(transform.position, target.position);

        if (dist > minDistToWhale)
        {
            transform.position =
                Vector3.Lerp(transform.position, targetPos, Time.deltaTime * followSpeed);
        }
        else
        {
            Vector3 pushDir = (transform.position - target.position).normalized;
            transform.position = target.position + pushDir * minDistToWhale;
        }
    }

    void EnableGravity()
    {
        gravityEnabled = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        constForce.force = new Vector3(0, -1.1f, 0); // �� �Ʒ��� ������� �ʹٸ� ���
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

/*    public void StartDragging(Transform targetTransform)
    {
        isDragged = true;
        target = targetTransform;
    }

    public void StopDragging()
    {
        isDragged = false;
        target = null;
    }*/
}
