using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUIController : MonoBehaviour
{
    public GameObject optionPanel; // 설정창 패널
    public Slider bgmSlider;
    public Slider effectSlider;

    [SerializeField] private CrosshairUI crosshairUI;

    private bool isOptionOpen = false;

    private void Start()
    {
        var _ = Managers.Sound;
        // 슬라이더 초기값 지정
        bgmSlider.value = Managers.Sound.GetVolume(Define.Sound.Bgm);
        effectSlider.value = Managers.Sound.GetVolume(Define.Sound.Effect);

        // 슬라이더 변경 시 이벤트 연결
        bgmSlider.onValueChanged.AddListener((value) =>
        {
            Managers.Sound.SetVolume(Define.Sound.Bgm, value);
        });

        effectSlider.onValueChanged.AddListener((value) =>
        {
            Managers.Sound.SetVolume(Define.Sound.Effect, value);
        });

        optionPanel.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P키 눌림");
            ToggleOptionPanel();
        }
    }


    public void ToggleOptionPanel()
    {
        isOptionOpen = !isOptionOpen;
        optionPanel.SetActive(isOptionOpen);

        // 게임 일시정지 / 재개
        Time.timeScale = isOptionOpen ? 0f : 1f;

        if (crosshairUI != null)
            crosshairUI.IsVisible = !isOptionOpen;

        if (isOptionOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Title)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    public void OnContinueButtonClicked()
    {
        isOptionOpen = false;
        optionPanel.SetActive(false);
        Time.timeScale = 1f; // 게임 재개

        if (Managers.Scene.CurrentScene.SceneType == Define.Scene.Title)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (crosshairUI != null)
                crosshairUI.IsVisible = true;
        }
    }

    public void OnExitButtonClicked()   
    {
        //Application.Quit(); // 혹은 Title 씬으로 이동
        Debug.Log("나가기 호출! 테스트를 위해 씬 이동은 막은 상태");
        Time.timeScale = 1f;
        // 타이틀이면 나가기
        if(Managers.Scene.CurrentScene.SceneType == Define.Scene.Title)
        {
            StartCoroutine(QuitGameRoutine());
        }
        else
        {
            //Managers.Scene.LoadSceneAsync(Define.Scene.Title, false);
            StartCoroutine(ReturnToTitleRoutine());
        }
           
    }

    private IEnumerator PlayFadeOut(float duration)
    {
        FadeController fade = FindObjectOfType<FadeController>();

        if (fade != null)
            yield return fade.FadeOut(duration);        // 프로젝트에 이미 있는 FadeIn/FadeOut 코루틴
        else
            yield return new WaitForSecondsRealtime(duration);
    }


    /// <summary>페이드 후 애플리케이션 종료</summary>
    private IEnumerator QuitGameRoutine()
    {
        yield return PlayFadeOut(1.5f);     // 1.5초 동안 서서히 암전
        QuitApplication();
    }

    /// <summary>페이드 후 타이틀 씬으로 이동</summary>
    private IEnumerator ReturnToTitleRoutine()
    {
        yield return PlayFadeOut(1.0f);
        Managers.Scene.LoadScene(Define.Scene.Title);   // 필요하면 LoadSceneAsync
    }

    private void QuitApplication()
    {
        Application.Quit();                 // 빌드된 실행 파일에서 앱 종료
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // 에디터 Play 모드 종료
#endif
    }
}
