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
            Debug.Log("모든 쓰레기를 제거했습니다!");
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
            Debug.LogWarning("DialogueManager 인스턴스를 찾을 수 없습니다.");
            return;
        }

        if (totalTrash <= 0 && dialogueManager.AreAllQuestsCleared())
        {
            Debug.Log("모든 퀘스트 완료 및 쓰레기 처리 완료! 엔딩 씬으로 이동합니다.");
            GameScene gameScene = FindObjectOfType<GameScene>();
            if (gameScene != null)
            {
                gameScene.GotoEnding();
            }
            else
            {
                Debug.LogError("GameScene 인스턴스를 찾을 수 없습니다.");
            }
        }
    }


}
