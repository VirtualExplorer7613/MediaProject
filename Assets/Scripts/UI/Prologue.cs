using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prologue : BaseScene
{
    [Header("Prologue Settings")]
    public Image prologueImage; // �̹����� ǥ���� UI ������Ʈ
    public TextMeshProUGUI prologueText;  // �ؽ�Ʈ�� ǥ���� UI ������Ʈ
    public List<Sprite> imageList; // ���ѷα� �̹��� ����Ʈ
    public List<string> textList;  // ���ѷα� �ؽ�Ʈ ����Ʈ
    public float typingSpeed = 0.09f; // Ÿ���� �ӵ�
    //public GameObject skipButton;



    [Header("Next Scene")]
    [SerializeField]
    private Define.Scene sceneName;

    private int currentIndex = 0; // ���� ǥ�� ���� �̹���/�ؽ�Ʈ �ε���
    private bool isTyping = false; // ���� �ؽ�Ʈ Ÿ���� ������ Ȯ��
    private bool isMoving = false;  // �� �̵��� �����ϴ��� Ȯ��
    private const float baseWidth = 960f; // ���� ���� ũ��
    private const float baseHeight = 600; // ���� ���� ũ��

    private void Start()
    {
        DisplayCurrentStep();
        //skipButton?.SetActive(false);
    }

    private void Update()
    {
        // �Է� ����: Ŭ�� �Ǵ� �����̽���
        if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)) && !isTyping)
        {
            ShowNextStep();
        }
    }

    private void DisplayCurrentStep()
    {
        // �̹��� ������Ʈ
        if (currentIndex < imageList.Count)
        {
            prologueImage.sprite = imageList[currentIndex];
            AdjustImageSize();
        }

        // �ؽ�Ʈ Ÿ���� ȿ�� ����
        if (currentIndex < textList.Count)
            StartCoroutine(TypeText(textList[currentIndex]));
    }

    private void ShowNextStep()
    {
        currentIndex++;
        if (currentIndex > 0)
        {
            //skipButton?.SetActive(true);
        }
        // ���� ������ �ִ� ��� ǥ��
        if (currentIndex < Mathf.Min(imageList.Count, textList.Count))
        {
            DisplayCurrentStep();
        }
        else if (!isMoving)
        {
            EndPrologue();
            isMoving = true;
        }
    }

    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        prologueText.text = ""; // ���� �ؽ�Ʈ �����

        foreach (char letter in text.ToCharArray())
        {
            prologueText.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void EndPrologue()
    {
        Debug.Log("���ѷα� ����");
        // �ʿ� �� ���� ������ ��ȯ�ϰų� �ٸ� �̺�Ʈ ����
        Managers.Scene.LoadScene(sceneName);
    }

    public void SkipPrologue()
    {
        EndPrologue();
    }

    /* private IEnumerator LoadNextSceneAfterFade()
     {
         if (fadeOutAudio != null)
             yield return new WaitForSeconds(fadeOutAudio.fadeDuration);

         mySceneManager.ChangeScene(nextScene);
     }*/


    private void AdjustImageSize()
    {
        RectTransform rectTransform = prologueImage.GetComponent<RectTransform>();
        Sprite currentSprite = prologueImage.sprite;

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

    public override void Clear()
    {

    }
}
