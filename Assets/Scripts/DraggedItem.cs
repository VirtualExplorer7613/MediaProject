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

    [Tooltip("퀘스트 조건을 위한 거리")]
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

            //TalkableNPC 기준으로 거리 계산 및 제거 처리
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
                Debug.Log("쓰레기 드래그 완료 → Trash로 전환");
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
