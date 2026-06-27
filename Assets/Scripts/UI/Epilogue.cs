using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Epliogue : BaseScene
{
    [Header("Epliogue Settings")]
    public Image epliogueImage; // 이미지를 표시할 UI 컴포넌트
    public TextMeshProUGUI epliogueText;  // 텍스트를 표시할 UI 컴포넌트
    public GameObject space;

    [Header("Additional Text Layouts")]
    [SerializeField] private TextMeshProUGUI dialogueText;  // 짧은 대화용 TMP
    [SerializeField] private TextMeshProUGUI monologueText; // 긴 독백용 TMP

    public List<VisualTextStep> steps;
/*    public List<Sprite> imageList; //  이미지 리스트
    public List<string> textList;  //  텍스트 리스트*/
    public float typingSpeed = 0.09f; // 타이핑 속도
    //public GameObject skipButton;

    FadeController fade;
    bool isTransitioning;

    [Header("Proceed")]
    [Tooltip("타이핑이 끝난 뒤, 입력을 허용하기 전 최소 대기 시간")]
    [SerializeField] private float defaultMinReadTime = 0.5f;

    private float canProceedAt = float.PositiveInfinity;

    [Header("Auto-Advance")]
    [Tooltip("켜면 기존처럼 자동으로 다음 컷으로 넘어감. 수동 진행만 원하면 OFF")]
    [SerializeField] private bool useAutoAdvance = false;

    [Tooltip("입력 가능 상태가 된 뒤 자동 진행까지 기다리는 시간")]
    [SerializeField] float autoAdvanceDelay = 3f;

    Coroutine autoAdvanceCo;
    Coroutine typingCo;
    Coroutine readyIndicatorCo;


    [Header("Next Scene")]
    [SerializeField]
    private Define.Scene sceneName;

    private int currentIndex = 0; // 현재 표시 중인 이미지/텍스트 인덱스
    private bool isTyping = false; // 현재 텍스트 타이핑 중인지 확인
    private bool isMoving = false;  // 씬 이동을 진행하는지 확인
    
    // private const float baseWidth = 1920; // 기준 가로 크기
    // private const float baseHeight = 880; // 기준 세로 크기

    private enum EpilogueImageFitMode
    {
        FixedLetterboxCrop, // 위아래 레터박스 유지 + 이미지 일부 크롭
        FullscreenCover,   // 화면 전체 채움 + 레터박스 없음
        Contain            // 이미지 전체 표시, 대신 비율에 따라 여백 생김
    }

    [Header("Illustration Fit")]
    [SerializeField] private EpilogueImageFitMode imageFitMode = EpilogueImageFitMode.FixedLetterboxCrop;

    [Tooltip("화면 높이 중 이미지가 차지할 비율. 1920x1080에서 880px 높이를 원하면 880/1080 = 0.815")]
    [SerializeField, Range(0.5f, 1f)] private float letterboxContentHeightRatio = 880f / 1080f;

    [SerializeField] private RectTransform imageViewport;

    private TextMeshProUGUI activeText;

    private void Start()
    {

        //skipButton?.SetActive(false);
        space?.SetActive(false);      // 첫 진입 시 OFF
        if (steps == null || steps.Count == 0)
        {
            Debug.Log("step list is empty, assigning default");

            steps.Add(new VisualTextStep
            {
                layout = EpilogueStepLayout.Monologue,
                image = null,
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

        /* if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        && !isTyping && !isTransitioning && !isMoving)
         {
             if (currentIndex >= steps.Count - 1)
             {
                 isMoving = true;
                 EndEpilogue();
                 //StartCoroutine(GoToTitleWithFade());
             }
             else
             {
                 StartCoroutine(AdvanceStepWithFade());
             }
         }*/

        if (!HasProceedInput())
            return;

        if (!CanProceedNow())
            return;

        ProceedToNextOrEnd();

    }

    private bool HasProceedInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            return true;

        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                if (IsPointerOverUI(touch.fingerId))
                    return false;

                return true;
            }
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (IsPointerOverUI())
                return false;

            return true;
        }

        return false;
    }

    private bool CanProceedNow()
    {
        return !isTyping
            && !isTransitioning
            && !isMoving
            && Time.time >= canProceedAt;
    }

    private void ProceedToNextOrEnd()
    {
        space?.SetActive(false);

        if (currentIndex >= steps.Count - 1)
        {
            isMoving = true;
            EndEpilogue();
        }
        else
        {
            StartCoroutine(AdvanceStepWithFade());
        }
    }

    IEnumerator AdvanceStepWithFade()
    {
        if (isMoving) yield break;

        isTransitioning = true;                 // 입력 막기

        // [추가된 로직] 다음으로 넘어갈 스텝이 Illustration 레이아웃인지 확인
        bool shouldFade = false;
        int nextIndex = currentIndex + 1;
        if (nextIndex < steps.Count)
        {
            if (steps[nextIndex].layout == EpilogueStepLayout.Illustration)
            {
                shouldFade = true; // 다음 스텝이 일러스트일 때만 페이드 효과 켜기
            }
        }

        FadeController fade = null;

        // shouldFade가 true일 때만 화면 암전
        if (shouldFade)
        {
            fade = FindObjectOfType<FadeController>();
            if (fade != null)
                yield return fade.FadeOut(2.0f);
        }

        ShowNextStep();                         // ② 이미지·텍스트 교체

        // shouldFade가 true일 때만 화면 다시 밝아짐
        if (shouldFade)
        {
            if (fade != null)
                yield return fade.FadeIn(2.0f);
        }

        isTransitioning = false;                // 입력 허용
    }

    #region 기존 버전 Display
    /*private void DisplayCurrentStep()
    {
        space?.SetActive(false);

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
            space?.SetActive(true);
            if (space != null)
            {
                Floating floating = space.GetComponent<Floating>();
                floating.setFrequency(0f);
            }
        }

        if (autoAdvanceCo != null)
        {
            StopCoroutine(autoAdvanceCo);
        }
        autoAdvanceCo = StartCoroutine(AutoAdvanceRoutine(currentIndex));
    }
*/
    #endregion

    private void DisplayCurrentStep()
    {
        space?.SetActive(false);

        StopRunningStepCoroutines();

        if (currentIndex >= steps.Count)
            return;

        VisualTextStep step = steps[currentIndex];

        ApplyStepLayout(step);

        canProceedAt = float.PositiveInfinity;

        if (!string.IsNullOrEmpty(step.text) && activeText != null)
        {
            typingCo = StartCoroutine(TypeText(activeText, step.text, currentIndex));
        }
        else
        {
            if (activeText != null)
                activeText.text = "";

            isTyping = false;
            canProceedAt = Time.time + GetMinReadTime(step);
        }

        readyIndicatorCo = StartCoroutine(ShowReadyIndicatorRoutine(currentIndex));

        if (useAutoAdvance)
            autoAdvanceCo = StartCoroutine(AutoAdvanceRoutine(currentIndex));
    }

    private void ApplyStepLayout(VisualTextStep step)
    {
        HideAllTexts();

        activeText = GetTextByLayout(step.layout);

        if (activeText != null)
        {
            activeText.text = "";
            activeText.gameObject.SetActive(true);
        }

        bool shouldShowImage =
            step.layout == EpilogueStepLayout.Illustration &&
            step.image != null;

        if (epliogueImage != null)
        {
            if (shouldShowImage)
            {
                epliogueImage.sprite = step.image;
                epliogueImage.gameObject.SetActive(true);
                AdjustImageSize();
            }
            else
            {
                epliogueImage.gameObject.SetActive(false);
            }
        }
    }

    private TextMeshProUGUI GetTextByLayout(EpilogueStepLayout layout)
    {
        switch (layout)
        {
            case EpilogueStepLayout.Dialogue:
                return dialogueText != null ? dialogueText : epliogueText;

            case EpilogueStepLayout.Monologue:
                return monologueText != null ? monologueText : epliogueText;

            case EpilogueStepLayout.Illustration:
            default:
                return epliogueText;
        }
    }

    private void HideAllTexts()
    {
        SetTextVisible(epliogueText, false);
        SetTextVisible(dialogueText, false);
        SetTextVisible(monologueText, false);
    }

    private void SetTextVisible(TextMeshProUGUI text, bool visible)
    {
        if (text == null) return;
        text.gameObject.SetActive(visible);
    }

    private void ShowNextStep()
    {
        currentIndex++;
        if (currentIndex < steps.Count)
        {
            DisplayCurrentStep();
        }
    }

    private void StopRunningStepCoroutines()
    {
        if (typingCo != null)
        {
            StopCoroutine(typingCo);
            typingCo = null;
        }

        if (readyIndicatorCo != null)
        {
            StopCoroutine(readyIndicatorCo);
            readyIndicatorCo = null;
        }

        if (autoAdvanceCo != null)
        {
            StopCoroutine(autoAdvanceCo);
            autoAdvanceCo = null;
        }
    }

    /*private IEnumerator TypeText(TextMeshProUGUI target, string text, int stepIndex)
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
        space?.SetActive(false);
        *//*if (space != null)
        {
            Floating floating = space.GetComponent<Floating>();
            floating.setFrequency(floating.getOriginalFrequency());
        }*//*
    }*/

    private IEnumerator TypeText(TextMeshProUGUI target, string text, int stepIndex)
    {
        isTyping = true;
        target.text = "";

        text = text.Replace("\\n", "\n");

        for (int i = 0; i < text.Length; i++)
        {
            if (stepIndex != currentIndex)
                yield break;

            char c = text[i];
            target.text += c;

            if (c == '\n')
            {
                yield return new WaitForSeconds(typingSpeed * 2f);
            }
            else if (c == ' ')
            {
                yield return new WaitForSeconds(typingSpeed * 0.3f);
            }
            else
            {
                yield return new WaitForSeconds(typingSpeed);
            }
        }

        isTyping = false;

        if (stepIndex >= 0 && stepIndex < steps.Count)
            canProceedAt = Time.time + GetMinReadTime(steps[stepIndex]);
        else
            canProceedAt = Time.time + defaultMinReadTime;
    }

    private void EndEpilogue()
    {
        Debug.Log("에필로그 종료");
        // 필요 시 다음 씬으로 전환하거나 다른 이벤트 실행
        space?.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Managers.Scene.LoadSceneAsync(Define.Scene.Title, false);
    }

    private IEnumerator ShowReadyIndicatorRoutine(int stepIndex)
    {
        yield return new WaitUntil(() =>
            stepIndex != currentIndex ||
            (!isTyping && Time.time >= canProceedAt)
        );

        if (stepIndex != currentIndex) yield break;
        if (isMoving || isTransitioning) yield break;

        space?.SetActive(true);
    }

    public void SkipEpilogue()
    {
        EndEpilogue();
    }

    /*private IEnumerator GoToTitleWithFade()
    {
        isTransitioning = true;

        if (autoAdvanceCo != null)
        {
            StopCoroutine(autoAdvanceCo);
            autoAdvanceCo = null;
        }

        FadeController fade = FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeOut(2.0f); // 마지막 페이드 아웃

        EndEpilogue();
    }*/

    /* private IEnumerator LoadNextSceneAfterFade()
     {
         if (fadeOutAudio != null)
             yield return new WaitForSeconds(fadeOutAudio.fadeDuration);

         mySceneManager.ChangeScene(nextScene);
     }*/

    #region 5번째 빌드 전 함수
    /*private void AdjustImageSize()
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
    }*/
    #endregion

    private void AdjustImageSize()
    {
        if (epliogueImage == null || epliogueImage.sprite == null)
            return;

        RectTransform imageRect = epliogueImage.rectTransform;

        RectTransform parentRect = imageViewport != null
            ? imageViewport.parent as RectTransform
            : imageRect.parent as RectTransform;

        if (parentRect == null)
            return;

        float screenW = parentRect.rect.width;
        float screenH = parentRect.rect.height;

        if (screenW <= 0f || screenH <= 0f)
        {
            screenW = Screen.width;
            screenH = Screen.height;
        }

        float spriteW = epliogueImage.sprite.rect.width;
        float spriteH = epliogueImage.sprite.rect.height;

        float targetW = screenW;
        float targetH = screenH;

        if (imageFitMode == EpilogueImageFitMode.FixedLetterboxCrop)
        {
            if (imageViewport == null)
            {
                Debug.LogWarning("[Epliogue] FixedLetterboxCrop을 쓰려면 imageViewport를 연결하는 것을 권장합니다.");
            }
            else
            {
                imageViewport.anchorMin = new Vector2(0f, 0.5f);
                imageViewport.anchorMax = new Vector2(1f, 0.5f);
                imageViewport.pivot = new Vector2(0.5f, 0.5f);
                imageViewport.anchoredPosition = Vector2.zero;

                targetH = screenH * letterboxContentHeightRatio;
                imageViewport.sizeDelta = new Vector2(0f, targetH);

                if (imageViewport.GetComponent<RectMask2D>() == null)
                    imageViewport.gameObject.AddComponent<RectMask2D>();
            }

            targetW = screenW;
            targetH = screenH * letterboxContentHeightRatio;
        }

        float scale;

        switch (imageFitMode)
        {
            case EpilogueImageFitMode.FixedLetterboxCrop:
            case EpilogueImageFitMode.FullscreenCover:
                // 영역을 꽉 채움. 비율이 안 맞으면 일부 크롭됨.
                scale = Mathf.Max(targetW / spriteW, targetH / spriteH);
                break;

            case EpilogueImageFitMode.Contain:
            default:
                // 이미지 전체 표시. 대신 여백이 생길 수 있음.
                scale = Mathf.Min(targetW / spriteW, targetH / spriteH);
                break;
        }

        imageRect.anchorMin = new Vector2(0.5f, 0.5f);
        imageRect.anchorMax = new Vector2(0.5f, 0.5f);
        imageRect.pivot = new Vector2(0.5f, 0.5f);
        imageRect.anchoredPosition = Vector2.zero;
        imageRect.sizeDelta = new Vector2(spriteW * scale, spriteH * scale);
    }

    IEnumerator AutoAdvanceRoutine(int stepIndex)
    {
        /* 1. 타이핑이 끝날 때까지 대기 */
        yield return new WaitUntil(() => !isTyping);

        /* 2. 읽을 시간을 줌 */
        yield return new WaitForSeconds(autoAdvanceDelay);

        if (stepIndex != currentIndex) yield break;
        if (isTransitioning || isTyping || isMoving) yield break;

        /* 3. 아직 같은 스텝이고, 페이드/타이핑 중이 아니면 자동 진행 */
        /*if (stepIndex == currentIndex && !isTransitioning && !isTyping)
        {
            StartCoroutine(AdvanceStepWithFade());
        }*/

        // 마지막 컷이면 일반 컷 전환이 아니라 타이틀 이동
        if (currentIndex >= steps.Count - 1)
        {
            isMoving = true;
            EndEpilogue();
        }
        else
        {
            StartCoroutine(AdvanceStepWithFade());
        }
    }

    private float GetMinReadTime(VisualTextStep step)
    {
        if (step.minReadTime >= 0f)
            return step.minReadTime;

        return defaultMinReadTime;
    }

    IEnumerator SimpleFadeOutChange(float time = 3f)
    {
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null) yield return fade.FadeOut(time);

    }

    private bool IsPointerOverUI(int fingerId = -1)
    {
        if (EventSystem.current == null)
            return false;

        if (fingerId >= 0)
            return EventSystem.current.IsPointerOverGameObject(fingerId);

        return EventSystem.current.IsPointerOverGameObject();
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
