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

        if (textList.Count == 0)
        {
            Debug.Log("text List is null!");
            textList = new List<string>
            {
                "오랜만에 돌아온 내 고향 바다…\\n" +
                "그런데 왜인지, 느낌이 다른 걸?\\n\\n" +

                "바다는 예전 모습 그대로인데,\\n" +
                "기억 속 그 느낌이 아니야.\\n" +

                "예전엔 이쯤에서 반가운 소리들이 들렸는데…\\n\\n" +

                "내가 없는 동안 무슨 일이 있었던 걸까?\\n" +
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
        // 이미지 업데이트
        if (currentIndex < imageList.Count)
        {
            prologueImage.sprite = imageList[currentIndex];
            AdjustImageSize();
        }

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

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        prologueText.text = ""; // 기존 텍스트 지우기

        int i = 0;
        while (i < text.Length)
        {
            // 만약 줄바꿈 문자 발견하면
            if (text[i] == '\\' && i + 1 < text.Length && text[i + 1] == 'n')
            {
                prologueText.text += '\n'; // 줄바꿈 추가
                i += 2; // 두 글자 넘기기
                yield return new WaitForSeconds(typingSpeed * 2f);
                continue; // 줄바꿈일때 살짝
            }

            prologueText.text += text[i];
            i++;

            // 띄어쓰기일 경우 빠르게, 아니면 기본 속도
            if (prologueText.text[^1] == ' ')
                yield return new WaitForSeconds(typingSpeed * 0.3f); // 띄어쓰기면 더 빠르게
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndPrologue()
    {
        Debug.Log("프롤로그 종료");
        // 필요 시 다음 씬으로 전환하거나 다른 이벤트 실행
        //Managers.Scene.LoadScene(sceneName);
        Managers.Scene.LoadSceneAsync(sceneName, false);
    }

    public void SkipPrologue()
    {
        EndPrologue();
    }

    /* private IEnumerator LoadNextSceneAfterFade()
     {
         if (fadeOutAudio != null)
             yield return new WaitForSeconds(fadeOutAudio.fadeDuration);

         mySceneManager.ChangeScene(nextScene);
     }*/


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

    public override void Clear()
    {

    }
}
