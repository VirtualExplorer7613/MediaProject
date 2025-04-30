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
        Debug.Log($"[상호작용 시작]: {action}");

        // 예시: 'remove_question_icon'과 같은 action에 대한 처리
        if (action == "remove_question_icon")
        {
            // 물음표 아이콘을 제거하는 코드
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
                string characterName = "tuna";  // 예시 캐릭터 이름
                dialogueManager.StartDialogue(characterName, null);
            }
        }
        else if (action.Contains("빨대") || action.Contains("초음파") || action.Contains("쓰레기"))
        {
            // 이런 키워드가 들어간 상호작용이면, 실제 상호작용은 외부에서 수행되고,
            // 완료 시 InteractionSystem.Instance.NotifyInteractionComplete()가 호출됨
            Debug.Log("▶ 플레이어 상호작용을 기다리는 중...");
        }
        else
        {
            Debug.LogWarning($"알 수 없는 action: {action}");
        }
    }

    private void RemoveQuestionIcon()
    {
        // 물음표 아이콘 제거 로직을 여기에 작성
        Debug.Log("[물음표 아이콘 제거] 작업 수행");
    }

    public void NotifyInteractionComplete()
    {
        var npc = DialogueManager.Instance.CurrentTalkingNPC;

        if (npc != null)
        {
            Debug.Log($"▶ NPC {npc.characterName}에 퀘스트 완료 알림");
            npc.CheckQuestCondition();
        }

        DialogueManager.Instance?.OnInteractionCompleted();
    }
}
