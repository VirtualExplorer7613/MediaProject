using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerEX
{
    public BaseScene CurrentScene { get { return GameObject.FindAnyObjectByType<BaseScene>(); } }

    public void LoadScene(Define.Scene type)
    {
        CurrentScene.Clear();
        SceneManager.LoadScene(GetSceneName(type));
    }

    //enum 타입을 string으로 변환
    string GetSceneName(Define.Scene type)
    {
        string name = System.Enum.GetName(typeof(Define.Scene), type);
        return name;
    }

}
