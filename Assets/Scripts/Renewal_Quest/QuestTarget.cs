using UnityEngine;

public enum QuestItemType
{
    Ultrasound,
    Pull,
    Magnet,
    Sponge,
    Scissor,
    Hammer
}

public class QuestTarget : MonoBehaviour
{

    [Header("Quest")]
    [SerializeField] private QuestItemType requiredItemType;
    [SerializeField] private bool questEnabled = false;
    [SerializeField] private bool completed = false;

    private TalkableNPC ownerNpc;

    public QuestItemType RequiredItemType => requiredItemType;
    public bool QuestEnabled => questEnabled;
    public bool Completed => completed;

    public void BindOwner(TalkableNPC npc)
    {
        ownerNpc = npc;
    }

    public void SetQuestEnabled(bool enabled)
    {
        questEnabled = enabled;
    }

    public bool CanBeProcessedBy(QuestItemType itemType)
    {
        // 퀘스트가 끝나지 않고 활성화된 상태에서 아이템 종류가 일치
        return questEnabled && !completed && requiredItemType == itemType;
    }

    public void CompleteQuestTarget()
    {
        if (completed) return;

        completed = true;
        ownerNpc?.NotifyQuestTargetCompleted(this);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
