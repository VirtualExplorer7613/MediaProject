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

    [Tooltip("퀘스트 조건을 위한 거리")]
    [SerializeField] private float questClearDistance = 5f;
    [SerializeField] float releaseEnableGravity = 1.5f; // 시작 위치에서 이 만큼 벗어나면 중력 ON
    [SerializeField] float destroyDistance = 10f;  // 이 거리 넘으면 파괴

    Vector3 startPos;          // 최초 위치
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

        // ───────────────── 드래그 중이 아닐 때 ─────────────────
        // 이미 중력이 켜졌다면 그냥 물리에 맡긴다
        if (gravityEnabled)
        {
            // 멀리 가면 자동 파괴
            if (Vector3.Distance(startPos, transform.position) >= destroyDistance)
            {
                Managers.Sound.Play("Bubble_Surface_Large_01");
                Destroy(gameObject);
            }
                

            return;
        }

        // 아직 고정 상태 → 일정 거리 이상 뽑혔으면 물리 활성화
        float distFromStart = Vector3.Distance(startPos, transform.position);
        if (distFromStart >= releaseEnableGravity)
            EnableGravity();
    }

    /* --------------------------- Drag 제어 --------------------------- */
    public void StartDragging(Transform targetTransform)
    {
        isDragged = true;
        target = targetTransform;

        // 드래그는 직접 위치를 조정하므로, 다시 kinematic 으로
        rb.isKinematic = true;
        rb.useGravity = false;
        constForce.force = Vector3.zero;
    }

    public void StopDragging()
    {
        isDragged = false;
        target = null;

        // 놓아준 순간 이미 충분히 뽑혔으면 곧바로 중력 적용
        if (Vector3.Distance(startPos, transform.position) >= releaseEnableGravity)
            EnableGravity();
    }

    /* --------------------------- 내부 기능 --------------------------- */
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
        constForce.force = new Vector3(0, -1.1f, 0); // 물 아래로 끌어내리고 싶다면 사용
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
