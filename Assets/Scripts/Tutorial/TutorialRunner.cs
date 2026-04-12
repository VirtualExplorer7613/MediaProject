using System.Collections;
using UnityEngine;

public class TutorialRunner : MonoBehaviour
{
    [Header("Steps")]
    [SerializeField] private TutorialStepAsset[] steps;

    [Header("Language")]
    [SerializeField] private GameLanguage currentLanguage = GameLanguage.Korean;

    [Header("References")]
    [SerializeField] private TutorialUI ui;
    [SerializeField] private Transform player;
    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;
    [SerializeField] private TutorialHighlighter highlighter;
    [SerializeField] private Transform orcaTarget;

    private int index = -1;
    private TutorialCondition currentCondition;
    private TutorialContext context;

    public int CurrentStepIndex => index;

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
        context = new TutorialContext
        {
            player = player,
            itemUseBroadcaster = itemUseBroadcaster,
            orcaTarget = orcaTarget
        };
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
        if (steps == null || index >= steps.Length)
        {
            ui.SetVisible(false);
            if (highlighter != null) highlighter.Clear();
            return;
        }

        var step = steps[index];

        // 문자열 직접 가져오기
        ui.SetMessage(step.GetMessage(currentLanguage));
        ui.SetVisible(true);

        if (highlighter != null)
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
        currentCondition.Activate(context);
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
}
