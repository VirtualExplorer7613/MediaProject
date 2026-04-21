using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpongeCleaningModeController : MonoBehaviour
{
    public static SpongeCleaningModeController Instance { get; private set; }

    [Header("References")]
    [Tooltip("메인 카메라")]
    [SerializeField] private Camera gameplayCamera;
    [Tooltip("스펀지용 카메라")]
    [SerializeField] private Camera cleaningCamera;
    [SerializeField] private InteractionController interactionController;
    [SerializeField] private WhaleController whaleController;

    [Tooltip("모바일 UI; 청소 모드 동안 숨기고 싶으면 연결")]
    [SerializeField] private List<GameObject> gameplayUIRoots = new();

    [Tooltip("청소 완료 여부를 체크할 QuestTarget들")]
    [SerializeField] private List<QuestTarget> cleaningTargets = new();

    [Header("Options")]
    [SerializeField] private bool disableWhaleControllerDuringCleaning = true;
    [SerializeField] private bool hideGameplayUI = true;
    [SerializeField] private bool autoDetectCompletion = true;
    [SerializeField] private float returnDelay = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool startOnPlay = false;

    public bool IsCleaningModeActive { get; private set; }

    // 카메라 전환할때 필요한 변수
    private Camera originalContextCamera;
    private bool gameplayCameraPrevState;
    private bool cleaningCameraPrevState;
    private List<bool> gameplayUIPrevStates = new();
    private Coroutine completionRoutine;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        if (cleaningCamera != null)
            cleaningCamera.gameObject.SetActive(false);

        if (interactionController == null)
            interactionController = FindAnyObjectByType<InteractionController>();

        if (whaleController == null)
            whaleController = FindAnyObjectByType<WhaleController>();

        if (startOnPlay)
            StartCleaningMode();
    }

    private void Update()
    {
        if (!IsCleaningModeActive) return;
        if (!autoDetectCompletion) return;

        if (AreAllTargetsCompleted())
        {
            if (completionRoutine == null)
                completionRoutine = StartCoroutine(EndCleaningModeAfterDelay());
        }
        else
        {
            if (completionRoutine != null)
            {
                StopCoroutine(completionRoutine);
                completionRoutine = null;
            }
        }
    }

    public void StartCleaningMode()
    {
        if (IsCleaningModeActive) return;

        if (interactionController == null)
        {
            Debug.LogWarning("[SpongeCleaningModeController] InteractionController is null.");
            return;
        }

        if (cleaningCamera == null)
        {
            Debug.LogWarning("[SpongeCleaningModeController] cleaningCamera is null.");
            return;
        }

        IsCleaningModeActive = true;

        // 현재 상태 백업
        gameplayCameraPrevState = gameplayCamera != null && gameplayCamera.gameObject.activeSelf;
        cleaningCameraPrevState = cleaningCamera.gameObject.activeSelf;

        // SpongeStrategy가 사용할 카메라 백업 및 교체
        originalContextCamera = interactionController.CTX.cam;
        interactionController.CTX.cam = cleaningCamera;

        // 카메라 전환
        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(false);

        cleaningCamera.gameObject.SetActive(true);

        // 플레이어 조작 제한
        if (disableWhaleControllerDuringCleaning && whaleController != null)
        {
            whaleController.SetControlLocked(true, true);
        }

        if (hideGameplayUI)
        {
            gameplayUIPrevStates.Clear();

            foreach (var uiRoot in gameplayUIRoots)
            {
                if (uiRoot == null)
                {
                    gameplayUIPrevStates.Add(false);
                    continue;
                }

                gameplayUIPrevStates.Add(uiRoot.activeSelf);
                uiRoot.SetActive(false);
            }
        }

        Debug.Log("[SpongeCleaningModeController] Cleaning mode started.");

    }

    public void EndCleaningMode()
    {
        if (!IsCleaningModeActive) return;

        IsCleaningModeActive = false;

        if (completionRoutine != null)
        {
            StopCoroutine(completionRoutine);
            completionRoutine = null;
        }

        // 카메라 복구
        if (cleaningCamera != null)
            cleaningCamera.gameObject.SetActive(false);

        if (gameplayCamera != null)
            gameplayCamera.gameObject.SetActive(true);

        // SpongeStrategy가 사용할 카메라도 원복
        if (interactionController != null)
            interactionController.CTX.cam = originalContextCamera != null ? originalContextCamera : gameplayCamera;

        // 플레이어 조작 복구
        if (disableWhaleControllerDuringCleaning && whaleController != null)
        {
            whaleController.SetControlLocked(false, false);
        }

        // UI 복구
        if (hideGameplayUI)
        {
            for (int i = 0; i < gameplayUIRoots.Count; i++)
            {
                if (gameplayUIRoots[i] == null) continue;

                bool prevState = i < gameplayUIPrevStates.Count && gameplayUIPrevStates[i];
                gameplayUIRoots[i].SetActive(prevState);
            }
        }
        Debug.Log("[SpongeCleaningModeController] Cleaning mode ended.");

    }

    public void StartCleaningModeWithTargets(List<QuestTarget> targets)
    {
        // ?? 연산자
        // targets이 null이 아닌 경우 그 값을 넣고, null인 경우 물음표 뒤의 값을 대입
        cleaningTargets = targets ?? new List<QuestTarget>();
        StartCleaningMode();
    }

    public void SetTargets(List<QuestTarget> targets)
    {
        cleaningTargets = targets ?? new List<QuestTarget>();
    }

    public void RegisterTarget(QuestTarget target)
    {
        if (target == null) return;
        if (!cleaningTargets.Contains(target))
            cleaningTargets.Add(target);
    }

    public void UnregisterTarget(QuestTarget target)
    {
        if (target == null) return;
        cleaningTargets.Remove(target);
    }

    public void ForceFinish()
    {
        EndCleaningMode();
    }

    public void CheckAndFinishIfCompleted()
    {
        if (!IsCleaningModeActive) return;
        if (!AreAllTargetsCompleted()) return;

        if (completionRoutine == null)
            completionRoutine = StartCoroutine(EndCleaningModeAfterDelay());
    }

    private IEnumerator EndCleaningModeAfterDelay()
    {
        yield return new WaitForSeconds(returnDelay);
        EndCleaningMode();
        InteractionSystem.Instance?.NotifyInteractionComplete();
        completionRoutine = null;
    }

    private bool AreAllTargetsCompleted()
    {
        cleaningTargets.RemoveAll(t => t == null);

        if (cleaningTargets.Count == 0)
            return false;

        foreach (var target in cleaningTargets)
        {
            if (target != null && !target.Completed)
                return false;
        }

        return true;
    }
}
