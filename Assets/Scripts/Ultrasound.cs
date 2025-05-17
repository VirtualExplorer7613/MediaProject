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
        // ����
        transform.Translate(Vector3.forward * moveSpeed * Time.deltaTime);

        // Ŀ��
        float scale = transform.localScale.x + scaleSpeed * Time.deltaTime;
        transform.localScale = new Vector3(scale, scale, 0);



        // �Ÿ� �ʰ� �� ����
        if (Vector3.Distance(startPos, transform.position) >= maxDistance)
            Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        TalkableNPC npc = DialogueManager.Instance?.CurrentTalkingNPC;

        // ����Ʈ ���� ���� NPC�� �ְ�, �ش� ������Ʈ�� ����Ʈ ����� ��쿡�� ó��
        if (npc != null && npc.requiredObjects.Contains(other.gameObject))
        {
            Debug.Log("����Ʈ ��� ������ �ı���!");
            Destroy(other.gameObject);
            InteractionSystem.Instance.NotifyInteractionComplete();
            return;
        }

        if (other.CompareTag("Trash"))
        {
            Debug.Log("�Ϲ� ������ �ı���!");
            TrashManager.Instance?.DecreaseTrash();
            Destroy(other.gameObject);
        }
    }
}
