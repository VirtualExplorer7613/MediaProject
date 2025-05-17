using System.Collections;
using UnityEngine;

public class EndingScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Ending;
        StartCoroutine(FadeInAfterLoad());
    }

    public override void Clear()
    {

    }

    IEnumerator FadeInAfterLoad()
    {
        yield return null;
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeIn(1.5f);
    }
}
