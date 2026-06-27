using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class EndingDarkTextStep
{
    [TextArea(2, 5)]
    public string text;

    public float duration = 2f;

    public AudioClip voice;
}

public class EndingDarkSceneController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image blackBackground;
    [SerializeField] private TMP_Text centerDialogueText;
    [SerializeField] private GameObject gaugeRoot;
    [SerializeField] private Image gaugeFill;
    [SerializeField] private Button waveButton;
    [SerializeField] private GameObject waveButtonHighlight;
    [SerializeField] private RawImage waveRawImage;
    [SerializeField] private Image flashImage;

    [Header("Dialogue Steps")]
    [SerializeField] private List<EndingDarkTextStep> introSteps = new();
    [SerializeField] private List<EndingDarkTextStep> completeSteps = new();

    [Header("Voice")]
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private bool stopPreviousVoice = true;

    [Header("Wave Preview")]
    [SerializeField] private GameObject wavePrefab;
    [SerializeField] private Transform waveSpawnRoot;
    [SerializeField] private float waveDestroyDelay = 2.5f;
    [SerializeField] private Vector3 waveLocalPosition = Vector3.zero;
    [SerializeField] private Vector3 waveLocalEuler = Vector3.zero;
    [SerializeField] private Vector3 waveLocalScale = Vector3.one;

    [Header("Wave Cooldown")]
    [SerializeField] private float waveCooldown = 1.0f;
    [SerializeField] private bool useWaveCooldown = true;

    [Header("Progress")]
    [SerializeField] private float progressPerClick = 0.1f;
    [SerializeField] private float minBlackAlpha = 0.15f;

    [Header("Gauge Smooth Fill")]
    [SerializeField] private bool useSmoothGaugeFill = true;
    [SerializeField] private float gaugeFillSmoothDuration = 0.35f;
    [SerializeField] private bool smoothBlackAlphaWithGauge = true;
    [SerializeField] private bool waitGaugeFillBeforeComplete = true;
    [SerializeField] private AnimationCurve gaugeFillCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private float visualProgress = 0f;
    private Coroutine gaugeFillCoroutine;

    private float nextWaveAvailableTime = 0f;

    [Header("Timing")]
    [SerializeField] private float textGap = 0.25f;
    [SerializeField] private float completeDelay = 1.2f;
    [SerializeField] private float sceneLoadDelay = 1.2f;
    // [타이핑 효과를 위해 추가된 부분]
    [SerializeField] private float typingSpeed = 0.09f;

    [Header("SFX")]
    [SerializeField] private string noiseSfx = "SFX/Noise";
    [SerializeField] private string waveClickSfx = "Underwater_Impact_02";
    [SerializeField] private string clearSfx = "SFX/ClearLight";

    [Header("Next Scene")]
    [SerializeField] private Define.Scene nextScene = Define.Scene.KelpCaves;

    private float progress;
    private bool interactionStarted;
    private bool completed;
    private Coroutine highlightCoroutine;

    private bool sequenceStarted = false;

    private void Awake()
    {
        progress = 0f;
        visualProgress = 0f;
        interactionStarted = false;
        completed = false;

        if (gaugeRoot != null)
            gaugeRoot.SetActive(false);

        if (waveButton != null)
            waveButton.gameObject.SetActive(false);

        if (waveButtonHighlight != null)
            waveButtonHighlight.SetActive(false);

        if (centerDialogueText != null)
            centerDialogueText.gameObject.SetActive(false);

        if (gaugeFill != null)
            gaugeFill.fillAmount = visualProgress;

        if (waveRawImage != null)
            waveRawImage.gameObject.SetActive(false);

        if (voiceAudioSource == null)
            voiceAudioSource = GetComponent<AudioSource>();

        SetBlackAlpha(1f);
        SetFlashAlpha(0f);
    }

    private void Start()
    {
        if (waveButton != null)
            waveButton.onClick.AddListener(OnWaveButtonClicked);
    }

    public void BeginSequence()
    {
        if (sequenceStarted)
            return;

        sequenceStarted = true;
        StartCoroutine(PlayIntroSequence());
    }

    private void OnDestroy()
    {
        if (waveButton != null)
            waveButton.onClick.RemoveListener(OnWaveButtonClicked);
    }

    private IEnumerator PlayIntroSequence()
    {
        PlaySfx(noiseSfx);
        /*
                yield return ShowCenterText("벨루가\n (치직…) 여기야… 거의 다 왔어.", 1.8f);
                yield return ShowCenterText("벨루가\n (치직… 치지직…) 조금만 더.", 1.8f);
                yield return ShowCenterText("고래\n 분명 이 근처인 것 같은데…\n너무 깜깜해서 아무것도 안 보여.", 2.4f);
                yield return ShowCenterText("고래\n 이 곳이 온통 까만 오염으로 뒤덮여 있어서 그런가 봐.\n강한 물살을 일으켜서 오염을 걷어내 보자!", 3.0f);*/

        if (introSteps == null || introSteps.Count == 0)
        {
            SetupDefaultIntroSteps();
        }

        foreach (var step in introSteps)
        {
            yield return ShowCenterText(step);
        }


        StartInteraction();
    }

    private IEnumerator ShowCenterText(EndingDarkTextStep step)
    {
        if (centerDialogueText == null || step == null)
            yield break;

        string parsedText = string.IsNullOrEmpty(step.text)
            ? ""
            : step.text.Replace("\\n", "\n");

        centerDialogueText.text = parsedText;

        // [타이핑 효과를 위해 추가된 부분] 시작
        centerDialogueText.maxVisibleCharacters = 0;
        centerDialogueText.gameObject.SetActive(true);

        PlayVoice(step.voice);

        centerDialogueText.ForceMeshUpdate();
        int totalVisibleCharacters = centerDialogueText.textInfo.characterCount;

        for (int i = 0; i <= totalVisibleCharacters; i++)
        {
            centerDialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(typingSpeed);
        }
        // [타이핑 효과를 위해 추가된 부분] 끝

        float duration = Mathf.Max(0.1f, step.duration);
        yield return new WaitForSeconds(duration);

        centerDialogueText.gameObject.SetActive(false);
        yield return new WaitForSeconds(textGap);
    }

    private void PlayVoice(AudioClip clip)
    {
        if (voiceAudioSource == null || clip == null)
            return;

        if (stopPreviousVoice)
            voiceAudioSource.Stop();

        voiceAudioSource.clip = clip;
        voiceAudioSource.Play();
    }

    private void StartInteraction()
    {
        interactionStarted = true;

        if (gaugeRoot != null)
            gaugeRoot.SetActive(true);

        if (waveButton != null)
        {
            waveButton.gameObject.SetActive(true);
            waveButton.interactable = true;
        }

        if (waveRawImage != null)
            waveRawImage.gameObject.SetActive(true);

        if (waveButtonHighlight != null)
        {
            waveButtonHighlight.SetActive(true);
            highlightCoroutine = StartCoroutine(CoHighlight());
        }
    }

    private void OnWaveButtonClicked()
    {
        if (!interactionStarted || completed)
            return;

        if (useWaveCooldown && Time.time < nextWaveAvailableTime)
            return;

        if (useWaveCooldown)
            nextWaveAvailableTime = Time.time + waveCooldown;

        SpawnWaveVfx();

        progress += progressPerClick;
        progress = Mathf.Clamp01(progress);

        SetGaugeProgress(progress);

        //UpdateBlackBackground();

        PlaySfx(waveClickSfx);

        if (progress >= 0.999f)
        {
            progress = 1f;
            SetGaugeProgress(progress);
            StartCoroutine(CompleteSequence());
        }
    }

    private void SetGaugeProgress(float targetProgress)
    {
        targetProgress = Mathf.Clamp01(targetProgress);

        // 어둠은 즉시 걷히게 하고 싶을 때
        if (!smoothBlackAlphaWithGauge)
            UpdateBlackBackground(targetProgress);

        // 부드러운 게이지 옵션이 꺼져 있으면 즉시 반영
        if (!useSmoothGaugeFill || gaugeFill == null)
        {
            if (gaugeFillCoroutine != null)
            {
                StopCoroutine(gaugeFillCoroutine);
                gaugeFillCoroutine = null;
            }

            visualProgress = targetProgress;

            if (gaugeFill != null)
                gaugeFill.fillAmount = visualProgress;

            if (smoothBlackAlphaWithGauge)
                UpdateBlackBackground(visualProgress);

            return;
        }

        if (gaugeFillCoroutine != null)
            StopCoroutine(gaugeFillCoroutine);

        gaugeFillCoroutine = StartCoroutine(CoSmoothGaugeFill(targetProgress));
    }

    private IEnumerator CoSmoothGaugeFill(float targetProgress)
    {
        float startProgress = visualProgress;
        float elapsed = 0f;
        float duration = Mathf.Max(0.01f, gaugeFillSmoothDuration);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / duration);

            if (gaugeFillCurve != null)
                t = gaugeFillCurve.Evaluate(t);

            visualProgress = Mathf.Lerp(startProgress, targetProgress, t);

            if (gaugeFill != null)
                gaugeFill.fillAmount = visualProgress;

            if (smoothBlackAlphaWithGauge)
                UpdateBlackBackground(visualProgress);

            yield return null;
        }

        visualProgress = targetProgress;

        if (gaugeFill != null)
            gaugeFill.fillAmount = visualProgress;

        if (smoothBlackAlphaWithGauge)
            UpdateBlackBackground(visualProgress);

        gaugeFillCoroutine = null;
    }

    private void SpawnWaveVfx()
    {
        if (wavePrefab == null || waveSpawnRoot == null)
        {
            Debug.LogWarning("[EndingDarkScene] wavePrefab 또는 waveSpawnRoot가 비어 있습니다.");
            return;
        }

        Vector3 spawnPosition = waveSpawnRoot.TransformPoint(waveLocalPosition);
        Quaternion spawnRotation = waveSpawnRoot.rotation * Quaternion.Euler(waveLocalEuler);

        GameObject wave = Instantiate(wavePrefab, spawnPosition, spawnRotation);

        //wave.transform.localPosition = waveLocalPosition;
        //wave.transform.localRotation = Quaternion.Euler(waveLocalEuler);
        wave.transform.localScale = waveLocalScale;

        Destroy(wave, waveDestroyDelay);
    }

    private void UpdateBlackBackground()
    {
        /*float alpha = Mathf.Lerp(1f, minBlackAlpha, progress);
        SetBlackAlpha(alpha);*/
        UpdateBlackBackground(progress);
    }

    private void UpdateBlackBackground(float progressValue)
    {
        // [수정됨] 선형(Linear) 계산 대신 제곱(Pow) 계산을 적용하여 
        // 초반에는 아주 천천히 밝아지고, 후반에 더 많은 차이가 나도록 곡선 처리
        float curvedProgress = Mathf.Pow(Mathf.Clamp01(progressValue), 2.5f);

        float alpha = Mathf.Lerp(1f, minBlackAlpha, curvedProgress);
        SetBlackAlpha(alpha);
    }

    private IEnumerator CompleteSequence()
    {
        if (completed)
            yield break;

        completed = true;
        interactionStarted = false;

        if (waveButton != null)
            waveButton.interactable = false;

        if (waveButtonHighlight != null)
            waveButtonHighlight.SetActive(false);

        if (highlightCoroutine != null)
            StopCoroutine(highlightCoroutine);

        if (waitGaugeFillBeforeComplete)
        {
            while (gaugeFillCoroutine != null)
                yield return null;
        }

        /*if (completeSteps == null || completeSteps.Count == 0)
        {
            SetupDefaultCompleteSteps();
        }

        foreach (var step in completeSteps)
        {
            yield return ShowCenterText(step);
        }*/

        // Complete Steps가 비어 있으면 아무 대사 없이 그대로 진행
        if (completeSteps != null)
        {
            foreach (var step in completeSteps)
            {
                // 인스펙터에서 Element만 만들고 text/voice를 비워둔 경우도 스킵
                if (step == null)
                    continue;

                if (string.IsNullOrWhiteSpace(step.text) && step.voice == null)
                    continue;

                yield return ShowCenterText(step);
            }
        }

        PlaySfx(clearSfx);

        yield return CoFlash();

        yield return new WaitForSeconds(sceneLoadDelay);

        Managers.Scene.LoadSceneAsyncWithFadeColor(nextScene, Color.white, 1.0f, 1.0f);
    }

    private IEnumerator CoFlash()
    {
        float time = 0f;
        float fadeInDuration = 0.2f;
        float fadeOutDuration = 0.6f;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            SetFlashAlpha(Mathf.Lerp(0f, 1f, time / fadeInDuration));
            yield return null;
        }

        time = 0f;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            SetFlashAlpha(Mathf.Lerp(1f, 0f, time / fadeOutDuration));
            yield return null;
        }

        SetFlashAlpha(0f);
    }

    private IEnumerator CoHighlight()
    {
        CanvasGroup group = waveButtonHighlight.GetComponent<CanvasGroup>();

        if (group == null)
            group = waveButtonHighlight.AddComponent<CanvasGroup>();

        while (!completed)
        {
            float time = 0f;
            float duration = 0.6f;

            while (time < duration)
            {
                time += Time.deltaTime;
                group.alpha = Mathf.Lerp(0.35f, 1f, time / duration);
                yield return null;
            }

            time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                group.alpha = Mathf.Lerp(1f, 0.35f, time / duration);
                yield return null;
            }
        }
    }

    private void SetBlackAlpha(float alpha)
    {
        if (blackBackground == null)
            return;

        Color c = blackBackground.color;
        c.a = alpha;
        blackBackground.color = c;
    }

    private void SetFlashAlpha(float alpha)
    {
        if (flashImage == null)
            return;

        Color c = flashImage.color;
        c.a = alpha;
        flashImage.color = c;
    }

    private void PlaySfx(string sfxName)
    {
        if (string.IsNullOrEmpty(sfxName))
            return;

        Managers.Sound.Play(sfxName);
    }

    // introSteps가 비어 있어도 기존처럼 작동
    private void SetupDefaultIntroSteps()
    {
        introSteps = new List<EndingDarkTextStep>
        {
            new EndingDarkTextStep
            {
                text = "벨루가\n(치직…) 여기야… 거의 다 왔어.",
                duration = 1.8f
            },
            new EndingDarkTextStep
            {
                text = "벨루가\n (치직… 치지직…) 조금만 더.",
                duration = 1.8f
            },
            new EndingDarkTextStep
            {
                text = "고래\n분명 이 근처인 것 같은데…\n너무 깜깜해서 아무것도 안 보여.",
                duration = 2.4f
            },
            new EndingDarkTextStep
            {
                text = "고래\n이 곳이 온통 까만 오염으로 뒤덮여 있어서 그런가 봐.\n강한 물살을 일으켜서 오염을 걷어내 보자!",
                duration = 3.0f
            }
        };
    }
    private void SetupDefaultCompleteSteps()
    {
        completeSteps = new List<EndingDarkTextStep>
        {
            new EndingDarkTextStep
            {
                text = "벨루가\n아… 드디어, 이 답답한 어둠 속에서 벗어나게 됐어…",
                duration = 2.5f
            },
            //new EndingDarkTextStep
            //{
               // text = "고래\n어? 이 목소리는… 계속 나를 부르던 그 목소리잖아!",
               // duration = 2.5f
            //}
        };
    }
}