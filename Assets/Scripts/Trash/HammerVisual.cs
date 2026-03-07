using System.Collections;
using UnityEngine;

public class HammerVisual : MonoBehaviour
{
    [Header("Attach")]
    [SerializeField] private Transform followTarget; // 보통 Main Camera Transform 넣기

    [Header("Visual")]
    [SerializeField] private GameObject visualRoot;

    [Header("Pose (camera local)")]
    [SerializeField] private Vector3 localPos = new Vector3(0.35f, -0.25f, 1.7f);
    [SerializeField] private Vector3 localEulerIdle = new Vector3(15f, 0f, 0f);

    [Header("Swing")]
    [SerializeField] private float swingDuration = 1f; // 0.08~0.15 추천
    [SerializeField] private float swingRollFrom = -120f;
    [SerializeField] private float swingRollTo = 120f;
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine swingCo;
    private Quaternion idleRot;
    private Quaternion extraRot = Quaternion.identity;

    private void Awake()
    {
        idleRot = Quaternion.Euler(localEulerIdle);
    }

    private void LateUpdate()
    {
        if (followTarget == null) return;

        // 카메라에 붙어서 따라가게
        transform.position = followTarget.TransformPoint(localPos);
        transform.rotation = followTarget.rotation * idleRot * extraRot;
    }

    public void SetVisible(bool visible)
    {
        Debug.Log($"[HammerVisual] SetVisible {visible}");
        if (visualRoot != null)
            visualRoot.SetActive(visible);
    }

    public void PlaySwing()
    {
        Debug.Log("[HammerVisual] PlaySwing");

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[HammerVisual] Root inactive");
            return;
        }

        if (visualRoot != null && !visualRoot.activeSelf)
            visualRoot.SetActive(true);

        //if (swingCo != null) StopCoroutine(swingCo);
        if (swingCo != null) return;
        swingCo = StartCoroutine(SwingRoutine());
    }

    private IEnumerator SwingRoutine()
    {
        float t = 0f;

        while (t < swingDuration)
        {
            t += Time.deltaTime;
            float u = Mathf.Clamp01(t / swingDuration);
            float k = ease.Evaluate(u);
            float roll = Mathf.Lerp(swingRollFrom, swingRollTo, k);

            //extraRot = Quaternion.AngleAxis(roll, Vector3.forward);
            extraRot = Quaternion.Euler(roll, 0f, 0f);
            yield return null;
        }

        extraRot = Quaternion.identity;
        swingCo = null;
    }
}