using System.Collections.Generic;
using UnityEngine;

public class TalkableNPC : MonoBehaviour
{
    public string characterName; // 이 NPC가 어떤 캐릭터인지 (ex: tuna, whale)
    public float interactionDistance = 3f; // 상호작용 가능한 거리
    public KeyCode interactionKey = KeyCode.R; // 대화 시작 키

    private Transform player;

    public List<GameObject> requiredObjects; // 목표 오브젝트들
    private bool hasStartedDialogue = false;   // 첫 대사 했는지
    public bool questCleared = false; // 퀘스트 했는지

    public GameObject questionMarkIcon; // 물음표UI
    //public GameObject exclamationMarkIcon; // 느낌표 UI
    private bool dialogueAfterQuestDone = false; // 퀘스트 후 대사까지 완료됐는지
    public GameObject heartIcon;
    private bool questClearEffectPlayed = false;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        DisableQuestItemsInitially();
        UpdateQuestIcon();
    }

    void DisableQuestItemsInitially()
    {
        foreach (var obj in requiredObjects)
        {
            if (obj == null) continue;

            // DraggedItem이 붙어 있으면 interactable 잠금
            if (obj.TryGetComponent(out DraggedItem di))
                di.interactable = false;
        }
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

        // 퀘스트 상태 선제 감지
        if (!questCleared)
        {
            CheckQuestCondition(); // 조건을 계속 확인
        }

        if (distance <= interactionDistance && Input.GetKeyDown(interactionKey))
        {
            TryTalk();
        }
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
            dialogueManager.ContinueDialogue(characterName); // 이어서 출력
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
        requiredObjects.RemoveAll(obj => obj == null);
        if (requiredObjects.Count == 0)
        {
            questCleared = true;
            Debug.Log($"[퀘스트 완료됨]: {characterName}");
            if (!questClearEffectPlayed)
            {
                questClearEffectPlayed = true;
                ShowHeartIcon();
            }
            UpdateQuestIcon();
        }
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
        /*else if (questCleared && !dialogueAfterQuestDone)
        {
            questionMarkIcon?.SetActive(false);
            //exclamationMarkIcon?.SetActive(true);
        }*/
        else
        {
            questionMarkIcon?.SetActive(false);
            //exclamationMarkIcon?.SetActive(false);
        }
    }

    public void OnDialogueAfterQuestFinished()
    {
        dialogueAfterQuestDone = true;
        UpdateQuestIcon();
    }
    
    void ShowHeartIcon()
    {
        if (heartIcon != null)
        {
            heartIcon.SetActive(true);
        }
    }
    
    void EnableQuestItems()
    {
        foreach (var obj in requiredObjects)
        {
            if (obj == null) continue;

            if (obj.TryGetComponent(out DraggedItem di))
            {
                di.interactable = true;
            }
            // ② 초음파-파괴형(DraggedItem 없으면) → Tag = "Trash" 부여
            else
            {
                obj.tag = "Trash";
            }
        }
    }
    
}
