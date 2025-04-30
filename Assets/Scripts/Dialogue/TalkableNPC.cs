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

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
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
        else if (questCleared && !dialogueManager.IsDialoguePlaying)
        {
            dialogueManager.ContinueDialogue(characterName); // �̾ ���
        }
        else
        {
            Debug.Log("���� ��ǥ�� �Ϸ����� ����");
        }
    }

    public void CheckQuestCondition()
    {
        requiredObjects.RemoveAll(obj => obj == null);
        if (requiredObjects.Count == 0)
        {
            questCleared = true;
            Debug.Log($"[����Ʈ �Ϸ��]: {characterName}");
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
}
