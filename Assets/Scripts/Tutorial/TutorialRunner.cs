using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialRunner : MonoBehaviour
{
    [Serializable]
    public class TargetBinding
    {
        public string id;
        public Transform target;
    }

    [Header("Steps")]
    [SerializeField] private TutorialStepAsset[] steps;

    [Header("Language")]
    [SerializeField] private GameLanguage currentLanguage = GameLanguage.Korean;

    [Header("References")]
    [SerializeField] private TutorialUI ui;
    [SerializeField] private Transform player;
    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;
    [SerializeField] private TutorialHighlighter highlighter;
    [SerializeField] private TutorialWorldVfxController worldVfxController;
    //[SerializeField] private Transform orcaTarget;

    [Header("World Targets")]
    [SerializeField] private TargetBinding[] worldTargets;

    private int index = -1;
    private TutorialCondition currentCondition;
    private TutorialContext context;

    public int CurrentStepIndex => index;

    public event Action<int> StepChanged;

    public bool IsCurrentStep(int stepIndex)
    {
        return index == stepIndex;
    }

    public bool IsStepOrLater(int stepIndex)
    {
        return index >= stepIndex;
    }


    private void Awake()
    {
        Dictionary<string, Transform> targetMap = new(StringComparer.OrdinalIgnoreCase);

        if (worldTargets != null)
        {
            foreach (var binding in worldTargets)
            {
                if (binding == null || string.IsNullOrWhiteSpace(binding.id) || binding.target == null)
                    continue;

                targetMap[binding.id] = binding.target;
            }
        }

        context = new TutorialContext
        {
            player = player,
            itemUseBroadcaster = itemUseBroadcaster,
            worldVfxController = worldVfxController
        };

        context.SetTargets(targetMap);
    }

    private void Start()
    {
        PlayFromBeginning();
    }

    public void PlayFromBeginning()
    {
        index = -1;
        NextStep();
    }

    public void NextStep()
    {
        CleanupCurrent();

        index++;
        StepChanged?.Invoke(index);

        if (steps == null || index >= steps.Length)
        {
            ui.SetVisible(false);
            if (highlighter != null) highlighter.Clear();
            if (worldVfxController != null) worldVfxController.HideAll();
            return;
        }

        var step = steps[index];

        // 문자열 직접 가져오기
        ui.SetMessage(step.GetMessage(currentLanguage));
        ui.SetVisible(true);

        #region 기존의 Highlight
        /*        if (highlighter != null)
                {
                    if (step.targetsToHide != null)
                    {
                        foreach (var id in step.targetsToHide)
                        {
                            highlighter.ShowTarget(id, false);
                        }
                    }

                    if (step.targetsToShow != null)
                    {
                        foreach (var id in step.targetsToShow)
                            highlighter.ShowTarget(id, true);
                    }

                    if (!string.IsNullOrEmpty(step.highlightTargetId))
                        highlighter.Highlight(step.highlightTargetId);
                    else
                        highlighter.Clear();
                }

                if (step.conditionPrefab == null)
                {
                    StartCoroutine(AdvanceAfter(step.advanceDelay));
                    return;
                }

                currentCondition = Instantiate(step.conditionPrefab, transform);
                currentCondition.Completed += OnConditionCompleted;
                currentCondition.Activate(context);*/
        #endregion

        ApplyWorldVfx(step);
        ApplyHighlight(step);

        if (step.conditionPrefab == null)
        {
            StartCoroutine(AdvanceAfter(step.advanceDelay));
            return;
        }

        currentCondition = Instantiate(step.conditionPrefab, transform);
        currentCondition.Completed += OnConditionCompleted;
        currentCondition.Activate(context);
    }

    private void ApplyWorldVfx(TutorialStepAsset step)
    {
        if (worldVfxController == null || step == null)
            return;

        if (step.worldVfxToHide != null)
        {
            foreach (var id in step.worldVfxToHide)
                worldVfxController.Hide(id);
        }

        if (step.worldVfxToShow != null)
        {
            foreach (var id in step.worldVfxToShow)
                worldVfxController.Show(id);
        }
    }

    private void ApplyHighlight(TutorialStepAsset step)
    {
        if (highlighter == null || step == null)
            return;

        if (step.targetsToHide != null)
        {
            foreach (var id in step.targetsToHide)
                highlighter.ShowTarget(id, false);
        }

        if (step.targetsToShow != null)
        {
            foreach (var id in step.targetsToShow)
                highlighter.ShowTarget(id, true);
        }

        if (!string.IsNullOrEmpty(step.highlightTargetId))
            highlighter.Highlight(step.highlightTargetId);
        else
            highlighter.Clear();
    }

    private void OnConditionCompleted()
    {
        var step = steps[index];
        StartCoroutine(AdvanceAfter(step.advanceDelay));
    }

    private IEnumerator AdvanceAfter(float delay)
    {
        ui.SetVisible(false);
        yield return new WaitForSecondsRealtime(delay);
        NextStep();
    }

    private void CleanupCurrent()
    {
        if (index >= 0 && steps != null && index < steps.Length)
        {
            var step = steps[index];

            if (worldVfxController != null && step.worldVfxToHideOnExit != null)
            {
                foreach (var id in step.worldVfxToHideOnExit)
                    worldVfxController.Hide(id);
            }
        }

        if (currentCondition != null)
        {
            currentCondition.Completed -= OnConditionCompleted;
            currentCondition.Deactivate();
            Destroy(currentCondition.gameObject);
            currentCondition = null;
        }
    }

    // 언어 변경 가능
    public void SetLanguage(GameLanguage language)
    {
        currentLanguage = language;

        // 현재 단계 메시지 갱신
        if (index >= 0 && index < steps.Length)
        {
            ui.SetMessage(steps[index].GetMessage(currentLanguage));
        }
    }

    public void ForceClearVisuals()
    {
        if (ui != null)
            ui.SetVisible(false);

        if (highlighter != null)
            highlighter.Clear();

        if (worldVfxController != null)
            worldVfxController.HideAll();
    }
}
