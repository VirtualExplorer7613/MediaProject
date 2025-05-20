using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrashMonologueTrigger : MonoBehaviour
{
    [Header("����")]
    public MonologueData monologue;      // 
    public float triggerRadius = 5f;     // ���� �Ÿ�

    Transform player;
    bool played;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;        // ������ġ
    }

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (played || player == null) return;

        if (Vector3.Distance(player.position, transform.position) <= triggerRadius)
        {
            played = true;                       // 1ȸ�� ���
            MonologuePopup.Instance.Show(monologue.steps);
        }
    }
}
