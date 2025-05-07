using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEX
{
    public BaseScene CurrentScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }

    // 기본 로딩
    public void LoadScene(Define.Scene type)
    {
        Managers.Clear();
        SceneManager.LoadScene(GetSceneName(type));
    }

    // 비동기적 로딩
    public void LoadSceneAsync(Define.Scene type, bool useLoadingScreen)
    {
        if (useLoadingScreen)
        {
            // "Loading" 씬으로 먼저 이동
            SceneManager.LoadScene("Loading");
            LoadingSceneController.targetScene = type;
        }
        else
        {
            // 일반 페이드 처리 (ex: Title → Prologue)
            Managers.Instance.StartCoroutine(SimpleFadeSceneChange(type));
        }
    }

    IEnumerator SimpleFadeSceneChange(Define.Scene type)
    {
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null) yield return fade.FadeOut(3.0f);

        SceneManager.LoadScene(GetSceneName(type));
    }


    //enum 타입을 string으로 변환
    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

    public void Clear()
    {
        CurrentScene.Clear();
    }
}
