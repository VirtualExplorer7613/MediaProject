using UnityEngine;

public class Ultrasound : MonoBehaviour
{
    [SerializeField]
    float moveSpeed = 10f;
    [SerializeField]
    float scaleSpeed = 1f;
    [SerializeField]
    float maxDistance = 20f;

    Vector3 startPos;

    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;
    private bool hasReportedTrashClean = false;

    [SerializeField] private GameObject trashBreakVfxPrefab;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float vfxDestroyDelay = 1.5f;

    public void SetBroadcaster(ItemUseBroadcaster Broadcaster)
    {
        itemUseBroadcaster = Broadcaster;
    }

    private void Start()
    {
        startPos = transform.position;
    }

    private void Awake()
    {
        itemUseBroadcaster = FindAnyObjectByType<ItemUseBroadcaster>();
    }

    private void Update()
    {
        // 전진
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // 커짐
        float scale = transform.localScale.x + scaleSpeed * Time.deltaTime;
        transform.localScale = new Vector3(scale, scale, 0);



        // 거리 초과 시 제거
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
        {
            Destroy(transform.root.gameObject);
            //Destroy(gameObject);
        }
            
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        // 해머 전용 쓰레기는 초음파 무시
        if (other.GetComponent<HammerableTrash>() != null)
            return;

        // TalkableNPC npc = DialogueManager.Instance?.CurrentTalkingNPC;

        /* // 퀘스트 진행 중인 NPC가 있고, 해당 오브젝트가 퀘스트 대상인 경우에만 처리
         if (npc != null && npc.requiredObjects.Contains(other.gameObject)&& other.CompareTag("Trash"))
         {
             Debug.Log("퀘스트 대상 쓰레기 파괴됨!");
             Destroy(other.gameObject);
             Managers.Sound.Play("Bubble_Surface_Large_01");
             InteractionSystem.Instance.NotifyInteractionComplete();
             return;
         }*/

        QuestTarget questTarget = other.GetComponent<QuestTarget>();

        if (questTarget != null)
        {
            // 초음파로 처리 가능한 상태가 아니면 무시
            if (!questTarget.CanBeProcessedBy(QuestItemType.Ultrasound))
                return;

            Debug.Log("퀘스트 대상 쓰레기 파괴됨!");

            questTarget.CompleteQuestTarget();
            SpawnTrashBreakVfx(other.bounds.center);

            TrashVfxUtility.SpawnOneShot(trashBreakVfxPrefab, other.transform.position, vfxOffset, vfxDestroyDelay);
            Managers.Sound.Play("Bubble_Surface_Large_01");
            InteractionSystem.Instance.NotifyInteractionComplete();

            Destroy(other.gameObject);
            return;
        }

        if (other.CompareTag("Trash"))
        {
            SpawnTrashBreakVfx(other.bounds.center);
            TrashVfxUtility.SpawnOneShot(trashBreakVfxPrefab, other.transform.position, vfxOffset, vfxDestroyDelay);
            Debug.Log("일반 쓰레기 파괴됨!");

            TrashManager.Instance?.DecreaseTrash();
            Destroy(other.gameObject);

            Managers.Sound.Play("Bubble_Surface_Large_01");
            itemUseBroadcaster?.RaiseItemUsed("wave");

            if (!hasReportedTrashClean)
            {
                hasReportedTrashClean = true;
                itemUseBroadcaster?.RaiseItemUsed("wave");
            }
        }
    }

    private void SpawnTrashBreakVfx(Vector3 position)
    {
        TrashVfxUtility.SpawnOneShot(
            trashBreakVfxPrefab,
            position,
            vfxOffset,
            vfxDestroyDelay
        );
    }
}
