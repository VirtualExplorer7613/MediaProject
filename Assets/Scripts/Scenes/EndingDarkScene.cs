using System.Collections;
using UnityEngine;

public class EndingDarkScene : BaseScene
{
    [SerializeField] private EndingDarkSceneController endingDarkController;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.EndingDark;

        if (endingDarkController == null)
            endingDarkController = FindFirstObjectByType<EndingDarkSceneController>();

        StartCoroutine(FadeInAfterLoad());
    }

    public override void Clear()
    {

    }

    private IEnumerator FadeInAfterLoad()
    {
        yield return null;

        FadeController fade = GameObject.FindObjectOfType<FadeController>();

        if (fade != null)
            yield return fade.FadeIn(1.5f);

        yield return new WaitForSecondsRealtime(0.1f);

        if (endingDarkController != null)
            endingDarkController.BeginSequence();
        else
            Debug.LogWarning("[EndingDarkScene] EndingDarkSceneController reference missing!");
    }
}