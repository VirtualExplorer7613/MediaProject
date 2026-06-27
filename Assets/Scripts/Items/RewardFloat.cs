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

    [Header("Custom Rotation Axis (Optional)")]
    [Tooltip("켜면 기본 Local Y축 대신 아래 customRotationAxis 기준으로 회전")]
    [SerializeField] private bool useCustomRotationAxis = false;

    [Header("Axis Alignment (Optional)")]
    [SerializeField] private bool alignLocalZToWorldUp = false;

    [Tooltip("useCustomRotationAxis가 켜졌을 때 사용할 회전축")]
    [SerializeField] private Vector3 customRotationAxis = Vector3.up;

    [Tooltip("커스텀 회전축을 Local 기준으로 볼지, World 기준으로 볼지")]
    [SerializeField] private Space customRotationSpace = Space.Self;

    private Vector3 basePos;
    private float phase;

    private void OnEnable()
    {
        if (alignLocalZToWorldUp)
        {
            AlignLocalAxisToWorld(
                transform,
                Vector3.forward, // Local Z축
                Vector3.up       // World Up
            );
        }

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
            RotateReward();
            // transform.Rotate(0f, rotateSpeedY * Time.deltaTime, 0f, Space.Self);
        }
    }

    private void RotateReward()
    {
        if (!useCustomRotationAxis)
        {
            // 기존 동작 유지: Local Y축 회전
            transform.Rotate(0f, rotateSpeedY * Time.deltaTime, 0f, Space.Self);
            return;
        }

        Vector3 axis = customRotationAxis;

        // 축이 0,0,0이면 회전 불가하므로 안전하게 Y축으로 대체
        if (axis.sqrMagnitude <= 0.0001f)
            axis = Vector3.up;

        transform.Rotate(
            axis.normalized,
            rotateSpeedY * Time.deltaTime,
            customRotationSpace
        );
    }

    private void AlignLocalAxisToWorld(Transform target, Vector3 localAxis, Vector3 worldAxis)
    {
        Vector3 currentWorldAxis = target.TransformDirection(localAxis.normalized);

        Quaternion correction = Quaternion.FromToRotation(
            currentWorldAxis,
            worldAxis.normalized
        );

        target.rotation = correction * target.rotation;
    }
}
