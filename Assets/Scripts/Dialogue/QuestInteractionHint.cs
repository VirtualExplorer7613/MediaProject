using UnityEngine;

public class QuestInteractionHint : MonoBehaviour
{
    public GameObject player;
    public GameObject hintUI;      // ������
    public float showDistance = 3f; // ǥ�� �Ÿ�

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
