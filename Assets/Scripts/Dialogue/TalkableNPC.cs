using UnityEngine;

public class TalkableNPC : MonoBehaviour
{
    public string characterName; // 이 NPC가 어떤 캐릭터인지 (ex: tuna, whale)
    public float interactionDistance = 3f; // 상호작용 가능한 거리
    public KeyCode interactionKey = KeyCode.E; // 대화 시작 키

    private Transform player;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= interactionDistance)
        {
            // 플레이어가 가까이 있을 때
            if (Input.GetKeyDown(interactionKey))
            {
                StartDialogue();
            }
        }
    }

    private void StartDialogue()
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(characterName);
        }
        else
        {
            Debug.LogError("DialogueManager를 찾을 수 없습니다!");
        }
    }
}
