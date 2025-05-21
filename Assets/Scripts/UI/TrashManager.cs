using TMPro;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;

    //private int totalTrash;
    //private int initialTrashCount;
    private int totalTrash = 60;
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
        }
    }


    public void UpdateTrashUI()
    {
        trashCountText.text = totalTrash + "/60";
        //trashCountText.text = totalTrash + "/" + initialTrashCount;
    }
}
