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

    private void Start()
    {
        startPos = transform.position;
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
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        TalkableNPC npc = DialogueManager.Instance?.CurrentTalkingNPC;

        // 퀘스트 진행 중인 NPC가 있고, 해당 오브젝트가 퀘스트 대상인 경우에만 처리
        if (npc != null && npc.requiredObjects.Contains(other.gameObject))
        {
            Debug.Log("퀘스트 대상 쓰레기 파괴됨!");
            Destroy(other.gameObject);
            InteractionSystem.Instance.NotifyInteractionComplete();
            return;
        }

        if (other.CompareTag("Trash"))
        {
            Debug.Log("일반 쓰레기 파괴됨!");
            TrashManager.Instance?.DecreaseTrash();
            Destroy(other.gameObject);
        }
    }
}
