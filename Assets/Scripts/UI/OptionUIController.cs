using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionUIController : MonoBehaviour
{
    public GameObject optionPanel; // ����â �г�
    public Slider bgmSlider;
    public Slider effectSlider;

    [SerializeField] private CrosshairUI crosshairUI;

    private bool isOptionOpen = false;

    private void Start()
    {
        var _ = Managers.Sound;
        // �����̴� �ʱⰪ ����
        bgmSlider.value = Managers.Sound.GetVolume(Define.Sound.Bgm);
        effectSlider.value = Managers.Sound.GetVolume(Define.Sound.Effect);

        // �����̴� ���� �� �̺�Ʈ ����
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
            Debug.Log("PŰ ����");
            ToggleOptionPanel();
        }
    }


    public void ToggleOptionPanel()
    {
        isOptionOpen = !isOptionOpen;
        optionPanel.SetActive(isOptionOpen);

        // ���� �Ͻ����� / �簳
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
        Time.timeScale = 1f; // ���� �簳

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
        //Application.Quit(); // Ȥ�� Title ������ �̵�
        Debug.Log("������ ȣ��! �׽�Ʈ�� ���� �� �̵��� ���� ����");
        Time.timeScale = 1f;
        // Ÿ��Ʋ�̸� ������
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
            yield return fade.FadeOut(duration);        // ������Ʈ�� �̹� �ִ� FadeIn/FadeOut �ڷ�ƾ
        else
            yield return new WaitForSecondsRealtime(duration);
    }


    /// <summary>���̵� �� ���ø����̼� ����</summary>
    private IEnumerator QuitGameRoutine()
    {
        yield return PlayFadeOut(1.5f);     // 1.5�� ���� ������ ����
        QuitApplication();
    }

    /// <summary>���̵� �� Ÿ��Ʋ ������ �̵�</summary>
    private IEnumerator ReturnToTitleRoutine()
    {
        yield return PlayFadeOut(1.0f);
        Managers.Scene.LoadScene(Define.Scene.Title);   // �ʿ��ϸ� LoadSceneAsync
    }

    private void QuitApplication()
    {
        Application.Quit();                 // ����� ���� ���Ͽ��� �� ����
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // ������ Play ��� ����
#endif
    }
}
