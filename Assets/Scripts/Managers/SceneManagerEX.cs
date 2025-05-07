using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEX
{
    public BaseScene CurrentScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }

    // �⺻ �ε�
    public void LoadScene(Define.Scene type)
    {
        Managers.Clear();
        SceneManager.LoadScene(GetSceneName(type));
    }

    // �񵿱��� �ε�
    public void LoadSceneAsync(Define.Scene type, bool useLoadingScreen)
    {
        if (useLoadingScreen)
        {
            // "Loading" ������ ���� �̵�
            SceneManager.LoadScene("Loading");
            LoadingSceneController.targetScene = type;
        }
        else
        {
            // �Ϲ� ���̵� ó�� (ex: Title �� Prologue)
            Managers.Instance.StartCoroutine(SimpleFadeSceneChange(type));
        }
    }

    IEnumerator SimpleFadeSceneChange(Define.Scene type)
    {
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null) yield return fade.FadeOut(3.0f);

        SceneManager.LoadScene(GetSceneName(type));
    }


    //enum Ÿ���� string���� ��ȯ
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
