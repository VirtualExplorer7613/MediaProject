using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TalkableNPC : MonoBehaviour
{
    public string characterName; // 이 NPC가 어떤 캐릭터인지 (ex: tuna, whale)
    public float interactionDistance = 3f; // 상호작용 가능한 거리
    public KeyCode interactionKey = KeyCode.R; // 대화 시작 키

    private Transform player;

    // 새로 개정된 퀘스트 완료에 필요한 목표물
    [SerializeField] private List<QuestTarget> requiredTargets = new();

    public List<QuestTarget> GetRequiredTargets()
    {
        // 원본 리스트를 실수로 수정하는 걸 방지
        return new List<QuestTarget>(requiredTargets);
    }

    //public List<GameObject> requiredObjects; // 목표 오브젝트들
    private bool hasStartedDialogue = false;   // 첫 대사 했는지
    public bool questCleared = false; // 퀘스트 했는지

    private bool dialogueAfterQuestDone = false; // 퀘스트 후 대사까지 완료됐는지

    [Header("Quest Icons")]
    [SerializeField] private GameObject questionMarkIcon;     // 퀘스트 받기 전
    [SerializeField] private GameObject exclamationMarkIcon;  // 퀘스트 완료 후 보고 가능
    [SerializeField] private GameObject heartIcon;            // 최종 대화 종료 후

    //private bool questClearEffectPlayed = false;

    [Header("Dialogue Reward")]
    [SerializeField] private ItemDefinition rewardItemAfterFinalDialogue;
    [SerializeField] private bool giveRewardAfterFinalDialogue = false;

    private bool rewardGiven = false;

    public Transform dialogueAnchor; // 대화할때 플레어이어의 위치

    private bool uiInteractPressed = false;

    [Header("First Detect Icon")]
    [SerializeField] private bool showQuestionMarkOnlyAfterFirstApproach = false;
    [SerializeField] private float hideConditionDistance = 10f;
    private bool hasShownFirstApproachIcon = false;

    [Header("Quest Sequence")]
    [SerializeField] private QuestSequenceTarget sequenceTarget;
    [SerializeField] private bool hideIconWhenNotCurrentSequence = true;

    [Header("Dialogue Only NPC")]
    [SerializeField] private bool dialogueOnlyNpc = false;

    [SerializeField] private bool ignoreInteractionEntryForDialogueOnly = true;

    [SerializeField] private bool loadSceneOnInteractAfterFinished = false;
    [SerializeField] private float sceneLoadDelayAfterDialogue = 0.3f;
    [SerializeField] private Define.Scene sceneAfterDialogue = Define.Scene.Ending;
    private bool sceneLoadStarted = false;

    public bool IsDialogueOnlyNpc => dialogueOnlyNpc;
    public bool ShouldIgnoreInteractionEntryForDialogueOnly =>
        dialogueOnlyNpc && ignoreInteractionEntryForDialogueOnly;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        foreach (var target in requiredTargets)
        {
            if (target == null) continue;
            target.BindOwner(this);
            target.SetQuestEnabled(false);
        }

        if (sequenceTarget == null)
            sequenceTarget = GetComponent<QuestSequenceTarget>();

        /*// Orca 같은 특수 NPC는 시작 시 아이콘 숨김
        if (showQuestionMarkOnlyAfterFirstApproach)
        {
            questionMarkIcon?.SetActive(false);
        }
        else
        {
            UpdateQuestIcon();
        }*/

        RefreshIcons();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        HandleFirstApproachQuestionIcon(distance);

        /*if (distance <= interactionDistance)
        {
            // 플레이어가 가까이 있을 때
            if (Input.GetKeyDown(interactionKey))
            {
                StartDialogue();
            }
        }*/

        #region questionHintUI 관련 로직 제거
        /*
        if (!questCleared && hasStartedDialogue)
        {
            ShowquestionHintUI(false);
        }
        
        if(questCleared && !dialogueAfterQuestDone)
        {
            ShowquestionHintUI(true);
        }

        if (dialogueAfterQuestDone)
        {
            ShowquestionHintUI(false);
        }
        */
        #endregion


        // 퀘스트 상태 선제 감지
        if (!questCleared && !dialogueOnlyNpc)
        {
            CheckQuestCondition(); // 조건을 계속 확인
        }

        bool inputPressed = Input.GetKeyDown(interactionKey) || uiInteractPressed;

        if (distance <= interactionDistance && inputPressed)
        {
            TryTalk();
        }

        uiInteractPressed = false;
    }

    private void TryTalk()
    {
        if (sequenceTarget != null && !sequenceTarget.CanInteractHere())
        {
            Debug.Log($"[TalkableNPC] 아직 상호작용 순서가 아닙니다. npc={characterName}");
            return;
        }

        DialogueManager dialogueManager = DialogueManager.Instance;

        if (dialogueOnlyNpc)
        {
            TryTalkDialogueOnly(dialogueManager);
            return;
        }

        if (dialogueManager == null) return;

        CheckQuestCondition();

        if (!questCleared && !hasStartedDialogue)
        {
            hasStartedDialogue = true;
            // 상호작용 가능하게 만들기
            EnableQuestItems();
            dialogueManager.StartDialogue(characterName, this);
            RefreshIcons();
            return;
        }
        else if (questCleared && !dialogueManager.IsDialoguePlaying && !dialogueAfterQuestDone)
        {
            dialogueManager.ContinueDialogue(characterName, this); // 이어서 출력
            RefreshIcons();
            return;
        }
        else
        {
            Debug.Log("상호작용 불가능 상태");
        }
        //상태 변경마다 ICON Update
        //UpdateQuestIcon();
        RefreshIcons();
    }

    private void TryTalkDialogueOnly(DialogueManager dialogueManager)
    {
        if (dialogueManager == null) return;

        // 이미 대화가 완전히 끝난 상태
        if (dialogueAfterQuestDone)
        {
            if (loadSceneOnInteractAfterFinished)
            {
                Debug.Log($"[DialogueOnlyNPC] {characterName} 대화 완료 후 씬 이동: {sceneAfterDialogue}");
                Managers.Scene.LoadSceneAsync(sceneAfterDialogue, false);
            }
            else
            {
                Debug.Log($"[DialogueOnlyNPC] {characterName} 대화는 이미 완료되었습니다.");
            }

            return;
        }

        if (dialogueManager.IsDialoguePlaying)
            return;

        // 첫 대화 시작
        if (!hasStartedDialogue)
        {
            hasStartedDialogue = true;
            dialogueManager.StartDialogue(characterName, this);
            RefreshIcons();
            return;
        }

        // 혹시 중간 저장된 interaction 지점이 있는 경우를 대비
        dialogueManager.ContinueDialogue(characterName, this);
        RefreshIcons();
    }

    public void CheckQuestCondition()
    {
        requiredTargets.RemoveAll(t => t == null);
        foreach (var target in requiredTargets)
        {
            if (target != null && !target.Completed)
                return;
        }

        if (questCleared)
            return;

        questCleared = true;
        Debug.Log($"[퀘스트 완료됨]: {characterName}");

        /* if (!questClearEffectPlayed)
         {
             questClearEffectPlayed = true;
             ShowHeartIcon();
         }
         UpdateQuestIcon();*/

        RefreshIcons();
    }

    private void StartDialogue()
    {
        DialogueManager dialogueManager = FindFirstObjectByType<DialogueManager>();
        if (dialogueManager != null)
        {
            dialogueManager.StartDialogue(characterName, this);
        }
        else
        {
            Debug.LogError("DialogueManager를 찾을 수 없습니다!");
        }
    }

    public void UpdateQuestIcon()
    {
        /* if (!hasStartedDialogue)
         {
             questionMarkIcon?.SetActive(true);
             //exclamationMarkIcon?.SetActive(false);
         }
         else
         {
             questionMarkIcon?.SetActive(false);
             //exclamationMarkIcon?.SetActive(false);
         }*/

        RefreshIcons();
    }

    public void OnDialogueAfterQuestFinished()
    {
        dialogueAfterQuestDone = true;
        /*ShowquestionHintUI(false);
        UpdateQuestIcon();*/

        if (dialogueOnlyNpc)
            questCleared = true;

        if (sequenceTarget != null)
            sequenceTarget.NotifyCompleted();

        RefreshIcons();

        TryGiveFinalDialogueReward();

        // 벨루가 같은 Dialogue Only NPC는 대화 종료 직후 바로 씬 이동
        if (dialogueOnlyNpc && loadSceneOnInteractAfterFinished)
        {
            RefreshIcons();
            StartCoroutine(LoadSceneAfterDialogueDelay());
            return;
            /*
            Debug.Log($"[DialogueOnlyNPC] {characterName} 대화 종료 직후 씬 이동: {sceneAfterDialogue}");
            Managers.Scene.LoadSceneAsync(sceneAfterDialogue, false);
            return;*/
        }

        DialogueManager.Instance?.TryTriggerEndingAfterFinalDialogue();
    }

    private IEnumerator LoadSceneAfterDialogueDelay()
    {
        if (sceneLoadStarted)
            yield break;

        sceneLoadStarted = true;

        if (sceneLoadDelayAfterDialogue > 0f)
            yield return new WaitForSecondsRealtime(sceneLoadDelayAfterDialogue);

        Debug.Log($"[DialogueOnlyNPC] {characterName} 대화 종료 후 씬 이동: {sceneAfterDialogue}");
        Managers.Scene.LoadSceneAsync(sceneAfterDialogue, false);
    }

    private void TryGiveFinalDialogueReward()
    {
        if (!giveRewardAfterFinalDialogue) return;
        if (rewardGiven) return;
        if (rewardItemAfterFinalDialogue == null) return;

        rewardGiven = true;

        if (ItemPickupPopup.Instance != null)
        {
            ItemPickupPopup.Instance.Show(rewardItemAfterFinalDialogue);
            Debug.Log($"[NPC Reward] {characterName} 보상 지급: {rewardItemAfterFinalDialogue.name}");
        }
        else
        {
            Debug.LogWarning($"[NPC Reward] ItemPickupPopup.Instance를 찾을 수 없습니다. npc={characterName}");
        }
    }

    void ShowHeartIcon()
    {
        if (heartIcon != null)
        {
            heartIcon.SetActive(true);
        }
    }

    /*void ShowquestionHintUI(bool show)
    {
        if(questionHintUI != null)
        {
            questionHintUI.SetActive(show);
        }
    }*/

    private void EnableQuestItems()
    {
        foreach (var target in requiredTargets)
        {
            if (target == null) continue;
            target.SetQuestEnabled(true);
        }
    }

    public void NotifyQuestTargetCompleted(QuestTarget target)
    {
        CheckQuestCondition();
    }

    public void OnInteractButtonPressed()
    {
        uiInteractPressed = true;

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (player == null)
        {
            Debug.LogWarning($"[TalkableNPC] Player를 찾지 못했습니다. npc={characterName}");
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > interactionDistance)
        {
            Debug.Log($"[TalkableNPC] 거리 밖입니다. npc={characterName}, dist={distance}, allowed={interactionDistance}");
            return;
        }

        TryTalk();
    }

    private void HandleFirstApproachQuestionIcon(float distance)
    {
        if (!showQuestionMarkOnlyAfterFirstApproach) return;
        if (hasShownFirstApproachIcon) return;
        if (hasStartedDialogue) return;

        if (distance <= hideConditionDistance)
        {
            hasShownFirstApproachIcon = true;
            RefreshIcons();
        }
    }

    private void RefreshIcons()
    {
        bool isCurrentSequenceTarget = true;

        /* if (hideIconWhenNotCurrentSequence && sequenceTarget != null)
             isCurrentSequenceTarget = sequenceTarget.IsCurrentTarget();*/

        bool shouldHideBySequence = 
            (hideIconWhenNotCurrentSequence && 
            sequenceTarget != null && 
            QuestSequenceManager.Instance != null && 
            QuestSequenceManager.Instance.IsOrderEnforced);

        if (shouldHideBySequence)
            isCurrentSequenceTarget = sequenceTarget.IsCurrentTarget();

        bool showQuestion = false;
        bool showExclamation = false;
        bool showHeart = false;

        if (!isCurrentSequenceTarget && !dialogueAfterQuestDone)
        {
            if (questionMarkIcon != null) 
                questionMarkIcon.SetActive(false);

            if (exclamationMarkIcon != null) 
                exclamationMarkIcon.SetActive(false);

            if (heartIcon != null) heartIcon.SetActive(false);

            return;
        }

        if (dialogueOnlyNpc)
        {
            if (dialogueAfterQuestDone)
            {
                showExclamation = true;
            }
            else if (!hasStartedDialogue)
            {
                if (!showQuestionMarkOnlyAfterFirstApproach || hasShownFirstApproachIcon)
                    showQuestion = true;
            }

            if (questionMarkIcon != null) questionMarkIcon.SetActive(showQuestion);
            if (exclamationMarkIcon != null) exclamationMarkIcon.SetActive(showExclamation);
            if (heartIcon != null) heartIcon.SetActive(false);

            return;
        }

        // 1) 상호작용 종료 후
        if (dialogueAfterQuestDone)
        {
            showHeart = true;
        }
        // 2) 퀘스트 완료, 아직 보고 전
        else if (questCleared)
        {
            showExclamation = true;
        }
        // 3) 퀘스트 받기 전
        else if (!hasStartedDialogue)
        {
            // Orca 특수 처리 포함
            if (!showQuestionMarkOnlyAfterFirstApproach || hasShownFirstApproachIcon)
                showQuestion = true;
        }
        // 4) 퀘스트 진행 중
        else
        {
            // 아무 아이콘도 안 띄움
        }

        if (questionMarkIcon != null) questionMarkIcon.SetActive(showQuestion);
        if (exclamationMarkIcon != null) exclamationMarkIcon.SetActive(showExclamation);
        if (heartIcon != null) heartIcon.SetActive(showHeart);
    }

    public void RefreshSequenceIcon()
    {
        RefreshIcons();
    }
}
