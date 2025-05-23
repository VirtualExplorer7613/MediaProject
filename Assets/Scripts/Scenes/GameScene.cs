using System.Collections;
using UnityEngine;

public class GameScene : BaseScene
{
    [SerializeField] private KeyGuidePopup keyGuidePopup;

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

        yield return new WaitForSecondsRealtime(0.1f);  // 완충

        if (keyGuidePopup != null)
            keyGuidePopup.Show();
        else
            Debug.LogWarning("KeyGuidePopup reference missing!");
        yield return new WaitUntil(() => !keyGuidePopup.IsShowing); // IsShowing = false 가 될 때까지

    }

}
