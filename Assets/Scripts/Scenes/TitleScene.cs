using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : BaseScene
{
    private bool isPlayClicked = false;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Title;

        StartCoroutine(FadeInAfterLoad());

        isPlayClicked = false;
    }

    void Update()
    {
        /*if (Input.GetKeyUp(KeyCode.Q))
        {
            Managers.Scene.LoadScene(Define.Scene.Game);
        }*/
    }

    public override void Clear()
    {
        Debug.Log("Title Scene Clear!");
    }

    public void StartGame()
    {
        if (isPlayClicked)
        {
            return;
        }
        isPlayClicked = true;
        //Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.Scene.LoadSceneAsync(Define.Scene.Prologue, false);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    IEnumerator FadeInAfterLoad()
    {
        yield return null;
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeIn(1f);
    }
}
