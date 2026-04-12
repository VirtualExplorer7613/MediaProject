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

    //public List<GameObject> requiredObjects; // 목표 오브젝트들
    private bool hasStartedDialogue = false;   // 첫 대사 했는지
    public bool questCleared = false; // 퀘스트 했는지

    private bool dialogueAfterQuestDone = false; // 퀘스트 후 대사까지 완료됐는지

    [Header("Quest Icons")]
    [SerializeField] private GameObject questionMarkIcon;     // 퀘스트 받기 전
    [SerializeField] private GameObject exclamationMarkIcon;  // 퀘스트 완료 후 보고 가능
    [SerializeField] private GameObject heartIcon;            // 최종 대화 종료 후

    //private bool questClearEffectPlayed = false;

    public Transform dialogueAnchor; // 대화할때 플레어이어의 위치

    private bool uiInteractPressed = false;

    [Header("First Detect Icon")]
    [SerializeField] private bool showQuestionMarkOnlyAfterFirstApproach = false;
    [SerializeField] private float hideConditionDistance = 10f;
    private bool hasShownFirstApproachIcon = false;


    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        foreach (var target in requiredTargets)
        {
            if (target == null) continue;
            target.BindOwner(this);
            target.SetQuestEnabled(false);
        }

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
        if (!questCleared)
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
        DialogueManager dialogueManager = DialogueManager.Instance;
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
        RefreshIcons();
        DialogueManager.Instance?.TryTriggerEndingAfterFinalDialogue();
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
        bool showQuestion = false;
        bool showExclamation = false;
        bool showHeart = false;

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

}
