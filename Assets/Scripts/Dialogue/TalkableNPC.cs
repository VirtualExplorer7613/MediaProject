using UnityEngine;

public class TalkableNPC : MonoBehaviour
{
    public string characterName; // �� NPC�� � ĳ�������� (ex: tuna, whale)
    public float interactionDistance = 3f; // ��ȣ�ۿ� ������ �Ÿ�
    public KeyCode interactionKey = KeyCode.E; // ��ȭ ���� Ű

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
            // �÷��̾ ������ ���� ��
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
            Debug.LogError("DialogueManager�� ã�� �� �����ϴ�!");
        }
    }
}
