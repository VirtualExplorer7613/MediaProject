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

    public GameObject questionMarkIcon; // 물음표UI
    //public GameObject exclamationMarkIcon; // 느낌표 UI
    private bool dialogueAfterQuestDone = false; // 퀘스트 후 대사까지 완료됐는지
    public GameObject heartIcon;
    public GameObject questionHintUI;
    private bool questClearEffectPlayed = false;

    public Transform dialogueAnchor; // 대화할때 플레어이어의 위치

    private bool uiInteractPressed = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;

        foreach (var target in requiredTargets)
        {
            if (target == null) continue;
            target.BindOwner(this);
            target.SetQuestEnabled(false);
        }

        UpdateQuestIcon();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);


        /*if (distance <= interactionDistance)
        {
            // 플레이어가 가까이 있을 때
            if (Input.GetKeyDown(interactionKey))
            {
                StartDialogue();
            }
        }*/

        if(!questCleared && hasStartedDialogue)
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
        }
        else if (questCleared && !dialogueManager.IsDialoguePlaying && !dialogueAfterQuestDone)
        {
            dialogueManager.ContinueDialogue(characterName, this); // 이어서 출력
        }
        else
        {
            Debug.Log("상호작용 불가능 상태");
        }
        //상태 변경마다 ICON Update
        UpdateQuestIcon();
    }

    public void CheckQuestCondition()
    {
        requiredTargets.RemoveAll(t => t == null);
        foreach (var target in requiredTargets)
        {
            if (target != null && !target.Completed)
                return;
        }

        questCleared = true;
        Debug.Log($"[퀘스트 완료됨]: {characterName}");

        if (!questClearEffectPlayed)
        {
            questClearEffectPlayed = true;
            ShowHeartIcon();
        }
        UpdateQuestIcon();
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
        if (!hasStartedDialogue)
        {
            questionMarkIcon?.SetActive(true);
            //exclamationMarkIcon?.SetActive(false);
        }
        else
        {
            questionMarkIcon?.SetActive(false);
            //exclamationMarkIcon?.SetActive(false);
        }
    }

    public void OnDialogueAfterQuestFinished()
    {
        dialogueAfterQuestDone = true;
        ShowquestionHintUI(false);
        UpdateQuestIcon();
    }
    
    void ShowHeartIcon()
    {
        if (heartIcon != null)
        {
            heartIcon.SetActive(true);
        }
    }

    void ShowquestionHintUI(bool show)
    {
        if(questionHintUI != null)
        {
            questionHintUI.SetActive(show);
        }
    }

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
    }

}
