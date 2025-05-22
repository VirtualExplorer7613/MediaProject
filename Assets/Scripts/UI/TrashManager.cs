using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;

    //private int totalTrash;
    //private int initialTrashCount;
    private int totalTrash = 5;
    public TextMeshProUGUI trashCountText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        //initialTrashCount = GameObject.FindGameObjectsWithTag("Trash").Length;
        //totalTrash = initialTrashCount;

        UpdateTrashUI();
    }

    public void DecreaseTrash()
    {
        totalTrash--;

        if (trashCountText == null)
        {
            Debug.LogError("trashCountText is NULL!");
        }
        else
        {
            trashCountText.text = totalTrash + "/60";
            //trashCountText.text = totalTrash + "/" + initialTrashCount;
        }

        if (totalTrash <= 0)
        {
            Debug.Log("��� �����⸦ �����߽��ϴ�!");
            CheckEndingCondition();
        }
    }


    public void UpdateTrashUI()
    {
        trashCountText.text = totalTrash + "/60";
        //trashCountText.text = totalTrash + "/" + initialTrashCount;
    }

    private void CheckEndingCondition()
    {
        DialogueManager dialogueManager = DialogueManager.Instance;
        if (dialogueManager == null)
        {
            Debug.LogWarning("DialogueManager �ν��Ͻ��� ã�� �� �����ϴ�.");
            return;
        }

        if (totalTrash <= 0 && dialogueManager.AreAllQuestsCleared())
        {
            Debug.Log("��� ����Ʈ �Ϸ� �� ������ ó�� �Ϸ�! ���� ������ �̵��մϴ�.");
            GameScene gameScene = FindObjectOfType<GameScene>();
            if (gameScene != null)
            {
                gameScene.GotoEnding();
            }
            else
            {
                Debug.LogError("GameScene �ν��Ͻ��� ã�� �� �����ϴ�.");
            }
        }
    }


}
