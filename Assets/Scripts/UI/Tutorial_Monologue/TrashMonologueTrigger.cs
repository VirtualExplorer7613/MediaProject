using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TrashMonologueTrigger : MonoBehaviour
{
    [Header("설정")]
    public MonologueData monologue;      // 
    public float triggerRadius = 5f;     // 근접 거리

    Transform player;
    bool played;

    void Awake()
    {
        GetComponent<Collider>().isTrigger = true;        // 안전장치
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
            played = true;                       // 1회만 재생
            MonologuePopup.Instance.Show(monologue.steps);
        }
    }
}
