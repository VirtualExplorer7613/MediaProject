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
                dialogueManager.StartDialogue(characterName);
            }
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
}
