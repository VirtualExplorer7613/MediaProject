using UnityEngine;

public class InteractionSystem : MonoBehaviour
{
    public static InteractionSystem Instance;

    private void Awake()
    {
        Instance = this;
    }
    public void StartInteraction(string action)
    {
        Debug.Log($"[��ȣ�ۿ� ����]: {action}");

        // ����: 'remove_question_icon'�� ���� action�� ���� ó��
        if (action == "remove_question_icon")
        {
            // ����ǥ �������� �����ϴ� �ڵ�
            RemoveQuestionIcon();
        }
        else if (action == "start_dialogue")
        {
            DialogueManager dialogueManager = FindObjectOfType<DialogueManager>();
            if (dialogueManager == null)
            {
                dialogueManager = Object.FindFirstObjectByType<DialogueManager>();
            }

            if (dialogueManager != null)
            {
                string characterName = "tuna";  // ���� ĳ���� �̸�
                dialogueManager.StartDialogue(characterName, null);
            }
        }
        else if (action.Contains("����") || action.Contains("������") || action.Contains("������"))
        {
            // �̷� Ű���尡 �� ��ȣ�ۿ��̸�, ���� ��ȣ�ۿ��� �ܺο��� ����ǰ�,
            // �Ϸ� �� InteractionSystem.Instance.NotifyInteractionComplete()�� ȣ���
            Debug.Log("�� �÷��̾� ��ȣ�ۿ��� ��ٸ��� ��...");
        }
        else
        {
            Debug.LogWarning($"�� �� ���� action: {action}");
        }
    }

    private void RemoveQuestionIcon()
    {
        // ����ǥ ������ ���� ������ ���⿡ �ۼ�
        Debug.Log("[����ǥ ������ ����] �۾� ����");
    }

    public void NotifyInteractionComplete()
    {
        var npc = DialogueManager.Instance.CurrentTalkingNPC;

        if (npc != null)
        {
            Debug.Log($"�� NPC {npc.characterName}�� ����Ʈ �Ϸ� �˸�");
            npc.CheckQuestCondition();
        }

        DialogueManager.Instance?.OnInteractionCompleted();
    }
}
