using System.Collections;
using UnityEngine;

public class PrologueScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Prologue;
        StartCoroutine(FadeInAfterLoad());
    }

    public override void Clear()
    {

    }

    public void StartGame()
    {
        //Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.Scene.LoadSceneAsync(Define.Scene.Game, false);
    }

    IEnumerator FadeInAfterLoad()
    {
        yield return null;
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeIn(1.5f);
    }
}
