using System.Collections;
using TMPro;
using UnityEngine;

public class EndingPortalController : MonoBehaviour
{
    public static EndingPortalController Instance { get; private set; }

    [Header("Portal")]
    [SerializeField] private GameObject portalVisualRoot;
    [SerializeField] private Collider portalTrigger;
    [SerializeField] private string playerTag = "Player";

    [Header("Scene")]
    [SerializeField] private Define.Scene sceneToLoad = Define.Scene.EndingDark;
    [SerializeField] private bool useLoadingScreen = false;

    [Header("Beluga Timing")]
    [SerializeField] private bool waitForDialogueVoiceBeforeBeluga = true;
    [SerializeField] private float belugaStartSafetyDelay = 0.1f;
    [SerializeField] private float maxWaitForDialogueVoice = 10f;

    [Header("Portal Sound")]
    [SerializeField] private AudioSource sfxAudioSource;
    [SerializeField] private AudioClip portalAppearSfx;

    [Header("Beluga Voice")]
    [SerializeField] private AudioSource voiceAudioSource;
    [SerializeField] private AudioClip belugaVoiceClip;

    [Header("Beluga Text UI")]
    [SerializeField] private GameObject belugaTextRoot;
    [SerializeField] private TMP_Text belugaText;

    [TextArea(2, 4)]
    [SerializeField]
    private string belugaLine =
        "고래야. 이쪽이야...!";

    [Header("Beluga Timing After Portal")]
    [SerializeField] private float textDelay = 1f;
    [SerializeField] private bool autoHideText = false;
    [SerializeField] private float textVisibleSeconds = 4f;

    private bool portalOpened = false;
    private bool sceneLoading = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (portalVisualRoot != null)
            portalVisualRoot.SetActive(false);

        if (portalTrigger != null)
            portalTrigger.enabled = false;

        if (belugaTextRoot != null)
            belugaTextRoot.SetActive(false);
    }

    /* public void OpenPortal()
     {
         if (portalOpened)
             return;

         portalOpened = true;

         if (portalVisualRoot != null)
         {
             portalVisualRoot.SetActive(true);
             RestartPortalVfx();
         }


         if (portalTrigger != null)
             portalTrigger.enabled = true;

         if (sfxAudioSource != null && portalAppearSfx != null)
             sfxAudioSource.PlayOneShot(portalAppearSfx);

         StartCoroutine(ShowBelugaTextRoutine());

         Debug.Log("[EndingPortal] 포탈 생성 완료");
     }*/

    public void OpenPortal()
    {
        if (portalOpened)
            return;

        portalOpened = true;

        StartCoroutine(OpenPortalRoutine());
    }

    private IEnumerator OpenPortalRoutine()
    {
        // 1. 문어/고래 마지막 음성 종료까지 대기
        if (waitForDialogueVoiceBeforeBeluga && DialogueManager.Instance != null)
        {
            float waitTime = DialogueManager.Instance.GetRemainingDialogueVoiceTime();

            if (maxWaitForDialogueVoice > 0f)
                waitTime = Mathf.Min(waitTime, maxWaitForDialogueVoice);

            if (waitTime > 0f)
            {
                Debug.Log($"[EndingPortal] 기존 대사 음성 종료 대기: {waitTime:F2}초");
                yield return new WaitForSecondsRealtime(waitTime + belugaStartSafetyDelay);
            }
        }

        // 2. 포탈 등장
        ShowPortal();

        // 3. 포탈 등장 후 커스텀 딜레이
        if (textDelay > 0f)
            yield return new WaitForSecondsRealtime(textDelay);

        // 4. 벨루가 대사 출력
        ShowBelugaText();
    }

    private void ShowPortal()
    {
        if (portalVisualRoot != null)
        {
            portalVisualRoot.SetActive(true);
            RestartPortalVfx();
        }

        if (portalTrigger != null)
            portalTrigger.enabled = true;

        if (sfxAudioSource != null && portalAppearSfx != null)
            sfxAudioSource.PlayOneShot(portalAppearSfx);

        Debug.Log("[EndingPortal] 포탈 생성 완료");
    }

    private void ShowBelugaText()
    {
        if (belugaTextRoot != null)
            belugaTextRoot.SetActive(true);

        if (belugaText != null)
            belugaText.text = belugaLine;

        if (voiceAudioSource != null && belugaVoiceClip != null)
            voiceAudioSource.PlayOneShot(belugaVoiceClip);

        if (autoHideText)
            StartCoroutine(HideBelugaTextAfterDelay());
    }

    private IEnumerator HideBelugaTextAfterDelay()
    {
        yield return new WaitForSecondsRealtime(textVisibleSeconds);

        if (belugaTextRoot != null)
            belugaTextRoot.SetActive(false);
    }

    private void RestartPortalVfx()
    {
        if (portalVisualRoot == null)
            return;

        ParticleSystem[] particles = portalVisualRoot.GetComponentsInChildren<ParticleSystem>(true);

        foreach (ParticleSystem ps in particles)
        {
            ps.Clear(true);
            ps.Play(true);
        }
    }

    /*private IEnumerator ShowBelugaTextRoutine()
    {
        if (textDelay > 0f)
            yield return new WaitForSeconds(textDelay);

        if (belugaTextRoot != null)
            belugaTextRoot.SetActive(true);

        if (belugaText != null)
            belugaText.text = belugaLine;

        if (voiceAudioSource != null && belugaVoiceClip != null)
            voiceAudioSource.PlayOneShot(belugaVoiceClip);

        if (autoHideText)
        {
            yield return new WaitForSeconds(textVisibleSeconds);

            if (belugaTextRoot != null)
                belugaTextRoot.SetActive(false);
        }
    }*/

    private IEnumerator ShowBelugaTextRoutine()
    {
        if (waitForDialogueVoiceBeforeBeluga && DialogueManager.Instance != null)
        {
            float waitTime = DialogueManager.Instance.GetRemainingDialogueVoiceTime();

            if (maxWaitForDialogueVoice > 0f)
                waitTime = Mathf.Min(waitTime, maxWaitForDialogueVoice);

            if (waitTime > 0f)
            {
                Debug.Log($"[EndingPortal] 기존 대사 음성 종료 대기: {waitTime:F2}초");
                yield return new WaitForSecondsRealtime(waitTime + belugaStartSafetyDelay);
            }
        }

        if (textDelay > 0f)
            yield return new WaitForSecondsRealtime(textDelay);

        if (belugaTextRoot != null)
            belugaTextRoot.SetActive(true);

        if (belugaText != null)
            belugaText.text = belugaLine;

        if (voiceAudioSource != null && belugaVoiceClip != null)
            voiceAudioSource.PlayOneShot(belugaVoiceClip);

        if (autoHideText)
        {
            yield return new WaitForSecondsRealtime(textVisibleSeconds);

            if (belugaTextRoot != null)
                belugaTextRoot.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!portalOpened)
            return;

        if (sceneLoading)
            return;

        if (!IsPlayer(other))
            return;

        sceneLoading = true;

        if (portalTrigger != null)
            portalTrigger.enabled = false;

        Debug.Log("[EndingPortal] 플레이어가 포탈에 진입. Ending Dark Scene으로 이동");

        Managers.Scene.LoadSceneAsync(sceneToLoad, useLoadingScreen);
    }

    private bool IsPlayer(Collider other)
    {
        if (other.CompareTag(playerTag))
            return true;

        if (other.transform.root != null && other.transform.root.CompareTag(playerTag))
            return true;

        return false;
    }
}