using TMPro;
using UnityEngine;

public class TrashManager : MonoBehaviour
{
    public static TrashManager Instance;

    private int totalTrash = 10;
    public TextMeshProUGUI trashCountText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
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
            trashCountText.text = totalTrash + "/50";
        }

        if (totalTrash <= 0)
        {
            Debug.Log("모든 쓰레기를 제거했습니다!");
        }
    }


    public void UpdateTrashUI()
    {
        trashCountText.text = totalTrash + "/50";
    }
}
