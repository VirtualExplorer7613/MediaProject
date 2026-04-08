using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prologue : BaseScene
{
    [Header("Prologue Settings")]
    public Image prologueImage; // 이미지를 표시할 UI 컴포넌트
    public TextMeshProUGUI prologueText;  // 텍스트를 표시할 UI 컴포넌트
    public List<Sprite> imageList; // 프롤로그 이미지 리스트
    public List<string> textList;  // 프롤로그 텍스트 리스트
    public float typingSpeed = 0.09f; // 타이핑 속도
    //public GameObject skipButton;
    public GameObject space;

    // [새로 추가된 부분 1] 보이스 재생을 위한 변수 추가
    [Header("Voice Settings")]
    public AudioSource audioSource; // 음성을 재생할 오디오 소스 컴포넌트
    public List<AudioClip> voiceList; // 텍스트 순서에 맞춘 보이스 파일 리스트
    // ----------------------------------------------------

    [Header("Auto Proceed")]
    [SerializeField] float autoProceedDelay = 4f;


    [Header("Next Scene")]
    [SerializeField]
    private Define.Scene sceneName;

    private int currentIndex = 0; // 현재 표시 중인 이미지/텍스트 인덱스
    private bool isTyping = false; // 현재 텍스트 타이핑 중인지 확인
    private bool isMoving = false;  // 씬 이동을 진행하는지 확인
    private const float baseWidth = 960f; // 기준 가로 크기
    private const float baseHeight = 600; // 기준 세로 크기


    private void Start()
    {

        //skipButton?.SetActive(false);
        space?.SetActive(false);

        if (textList.Count == 0)
        {
            Debug.Log("text List is null!");
            textList = new List<string>
            {
                "오랜만에 돌아온 내 고향 바다…\n" +
                "그런데 왜인지, 느낌이 다른 걸?\n\n" +

                "바다는 예전 모습 그대로인데,\n" +
                "기억 속 그 느낌이 아니야.\n" +

                "예전엔 이쯤에서 반가운 소리들이 들렸는데…\n\n" +

                "내가 없는 동안 무슨 일이 있었던 걸까?\n" +
                "직접, 확인해봐야겠다."
            };
        }
        DisplayCurrentStep();
    }

    private void Update()
    {
        // 입력 감지: 클릭 또는 스페이스바
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !isTyping)
        {
            ShowNextStep();
        }
    }

    private void DisplayCurrentStep()
    {
        space?.SetActive(false);
        // 이미지 업데이트
        if (currentIndex < imageList.Count)
        {
            prologueImage.sprite = imageList[currentIndex];
            AdjustImageSize();
        }

        // [새로 추가된 부분 2] 텍스트가 뜰 때 해당 인덱스의 보이스 재생
        if (audioSource != null)
        {
            audioSource.Stop(); // 유저가 스페이스바를 빨리 눌러서 넘겼을 때 이전 대사 끊어주기

            // 현재 인덱스에 해당하는 보이스 파일이 있는지 확인 후 재생
            if (currentIndex < voiceList.Count && voiceList[currentIndex] != null)
            {
                audioSource.clip = voiceList[currentIndex];
                audioSource.Play();
            }
        }
        // ----------------------------------------------------

        // 텍스트 타이핑 효과 시작
        if (currentIndex < textList.Count)
            StartCoroutine(TypeText(textList[currentIndex]));
    }

    private void ShowNextStep()
    {
        currentIndex++;
        if (currentIndex > 0)
        {
            //skipButton?.SetActive(true);
        }
        // 다음 스텝이 있는 경우 표시
        if (currentIndex < Mathf.Min(imageList.Count, textList.Count))
        {
            DisplayCurrentStep();
        }
        else if (!isMoving)
        {
            EndPrologue();
            isMoving = true;
        }
    }

    // ==== [여기서부터 수정된 TypeText 코루틴] ====
    private IEnumerator TypeText(string text)
    {
        isTyping = true;

        // 인스펙터에서 작성한 \n 기호를 실제 줄바꿈으로 인식하도록 안전하게 변환
        string parsedText = text.Replace("\\n", "\n");

        // 1. 텍스트 전체를 먼저 넣고 화면 업데이트 강제 실행 (태그를 색상으로 파싱)
        prologueText.text = parsedText;
        prologueText.maxVisibleCharacters = 0; // 글자 숨기기
        prologueText.ForceMeshUpdate();

        // 2. 태그(<color> 등)를 제외한 '실제 화면에 보이는 글자 수' 가져오기
        int totalVisibleCharacters = prologueText.textInfo.characterCount;

        // 3. 한 글자씩 보이게 처리
        for (int i = 0; i < totalVisibleCharacters; i++)
        {
            prologueText.maxVisibleCharacters = i + 1;

            // 방금 출력된 글자가 무엇인지 확인 (기존 딜레이 로직 유지를 위해)
            char c = prologueText.textInfo.characterInfo[i].character;

            if (c == '\n')
            {
                yield return new WaitForSeconds(typingSpeed * 2f); // 줄바꿈일때 살짝
            }
            else if (c == ' ')
            {
                yield return new WaitForSeconds(typingSpeed * 0.3f); // 띄어쓰기면 더 빠르게
            }
            else
            {
                yield return new WaitForSeconds(typingSpeed); // 기본 속도
            }
        }

        isTyping = false;

        space?.SetActive(true);

        // 타이핑이 끝나면 자동으로 씬 이동
        if (currentIndex >= Mathf.Min(imageList.Count, textList.Count) - 1 && !isMoving)
        {
            StartCoroutine(AutoEndAfterDelay());
        }
    }
    // ==== [수정된 부분 끝] ====

    private void EndPrologue()
    {
        Debug.Log("프롤로그 종료");

        // [새로 추가된 부분 3] 씬이 넘어갈 때 재생 중인 보이스 정지
        if (audioSource != null)
        {
            audioSource.Stop();
        }
        // ----------------------------------------------------

        // 필요 시 다음 씬으로 전환하거나 다른 이벤트 실행
        //Managers.Scene.LoadScene(sceneName);
        space?.SetActive(false);
        Managers.Scene.LoadSceneAsync(sceneName, false);
    }

    public void SkipPrologue()
    {
        EndPrologue();
    }

    private void AdjustImageSize()
    {
        RectTransform rectTransform = prologueImage.GetComponent<RectTransform>();
        Sprite currentSprite = prologueImage.sprite;

        if (currentSprite != null)
        {
            // 이미지의 원본 비율 계산
            float imageWidth = currentSprite.rect.width;
            float imageHeight = currentSprite.rect.height;
            float aspectRatio = imageWidth / imageHeight;

            // 기준 크기와 비교하여 긴 부분에 맞춤
            if (aspectRatio >= baseWidth / baseHeight)
            {
                // 가로가 기준보다 길거나 같을 경우
                rectTransform.sizeDelta = new Vector2(baseWidth, baseWidth / aspectRatio);
            }
            else
            {
                // 세로가 기준보다 길 경우
                rectTransform.sizeDelta = new Vector2(baseHeight * aspectRatio, baseHeight);
            }
        }
    }

    IEnumerator AutoEndAfterDelay()
    {
        yield return new WaitForSeconds(autoProceedDelay);
        if (!isMoving)            // 중복 호출 방지
        {
            isMoving = true;
            EndPrologue();
        }
    }

    public override void Clear()
    {

    }
}