using UnityEngine;

public class QuestInteractionHint : MonoBehaviour
{
    public GameObject player;
    public GameObject hintUI;      // 아이콘
    public float showDistance = 3f; // 표시 거리

    void Update()
    {
        if (player == null || hintUI == null)
            return;

        float distance = Vector3.Distance(transform.position, player.transform.position);

        if (distance <= showDistance)
        {
            hintUI.SetActive(true);
        }
        else
        {
            hintUI.SetActive(false);
        }
    }
}
