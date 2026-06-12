using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step", fileName = "TutorialStep_")]
public class TutorialStepAsset : ScriptableObject
{
    [Header("Message")]
    [TextArea(2, 5)]
    public string messageKorean;

    [TextArea(2, 5)]
    public string messageEnglish;

    [Header("Condition")]
    public TutorialCondition conditionPrefab;

    [Header("Highlight (Optional)")]
    public string highlightTargetId;

    [Header("UI Control")]
    public string[] targetsToShow;
    public string[] targetsToHide;

    [Header("World VFX")]
    public string[] worldVfxToShow;
    public string[] worldVfxToHide;
    public string[] worldVfxToHideOnExit;

    [Header("Flow")]
    public float advanceDelay = 0.2f;

    public string GetMessage(GameLanguage language)
    {
        return language == GameLanguage.Korean
            ? messageKorean
            : messageEnglish;
    }
}
