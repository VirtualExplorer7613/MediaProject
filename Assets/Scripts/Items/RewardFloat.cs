using UnityEngine;

public class RewardFloat : MonoBehaviour
{
    [Header("Float")]
    [SerializeField] private float amplitude = 0.25f;   // 위아래 폭
    [SerializeField] private float frequency = 1.2f;    // 속도(초당 진동)
    [SerializeField] private bool useLocalSpace = false;

    [Header("Rotation (Optional)")]
    [SerializeField] private bool rotate = true;
    [SerializeField] private float rotateSpeedY = 60f;  // 초당 Y회전 각도

    [Header("Spawn Offset (Optional)")]
    [SerializeField] private float randomPhase = 1f;    // 여러 개 뜨면 서로 다른 타이밍

    private Vector3 basePos;
    private float phase;

    private void OnEnable()
    {
        phase = Random.value * randomPhase * Mathf.PI * 2f;
        basePos = useLocalSpace ? transform.localPosition : transform.position;
    }

    private void Update()
    {
        float t = Time.time * frequency + phase;
        float y = Mathf.Sin(t) * amplitude;

        if (useLocalSpace)
        {
            var p = basePos;
            p.y += y;
            transform.localPosition = p;
        }
        else
        {
            var p = basePos;
            p.y += y;
            transform.position = p;
        }

        if (rotate)
        {
            transform.Rotate(0f, rotateSpeedY * Time.deltaTime, 0f, Space.Self);
        }
    }
}
