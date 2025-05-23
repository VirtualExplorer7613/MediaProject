using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Epliogue : BaseScene
{
    [Header("Epliogue Settings")]
    public Image epliogueImage; // �̹����� ǥ���� UI ������Ʈ
    public TextMeshProUGUI epliogueText;  // �ؽ�Ʈ�� ǥ���� UI ������Ʈ



    public List<VisualTextStep> steps;
/*    public List<Sprite> imageList; //  �̹��� ����Ʈ
    public List<string> textList;  //  �ؽ�Ʈ ����Ʈ*/
    public float typingSpeed = 0.09f; // Ÿ���� �ӵ�
    //public GameObject skipButton;

    FadeController fade;
    bool isTransitioning;


    [Header("Next Scene")]
    [SerializeField]
    private Define.Scene sceneName;

    private int currentIndex = 0; // ���� ǥ�� ���� �̹���/�ؽ�Ʈ �ε���
    private bool isTyping = false; // ���� �ؽ�Ʈ Ÿ���� ������ Ȯ��
    private bool isMoving = false;  // �� �̵��� �����ϴ��� Ȯ��
    private const float baseWidth = 1920; // ���� ���� ũ��
    private const float baseHeight = 880; // ���� ���� ũ��


    private void Start()
    {

        //skipButton?.SetActive(false);

        if (steps == null || steps.Count == 0)
        {
            Debug.Log("step list is empty, assigning default");

            steps.Add(new VisualTextStep
            {
                image = null, // Ȥ�� �⺻ �̹����� �ִٸ� �־ OK
                text =
                    "�������� ���ƿ� �� ���� �ٴ١�\n" +
                    "�׷��� ������, ������ �ٸ� ��?\n\n" +

                    "�ٴٴ� ���� ��� �״���ε�,\n" +
                    "��� �� �� ������ �ƴϾ�.\n" +

                    "������ ���뿡�� �ݰ��� �Ҹ����� ��ȴµ���\n\n" +

                    "���� ���� ���� ���� ���� �־��� �ɱ�?\n" +
                    "����, Ȯ���غ��߰ڴ�."
            });
        }
        DisplayCurrentStep();
    }

    private void Update()
    {
        #region ���̵� ���� ����
        // �Է� ����: Ŭ�� �Ǵ� �����̽���
        /*if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !isTyping)
        {
            ShowNextStep();
        }*/
        #endregion

        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
       && !isTyping && !isTransitioning)
        {
            StartCoroutine(AdvanceStepWithFade());
        }



    }

    IEnumerator AdvanceStepWithFade()
    {
        isTransitioning = true;                 // �Է� ����

        FadeController fade = FindObjectOfType<FadeController>();
        if (fade != null)                       // �� ���� 3 ��
            yield return fade.FadeOut(2.0f);

        ShowNextStep();                         // �� �̹������ؽ�Ʈ ��ü

        if (fade != null)                       // �� ����� 3 ��
            yield return fade.FadeIn(1.5f);

        isTransitioning = false;                // �Է� ���
    }


    private void DisplayCurrentStep()
    {
        if (currentIndex >= steps.Count)
            return;

         VisualTextStep step = steps[currentIndex];

        // �̹��� ���
        if (step.image != null)
        {
            epliogueImage.sprite = step.image;
            epliogueImage.gameObject.SetActive(true);
            AdjustImageSize();
        }
        else
        {
            epliogueImage.gameObject.SetActive(false);
        }

        // �ؽ�Ʈ ���
        if (!string.IsNullOrEmpty(step.text))
        {
            StartCoroutine(TypeText(step.text));
        }
        else
        {
            epliogueText.text = "";
            isTyping = false;
        }
    }

    private void ShowNextStep()
    {
        currentIndex++;
        if (currentIndex < steps.Count)
        {
            DisplayCurrentStep();
        }
        else if (!isMoving)
        {
            StartCoroutine(GoToTitleWithFade());
            isMoving = true;
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        epliogueText.text = ""; // ���� �ؽ�Ʈ �����

        int i = 0;
        while (i < text.Length)
        {
            // ���� �ٹٲ� ���� �߰��ϸ�
            if (text[i] == '\\' && i + 1 < text.Length && text[i + 1] == 'n')
            {
                epliogueText.text += '\n'; // �ٹٲ� �߰�
                i += 2; // �� ���� �ѱ��
                yield return new WaitForSeconds(typingSpeed * 2f);
                continue; // �ٹٲ��϶� ��¦
            }

            epliogueText.text += text[i];
            i++;

            // ������ ��� ������, �ƴϸ� �⺻ �ӵ�
            if (epliogueText.text[^1] == ' ')
                yield return new WaitForSeconds(typingSpeed * 0.3f); // ����� �� ������
            else
                yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndEpilogue()
    {
        Debug.Log("���ʷα� ����");
        // �ʿ� �� ���� ������ ��ȯ�ϰų� �ٸ� �̺�Ʈ ����
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Managers.Scene.LoadSceneAsync(Define.Scene.Title, false);
    }

    public void SkipEpilogue()
    {
        EndEpilogue();
    }

    private IEnumerator GoToTitleWithFade()
    {
        isTransitioning = true;

        FadeController fade = FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeOut(2.0f); // ������ ���̵� �ƿ�

        EndEpilogue();
    }

    /* private IEnumerator LoadNextSceneAfterFade()
     {
         if (fadeOutAudio != null)
             yield return new WaitForSeconds(fadeOutAudio.fadeDuration);

         mySceneManager.ChangeScene(nextScene);
     }*/


    private void AdjustImageSize()
    {
        RectTransform rectTransform = epliogueImage.GetComponent<RectTransform>();
        Sprite currentSprite = epliogueImage.sprite;

        if (currentSprite != null)
        {
            // �̹����� ���� ���� ���
            float imageWidth = currentSprite.rect.width;
            float imageHeight = currentSprite.rect.height;
            float aspectRatio = imageWidth / imageHeight;

            // ���� ũ��� ���Ͽ� �� �κп� ����
            if (aspectRatio >= baseWidth / baseHeight)
            {
                // ���ΰ� ���غ��� ��ų� ���� ���
                rectTransform.sizeDelta = new Vector2(baseWidth, baseWidth / aspectRatio);
            }
            else
            {
                // ���ΰ� ���غ��� �� ���
                rectTransform.sizeDelta = new Vector2(baseHeight * aspectRatio, baseHeight);
            }
        }
    }

    IEnumerator SimpleFadeOutChange(float time = 3f)
    {
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null) yield return fade.FadeOut(time);

    }

    IEnumerator SimpleFadeInChange(float time = 3f)
    {
        yield return null;
        FadeController fade = GameObject.FindObjectOfType<FadeController>();
        if (fade != null)
            yield return fade.FadeIn(time);
    }

    public override void Clear()
    {

    }
}
