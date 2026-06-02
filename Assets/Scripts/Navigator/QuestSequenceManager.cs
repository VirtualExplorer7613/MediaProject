using System.Collections.Generic;
using UnityEngine;

public class QuestSequenceManager : MonoBehaviour
{
    public static QuestSequenceManager Instance { get; private set; }

    [Header("Debug / Rule")]
    [SerializeField] private bool enforceQuestOrder = true;
    [SerializeField] private bool resetProgressOnStart = false;

    [Header("Sequence")]
    [SerializeField] private List<QuestSequenceTarget> sequenceTargets = new();

    [Header("Navigator")]
    [SerializeField] private QuestPathNavigator pathNavigator;

    private int currentIndex = 0;

    public bool EnforceQuestOrder
    {
        get => enforceQuestOrder;
        set
        {
            enforceQuestOrder = value;
            RefreshNavigator();
        }
    }

    public bool IsOrderEnforced => enforceQuestOrder;

    public QuestSequenceTarget CurrentTarget
    {
        get
        {
            if (sequenceTargets == null || sequenceTargets.Count == 0)
                return null;

            if (currentIndex < 0 || currentIndex >= sequenceTargets.Count)
                return null;

            return sequenceTargets[currentIndex];
        }
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (resetProgressOnStart)
        {
            PlayerPrefs.DeleteKey("QuestSequence_CurrentIndex");

            foreach (var target in sequenceTargets)
            {
                if (target != null)
                    target.ForceSetCompleted(false);
            }
        }

        currentIndex = PlayerPrefs.GetInt("QuestSequence_CurrentIndex", 0);
        currentIndex = Mathf.Clamp(currentIndex, 0, sequenceTargets.Count);

        SkipAlreadyCompletedTargets();
        RefreshNavigator();
    }

    private void Update()
    {
        // 디버그용: F8로 순서 강제 on/off
        if (Input.GetKeyDown(KeyCode.F8))
        {
            //enforceQuestOrder = !enforceQuestOrder;
            EnforceQuestOrder = !EnforceQuestOrder;
            Debug.Log($"[QuestSequence] enforceQuestOrder = {enforceQuestOrder}");
        }

        // 디버그용: F9로 현재 목표 강제 완료
        if (Input.GetKeyDown(KeyCode.F9))
        {
            var current = CurrentTarget;
            if (current != null)
                CompleteObjective(current.ObjectiveId);
        }
    }

    public bool CanInteract(string objectiveId)
    {
        if (string.IsNullOrEmpty(objectiveId))
            return true;

        if (!enforceQuestOrder)
            return true;

        QuestSequenceTarget current = CurrentTarget;
        if (current == null)
            return false;

        return current.ObjectiveId == objectiveId;
    }

    public bool IsCurrentObjective(string objectiveId)
    {
        QuestSequenceTarget current = CurrentTarget;
        if (current == null)
            return false;

        return current.ObjectiveId == objectiveId;
    }

    public void CompleteObjective(string objectiveId)
    {
        if (string.IsNullOrEmpty(objectiveId))
            return;

        QuestSequenceTarget current = CurrentTarget;

        if (enforceQuestOrder)
        {
            if (current == null)
                return;

            if (current.ObjectiveId != objectiveId)
            {
                Debug.LogWarning($"[QuestSequence] 순서가 아닌 목표 완료 요청 무시: {objectiveId}, current={current.ObjectiveId}");
                return;
            }
        }

        Debug.Log($"[QuestSequence] 완료: {objectiveId}");

        if (current != null && current.ObjectiveId == objectiveId)
            current.ForceSetCompleted(true);

        SkipAlreadyCompletedTargets();

        PlayerPrefs.SetInt("QuestSequence_CurrentIndex", currentIndex);
        PlayerPrefs.Save();

        RefreshNavigator();
    }

    private void SkipAlreadyCompletedTargets()
    {
        while (currentIndex < sequenceTargets.Count)
        {
            QuestSequenceTarget target = sequenceTargets[currentIndex];

            if (target == null)
            {
                currentIndex++;
                continue;
            }

            if (!target.Completed)
                break;

            currentIndex++;
        }
    }

    private void RefreshNavigator()
    {
        if (pathNavigator == null)
        {
            RefreshAllNpcSequenceIcons();
            return;
        }

        QuestSequenceTarget current = CurrentTarget;

        if (current == null)
        {
            pathNavigator.ClearTarget();
            Debug.Log("[QuestSequence] 모든 목표 완료");
            RefreshAllNpcSequenceIcons();
            return;
        }

        if (!current.ShowNavigator)
        {
            pathNavigator.ClearTarget();
            Debug.Log($"[QuestSequence] 현재 목표는 Navigator 표시 안 함: {current.ObjectiveId}");
            RefreshAllNpcSequenceIcons();
            return;
        }

        pathNavigator.SetTarget(current);
        Debug.Log($"[QuestSequence] 다음 목표: {current.ObjectiveId}");

        RefreshAllNpcSequenceIcons();
    }

    private void RefreshAllNpcSequenceIcons()
    {
        TalkableNPC[] npcs = FindObjectsOfType<TalkableNPC>();

        foreach (var npc in npcs)
        {
            if (npc != null)
                npc.RefreshSequenceIcon();
        }
    }
}