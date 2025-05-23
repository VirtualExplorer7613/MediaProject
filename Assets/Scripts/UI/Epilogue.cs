using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Epliogue : BaseScene
{
    [Header("Epliogue Settings")]
    public Image epliogueImage; // 이미지를 표시할 UI 컴포넌트
    public TextMeshProUGUI epliogueText;  // 텍스트를 표시할 UI 컴포넌트



    public List<VisualTextStep> steps;
/*    public List<Sprite> imageList; //  이미지 리스트
    public List<string> textList;  //  텍스트 리스트*/
    public float typingSpeed = 0.09f; // 타이핑 속도
    //public GameObject skipButton;

    FadeController fade;
    bool isTransitioning;


    [Header("Next Scene")]
    [SerializeField]
    private Define.Scene sceneName;

    private int currentIndex = 0; // 현재 표시 중인 이미지/텍스트 인덱스
    private bool isTyping = false; // 현재 텍스트 타이핑 중인지 확인
    private bool isMoving = false;  // 씬 이동을 진행하는지 확인
    private const float baseWidth = 1920; // 기준 가로 크기
    private const float baseHeight = 880; // 기준 세로 크기


    private void Start()
    {

        //skipButton?.SetActive(false);

        if (steps == null || steps.Count == 0)
        {
            Debug.Log("step list is empty, assigning default");

            steps.Add(new VisualTextStep
            {
                image = null, // 혹시 기본 이미지가 있다면 넣어도 OK
                text =
                    "오랜만에 돌아온 내 고향 바다…\n" +
                    "그런데 왜인지, 느낌이 다른 걸?\n\n" +

                    "바다는 예전 모습 그대로인데,\n" +
                    "기억 속 그 느낌이 아니야.\n" +

                    "예전엔 이쯤에서 반가운 소리들이 들렸는데…\n\n" +

                    "내가 없는 동안 무슨 일이 있었던 걸까?\n" +
                    "직접, 확인해봐야겠다."
            });
        }
        DisplayCurrentStep();
    }

    private void Update()
    {
        #region 페이드 없는 버전
        // 입력 감지: 클릭 또는 스페이스바
        /*if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !isTyping)
        {
            ShowNextStep();
        }*/
        #endregion

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
       && !isTyping && !isTransitioning)
        {
            StartCoroutine(AdvanceStepWithFade());
        }



    }

    IEnumerator AdvanceStepWithFade()
    {
        isTransitioning = true;                 // 입력 막기

        FadeController fade = FindObjectOfType<FadeController>();
        if (fade != null)                       // ① 암전 3 초
            yield return fade.FadeOut(2.0f);

        ShowNextStep();                         // ② 이미지·텍스트 교체

        if (fade != null)                       // ③ 밝아짐 3 초
            yield return fade.FadeIn(1.5f);

        isTransitioning = false;                // 입력 허용
    }


    private void DisplayCurrentStep()
    {
        if (currentIndex >= steps.Count)
            return;

         VisualTextStep step = steps[currentIndex];

        // 이미지 출력
        if (step.image != null)
        {
            epliogueImage.sprite = step.image;
            epliogueImage.gameObject.SetActive(true);
            AdjustImageSize();
        }
        else
        {
            epliogueImage.gameObject.SetActive(false);
        }

        // 텍스트 출력
        if (!string.IsNullOrEmpty(step.text))
        {
            StartCoroutine(TypeText(step.text));
        }
        else
        {
            epliogueText.text = "";
            isTyping = false;
        }
    }

    private void ShowNextStep()
    {
        currentIndex++;
        if (currentIndex < steps.Count)
        {
            DisplayCurrentStep();
        }
        else if (!isMoving)
        {
            StartCoroutine(GoToTitleWithFade());
            isMoving = true;
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        epliogueText.text = ""; // 기존 텍스트 지우기

        int i = 0;
        while (i < text.Length)
        {
            // 만약 줄바꿈 문자 발견하면
            if (text[i] == '\\' && i + 1 < text.Length && text[i + 1] == 'n')
            {
                epliogueText.text += '\n'; // 줄바꿈 추가
                i += 2; // 두 글자 넘기기
                yield return new WaitForSeconds(typingSpeed * 2f);
                continue; // 줄바꿈일때 살짝
            }

            epliogueText.text += text[i];
            i++;

            // 띄어쓰기일 경우 빠르게, 아니면 기본 속도
            if (epliogueText.text[^1] == ' ')
                yield return new WaitForSeconds(typingSpeed * 0.3f); // 띄어쓰기면 더 빠르게
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndEpilogue()
    {
        Debug.Log("에필로그 종료");
        // 필요 시 다음 씬으로 전환하거나 다른 이벤트 실행
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Managers.Scene.LoadSceneAsync(Define.Scene.Title, false);
    }

    public void SkipEpilogue()
    {
        EndEpilogue();
    }

    private IEnumerator GoToTitleWithFade()
    {
        isTransitioning = true;

        FadeController fade = FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeOut(2.0f); // 마지막 페이드 아웃

        EndEpilogue();
    }

    /* private IEnumerator LoadNextSceneAfterFade()
     {
         if (fadeOutAudio != null)
             yield return new WaitForSeconds(fadeOutAudio.fadeDuration);

         mySceneManager.ChangeScene(nextScene);
     }*/


    private void AdjustImageSize()
    {
        RectTransform rectTransform = epliogueImage.GetComponent<RectTransform>();
        Sprite currentSprite = epliogueImage.sprite;

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

    IEnumerator SimpleFadeOutChange(float time = 3f)
    {
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null) yield return fade.FadeOut(time);

    }

    IEnumerator SimpleFadeInChange(float time = 3f)
    {
        yield return null;
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeIn(time);
    }

    public override void Clear()
    {

    }
}
