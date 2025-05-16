using UnityEngine;
using UnityEngine.UI;

public class OptionUIController : MonoBehaviour
{
    public GameObject optionPanel; // 설정창 패널
    public Slider bgmSlider;
    public Slider effectSlider;

    private bool isOptionOpen = false;

    private void Start()
    {
        var _ = Managers.Sound;
        // 슬라이더 초기값 지정
        bgmSlider.value = 1f;
        effectSlider.value = 1f;

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

        if (isOptionOpen)
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

    public void OnContinueButtonClicked()
    {
        isOptionOpen = false;
        optionPanel.SetActive(false);
        Time.timeScale = 1f; // 게임 재개

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnExitButtonClicked()
    {
        //Application.Quit(); // 혹은 Title 씬으로 이동
        Debug.Log("나가기 호출! 테스트를 위해 씬 이동은 막은 상태");
        Time.timeScale = 1f;
        Managers.Scene.LoadScene(Define.Scene.Title);
    }
}
