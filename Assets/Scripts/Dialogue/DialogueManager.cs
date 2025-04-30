using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }
    public TalkableNPC CurrentTalkingNPC { get; private set; }

    private WhaleController whaleController;

    public TMP_Text dialogueText;         // 대화 내용을 표시할 텍스트
    public GameObject dialogueBox;    // 대화창 패널

    public Camera mainCamera;
    public Vector3 spawnOffset = new Vector3(2f, 0f, 3f); // 캐릭터 위치 오프셋 (조절 가능)
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
        if (currentCharacterModel != null)
            Destroy(currentCharacterModel);

        GameObject prefab = Resources.Load<GameObject>($"Characters/{characterName}");
        if (prefab != null)
        {
            // 기본 스폰 위치: 카메라 앞 spawnOffset 거리
            Vector3 basePosition = mainCamera.transform.position + mainCamera.transform.forward * spawnOffset.z + Vector3.up * spawnOffset.y;

            // 방향 결정: 고래는 왼쪽, 나머지는 오른쪽
            Vector3 sideOffset = mainCamera.transform.right * spawnOffset.x;
            if (characterName == "whale")
            {
                basePosition -= sideOffset; // 왼쪽
            }
            else
            {
                basePosition += sideOffset; // 오른쪽
            }

            currentCharacterModel = Instantiate(prefab, basePosition, Quaternion.LookRotation(mainCamera.transform.forward));
        }
        else
        {
            Debug.LogError($"캐릭터 프리팹 {characterName}을(를) 찾을 수 없습니다.");
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
}
