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

    public TMP_Text dialogueText;         // 대화 내용을 표시할 텍스트
    public GameObject dialogueBox;    // 대화창 패널
    public Image characterNameBox; //이름창
    public TMP_Text characterNameText; //캐릭터이름


    public Camera mainCamera;
    public Vector3 spawnOffset = new Vector3(1.6f, 0.25f, 3f); // 캐릭터 위치 오프셋 (조절 가능)
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
        { "whale", new CharacterInfo { displayName = "고래", nameColor = new Color(0.6f, 0.7f, 0.8f) } },
        { "seahorse", new CharacterInfo { displayName = "해마", nameColor = new Color(1f, 0.85f, 0.6f) } },
        { "tuna", new CharacterInfo { displayName = "참치", nameColor = new Color(0.6f, 0.75f, 0.85f) } },
        { "turtle", new CharacterInfo { displayName = "거북", nameColor = new Color(0.7f, 0.85f, 0.6f) } },
        { "octopus", new CharacterInfo { displayName = "문어", nameColor = new Color(0.85f, 0.6f, 0.7f) } },
        { "crab", new CharacterInfo { displayName = "게", nameColor = new Color(1f, 0.6f, 0.6f) } },
        { "clownfish", new CharacterInfo { displayName = "흰동가리", nameColor = new Color(1f, 0.75f, 0.5f) } },
        { "orca", new CharacterInfo { displayName = "범고래", nameColor = new Color(0.7f, 0.7f, 0.75f) } },
        { "dolphin", new CharacterInfo { displayName = "돌고래", nameColor = new Color(0.7f, 0.85f, 0.95f) } },
        { "squid", new CharacterInfo { displayName = "오징어", nameColor = new Color(0.85f, 0.75f, 1f) } },
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
            Debug.Log("이미 다른 NPC와 상호작용 중입니다.");
            return;
        }

        if (whaleController != null)
        {
            whaleController.SetVisible(false);
        }

        CurrentTalkingNPC = npc;

        // Resources 폴더에서 해당 캐릭터 이름에 맞는 JSON 파일을 로드
        TextAsset jsonFile = Resources.Load<TextAsset>($"{characterName}_interaction");

        if (jsonFile != null)
        {
            // JSON 파일을 DialogueData 객체로 변환
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

                // 고래 다시 보이기
                if (whaleController != null)
                {
                    whaleController.SetVisible(true);
                }

                yield break; // 대사 종료
            }

            ShowCharacter(entry.type);
            ChangeCharacterExpression(entry.expression);
            SetCharacterNameAndColor(entry.type);
            yield return ShowDialogue(entry.dialogue);
            yield return new WaitUntil(() => canProceed);
            canProceed = false;
            //IsDialoguePlaying = false; //TODO: 플레이어 조작 스크립트 수정

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
            Debug.Log("물음표 아이콘 제거");
            //TODO: 물음표 제거 코드 추가
        }
        else if (entry.type == "interaction")
        {
            Debug.Log($"상호작용 요청: {entry.action}");
            //TODO: 상호작용 시스템에 요청 보내기
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
            // 같은 캐릭터면 재생성하지 않음
            return;
        }

        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);

        GameObject prefab = Resources.Load<GameObject>($"Characters/{characterName}");
        if (prefab != null)
        {
            // 기본 스폰 위치: 카메라 앞 spawnOffset 거리
            Vector3 basePosition = mainCamera.transform.position + mainCamera.transform.forward * spawnOffset.z + Vector3.up * spawnOffset.y;

            // 방향 결정: 고래는 왼쪽, 나머지는 오른쪽
            Vector3 sideOffset = mainCamera.transform.right * spawnOffset.x;

            Quaternion offset;

            if (characterName == "whale")
            {
                basePosition -= sideOffset; // 왼쪽
                offset = Quaternion.Euler(0f, -30f, 0f);
            }
            else
            {
                basePosition += sideOffset; // 오른쪽
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
            Debug.LogError($"캐릭터 프리팹 {characterName}을(를) 찾을 수 없습니다.");
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

        // NameBox 지우기
        characterNameText.gameObject.SetActive(false);
        characterNameBox.gameObject.SetActive(false);
    }

    public void ContinueDialogue(string characterName)
    {
        TextAsset jsonFile = Resources.Load<TextAsset>($"{characterName}_interaction");
        if (jsonFile != null)
        {
            dialogueData = JsonUtility.FromJson<DialogueData>(jsonFile.text);

            // 이전 진행 상태 기억 (PlayerPrefs나 NPC에서 전달도 가능)
            currentIndex = PlayerPrefs.GetInt($"{characterName}_dialogue_index", 0);
            if (dialogueData.dialogue_sequence[currentIndex].type == "interaction")
                currentIndex++; // interaction 건너뛰고 다음으로

            //고래 숨기기
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
            // 혹은 필요한 다른 상태도 같이 초기화 or 완료 상태로 변경
        }
    }

}
