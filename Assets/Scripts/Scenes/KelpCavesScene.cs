using System.Collections;
using UnityEngine;

public class KelpCavesScene : BaseScene
{
    [Header("Scene Settings")]
    [SerializeField] private bool fadeInOnStart = true;
    [SerializeField] private float fadeInDuration = 1.5f;

    [Header("Debug")]
    [SerializeField] private bool enableDebugEndingKey = true;
    [SerializeField] private KeyCode debugEndingKey = KeyCode.X;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.KelpCaves;

        if (fadeInOnStart)
            StartCoroutine(FadeInAfterLoad());
    }

    private void Update()
    {
        // 엔딩 테스트용
        if (enableDebugEndingKey && Input.GetKeyDown(debugEndingKey))
        {
            GotoEnding();
        }
    }

    public override void Clear()
    {
        // 씬 정리할 내용이 생기면 여기에 작성
        // 예: BGM 정지, PlayerPrefs 저장, 임시 오브젝트 정리 등
    }

    public void GotoEnding()
    {
        //Managers.Scene.LoadSceneAsync(Define.Scene.Ending, false);
        Managers.Scene.LoadSceneAsyncWithFadeColor(
        Define.Scene.Ending,
        Color.black,
        1.0f,
        1.0f
    );
    }

    private IEnumerator FadeInAfterLoad()
    {
        yield return null;

        FadeController fade = FindObjectOfType<FadeController>();

        if (fade != null)
            yield return fade.FadeIn(fadeInDuration);
    }
}