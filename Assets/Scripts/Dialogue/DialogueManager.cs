using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public TalkableNPC CurrentTalkingNPC { get; private set; }

    private WhaleController whaleController;

    public TMP_Text dialogueText;         // ��ȭ ������ ǥ���� �ؽ�Ʈ
    public GameObject dialogueBox;    // ��ȭâ �г�

    public Camera mainCamera;
    public Vector3 spawnOffset = new Vector3(2f, 0f, 3f); // ĳ���� ��ġ ������ (���� ����)
    private GameObject currentCharacterModel;
    public bool IsDialoguePlaying { get; private set; }

    private DialogueData dialogueData;
    private int currentIndex = 0;
    private bool waitingForInteraction = false;
    private bool canProceed = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        whaleController = FindObjectOfType<WhaleController>();
    }


    public void StartDialogue(string characterName, TalkableNPC npc)
    {
        if (CurrentTalkingNPC != null && !CurrentTalkingNPC.questCleared)
        {
            Debug.Log("�̹� �ٸ� NPC�� ��ȣ�ۿ� ���Դϴ�.");
            return;
        }

        if (whaleController != null)
        {
            whaleController.SetVisible(false);
        }

        CurrentTalkingNPC = npc;

        // Resources �������� �ش� ĳ���� �̸��� �´� JSON ������ �ε�
        TextAsset jsonFile = Resources.Load<TextAsset>($"{characterName}_interaction");

        if (jsonFile != null)
        {
            // JSON ������ DialogueData ��ü�� ��ȯ
            dialogueData = JsonUtility.FromJson<DialogueData>(jsonFile.text);
            currentIndex = 0;
            ShowCharacter(characterName);
            IsDialoguePlaying = true;
            StartCoroutine(PlayDialogueSequence());
        }
        else
        {
            Debug.LogError($"Failed to load dialogue data for {characterName}.");
        }
    }


    private IEnumerator PlayDialogueSequence()
    {
        dialogueBox.SetActive(true);

        while (currentIndex < dialogueData.dialogue_sequence.Count)
        {
            DialogueEntry entry = dialogueData.dialogue_sequence[currentIndex];

            if (entry.type == "player_detects")
            {
                HandleAction(entry);
                currentIndex++;
                continue;
            }
    
            else if (entry.type == "interaction")
            {
                /*HandleAction(entry);
                waitingForInteraction = true;
                yield return new WaitUntil(() => waitingForInteraction == false);
                currentIndex++;
                continue;*/

                HandleAction(entry);
                waitingForInteraction = true;
                PlayerPrefs.SetInt($"{dialogueData.character}_dialogue_index", currentIndex);
                dialogueBox.SetActive(false);
                HideCharacter();
                IsDialoguePlaying = false;

                // �� �ٽ� ���̱�
                if (whaleController != null)
                {
                    whaleController.SetVisible(true);
                }

                yield break; // ��� ����
            }

            ShowCharacter(entry.type);
            ChangeCharacterExpression(entry.expression);
            yield return ShowDialogue(entry.dialogue);
            yield return new WaitUntil(() => canProceed);
            canProceed = false;
            //IsDialoguePlaying = false; //TODO: �÷��̾� ���� ��ũ��Ʈ ����

            currentIndex++;
            //yield return null;
        }

        /*dialogueBox.SetActive(false);
        HideCharacter();*/

        PlayerPrefs.DeleteKey($"{dialogueData.character}_dialogue_index");
        dialogueBox.SetActive(false);
        HideCharacter();

        if (whaleController != null)
        { 
            whaleController.SetVisible(true); 
        }

        IsDialoguePlaying = false;
        CurrentTalkingNPC = null;
    }
    void Update()
    {
        if (IsDialoguePlaying && Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayerPressedNext();
        }
    }

    private void HandleAction(DialogueEntry entry)
    {
        if (entry.type == "player_detects")
        {
            Debug.Log("����ǥ ������ ����");
            //TODO: ����ǥ ���� �ڵ� �߰�
        }
        else if (entry.type == "interaction")
        {
            Debug.Log($"��ȣ�ۿ� ��û: {entry.action}");
            //TODO: ��ȣ�ۿ� �ý��ۿ� ��û ������
            InteractionSystem.Instance.StartInteraction(entry.action);
        }
    }

    private IEnumerator ShowDialogue(string dialogue)
    {
        dialogueText.text = dialogue.Replace("\\n", "\n");
        yield return null;
    }

    public void OnPlayerPressedNext()
    {
        if (!waitingForInteraction)
            canProceed = true;
    }

    public void OnInteractionCompleted()
    {
        waitingForInteraction = false;
    }

    private void ChangeCharacterExpression(string expressionName)
    {
        if (string.IsNullOrEmpty(expressionName))
            expressionName = dialogueData.expressions.@default;

        if (currentCharacterModel != null)
        {
            Animator animator = currentCharacterModel.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play(expressionName,1);
            }
        }
    }

    private void ShowCharacter(string characterName)
    {
        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);

        GameObject prefab = Resources.Load<GameObject>($"Characters/{characterName}");
        if (prefab != null)
        {
            // �⺻ ���� ��ġ: ī�޶� �� spawnOffset �Ÿ�
            Vector3 basePosition = mainCamera.transform.position + mainCamera.transform.forward * spawnOffset.z + Vector3.up * spawnOffset.y;

            // ���� ����: ���� ����, �������� ������
            Vector3 sideOffset = mainCamera.transform.right * spawnOffset.x;
            if (characterName == "whale")
            {
                basePosition -= sideOffset; // ����
            }
            else
            {
                basePosition += sideOffset; // ������
            }

            currentCharacterModel = Instantiate(prefab, basePosition, Quaternion.LookRotation(mainCamera.transform.forward));
        }
        else
        {
            Debug.LogError($"ĳ���� ������ {characterName}��(��) ã�� �� �����ϴ�.");
        }
    }

    private void HideCharacter()
    {
        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);
    }

    public void ContinueDialogue(string characterName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"{characterName}_interaction");
        if (jsonFile != null)
        {
            dialogueData = JsonUtility.FromJson<DialogueData>(jsonFile.text);

            // ���� ���� ���� ��� (PlayerPrefs�� NPC���� ���޵� ����)
            currentIndex = PlayerPrefs.GetInt($"{characterName}_dialogue_index", 0);
            if (dialogueData.dialogue_sequence[currentIndex].type == "interaction")
                currentIndex++; // interaction �ǳʶٰ� ��������

            //�� �����
            if (whaleController != null)
            {
                whaleController.SetVisible(false);
            }

            ShowCharacter(characterName);
            IsDialoguePlaying = true;
            StartCoroutine(PlayDialogueSequence());
        }
    }
}
