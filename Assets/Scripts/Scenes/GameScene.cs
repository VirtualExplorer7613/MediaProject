using System.Collections;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Game;
        StartCoroutine(FadeInAfterLoad());
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.X))
            GotoEnding();
    }

    public override void Clear()
    {
        
    }

    public void GotoEnding()
    {
        Managers.Scene.LoadSceneAsync(Define.Scene.Ending, false);
    }

    IEnumerator FadeInAfterLoad()
    {
        yield return null;
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeIn(1.5f);
    }

}
