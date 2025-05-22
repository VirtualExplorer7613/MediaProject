using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using TMPro.Examples;
using UnityEngine.SceneManagement;


public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public TalkableNPC CurrentTalkingNPC { get; private set; }

    private WhaleController whaleController;

    public TMP_Text dialogueText;         // ��ȭ ������ ǥ���� �ؽ�Ʈ
    public GameObject dialogueBox;    // ��ȭâ �г�
    public Image characterNameBox; //�̸�â
    public TMP_Text characterNameText; //ĳ�����̸�


    public Camera mainCamera;
    public Vector3 spawnOffset = new Vector3(1.6f, 0.25f, 3f); // ĳ���� ��ġ ������ (���� ����)
    private GameObject currentCharacterModel;
    public bool IsDialoguePlaying { get; private set; }
    private string currentCharacterName = null;

    private DialogueData dialogueData;
    private int currentIndex = 0;
    private bool waitingForInteraction = false;
    private bool canProceed = false;

    [System.Serializable]
    public class CharacterInfo
    {
        public string displayName;
        public Color nameColor;
    }

    private Dictionary<string, CharacterInfo> characterInfoMap = new Dictionary<string, CharacterInfo>
    {
        { "whale", new CharacterInfo { displayName = "��", nameColor = new Color(0.6f, 0.7f, 0.8f) } },
        { "seahorse", new CharacterInfo { displayName = "�ظ�", nameColor = new Color(1f, 0.85f, 0.6f) } },
        { "tuna", new CharacterInfo { displayName = "��ġ", nameColor = new Color(0.6f, 0.75f, 0.85f) } },
        { "turtle", new CharacterInfo { displayName = "�ź�", nameColor = new Color(0.7f, 0.85f, 0.6f) } },
        { "octopus", new CharacterInfo { displayName = "����", nameColor = new Color(0.85f, 0.6f, 0.7f) } },
        { "crab", new CharacterInfo { displayName = "��", nameColor = new Color(1f, 0.6f, 0.6f) } },
        { "clownfish", new CharacterInfo { displayName = "�򵿰���", nameColor = new Color(1f, 0.75f, 0.5f) } },
        { "orca", new CharacterInfo { displayName = "����", nameColor = new Color(0.7f, 0.7f, 0.75f) } },
        { "dolphin", new CharacterInfo { displayName = "����", nameColor = new Color(0.7f, 0.85f, 0.95f) } },
        { "squid", new CharacterInfo { displayName = "��¡��", nameColor = new Color(0.85f, 0.75f, 1f) } },
    };


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
            SetCharacterNameAndColor(entry.type);
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
        CurrentTalkingNPC.OnDialogueAfterQuestFinished();
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
        if (currentCharacterModel != null && currentCharacterName == characterName)
        {
            // ���� ĳ���͸� ��������� ����
            return;
        }

        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);

        GameObject prefab = Resources.Load<GameObject>($"Characters/{characterName}");
        if (prefab != null)
        {
            // �⺻ ���� ��ġ: ī�޶� �� spawnOffset �Ÿ�
            Vector3 basePosition = mainCamera.transform.position + mainCamera.transform.forward * spawnOffset.z + Vector3.up * spawnOffset.y;

            // ���� ����: ���� ����, �������� ������
            Vector3 sideOffset = mainCamera.transform.right * spawnOffset.x;

            Quaternion offset;

            if (characterName == "whale")
            {
                basePosition -= sideOffset; // ����
                offset = Quaternion.Euler(0f, -30f, 0f);
            }
            else
            {
                basePosition += sideOffset; // ������
                offset = Quaternion.Euler(0f, 30f, 0f);
            }

            Vector3 directionToCamera = mainCamera.transform.position - basePosition;
            directionToCamera.y = 0f;

            Quaternion lookRotation = Quaternion.LookRotation(directionToCamera.normalized)*offset;
            lookRotation *= offset;

            currentCharacterModel = Instantiate(prefab, basePosition, lookRotation);
            currentCharacterModel.transform.localScale = Vector3.one * 2f;
            currentCharacterName = characterName;
        }
        else
        {
            Debug.LogError($"ĳ���� ������ {characterName}��(��) ã�� �� �����ϴ�.");
        }

        GameObject backdropPrefab = Resources.Load<GameObject>("Prefabs/CharacterBackdrop");
        if (backdropPrefab != null)
        {
            GameObject backdrop = Instantiate(backdropPrefab, currentCharacterModel.transform);
            float zDistance = 5f;

            float frustumHeight = 2.0f * zDistance * Mathf.Tan(Camera.main.fieldOfView * 0.5f * Mathf.Deg2Rad);
            float frustumWidth = frustumHeight * Camera.main.aspect;

            backdrop.transform.position = Camera.main.transform.position + Camera.main.transform.forward * zDistance;
            backdrop.transform.rotation = Camera.main.transform.rotation;
            backdrop.transform.Rotate(-90f, 0f, 0f);

            backdrop.transform.localScale = new Vector3(frustumWidth, 1f, frustumHeight);

        }

    }

    private void HideCharacter()
    {
        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);

        // NameBox �����
        characterNameText.gameObject.SetActive(false);
        characterNameBox.gameObject.SetActive(false);
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

    private void SetCharacterNameAndColor(string characterType)
    {
        if (characterInfoMap.TryGetValue(characterType, out CharacterInfo info))
        {
            characterNameText.text = info.displayName;
            characterNameText.color = Color.black;
            characterNameBox.color = info.nameColor;
            characterNameText.gameObject.SetActive(true);
            characterNameBox.gameObject.SetActive(true);
        }
        else
        {
            characterNameText.text = "";
            characterNameText.gameObject.SetActive(false);
            characterNameBox.gameObject.SetActive(false);
        }
    }

    public bool AreAllQuestsCleared()
    {
        TalkableNPC[] npcs = FindObjectsOfType<TalkableNPC>();
        foreach (var npc in npcs)
        {
            if (!npc.questCleared)
                return false;
        }
        return true;
    }
    public void ForceCompleteAllQuests()
    {
        foreach (var npc in FindObjectsOfType<TalkableNPC>())
        {
            npc.questCleared = true;
            // Ȥ�� �ʿ��� �ٸ� ���µ� ���� �ʱ�ȭ or �Ϸ� ���·� ����
        }
    }

}
