using System.Collections.Generic;
using UnityEngine;

public class TalkableNPC : MonoBehaviour
{
    public string characterName; // �� NPC�� � ĳ�������� (ex: tuna, whale)
    public float interactionDistance = 3f; // ��ȣ�ۿ� ������ �Ÿ�
    public KeyCode interactionKey = KeyCode.E; // ��ȭ ���� Ű

    private Transform player;

    public List<GameObject> requiredObjects; // ��ǥ ������Ʈ��
    private bool hasStartedDialogue = false;   // ù ��� �ߴ���
    public bool questCleared = false; // ����Ʈ �ߴ���

    public GameObject questionMarkIcon; // ����ǥUI
    //public GameObject exclamationMarkIcon; // ����ǥ UI
    private bool dialogueAfterQuestDone = false; // ����Ʈ �� ������ �Ϸ�ƴ���

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        UpdateQuestIcon();
    }

    private void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        /*if (distance <= interactionDistance)
        {
            // �÷��̾ ������ ���� ��
            if (Input.GetKeyDown(interactionKey))
            {
                StartDialogue();
            }
        }*/

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
            dialogueManager.StartDialogue(characterName, this);
        }
        else if (questCleared && !dialogueManager.IsDialoguePlaying && !dialogueAfterQuestDone)
        {
            dialogueManager.ContinueDialogue(characterName); // �̾ ���
        }
        else
        {
            Debug.Log("��ȣ�ۿ� �Ұ��� ����");
        }
        //���� ���渶�� ICON Update
        UpdateQuestIcon();
    }

    public void CheckQuestCondition()
    {
        requiredObjects.RemoveAll(obj => obj == null);
        if (requiredObjects.Count == 0)
        {
            questCleared = true;
            Debug.Log($"[����Ʈ �Ϸ��]: {characterName}");
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
            Debug.LogError("DialogueManager�� ã�� �� �����ϴ�!");
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
}
