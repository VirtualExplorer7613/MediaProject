using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Title;
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
        //Managers.Scene.LoadScene(Define.Scene.Game);
        Managers.Scene.LoadSceneAsync(Define.Scene.Prologue, false);
    }
    
}
