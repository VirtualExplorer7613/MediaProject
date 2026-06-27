using System.Collections;
using UnityEngine;

public class PullVisualBlendShape : MonoBehaviour
{
    [Header("Transform Fix (크기 및 축 보정)")]
    [SerializeField] private bool applyFix = true; // 인스펙터에서 켜고 끌 수 있는 스위치
    [SerializeField] private Vector3 targetScale = new Vector3(3f, 3f, 3f); // 원하는 크기 (예시)
    [SerializeField] private Vector3 targetRotation = new Vector3(90f, 0f, 0f); // 세우기 위한 회전 축 (예시)

    [Header("BlendShape")]
    [SerializeField] private SkinnedMeshRenderer skinnedMeshRenderer;
    [SerializeField] private int blendShapeIndex = 0;

    [Header("Appear Animation")]
    [SerializeField] private float appearDuration = 0.25f;
    [SerializeField] private float startWeight = 0f;
    [SerializeField] private float endWeight = 80f;

    [Header("Release Animation")]
    [SerializeField] private float releaseDuration = 0.15f;

    private Coroutine blendRoutine;

    private void Awake()
    {
        if (skinnedMeshRenderer == null)
            skinnedMeshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();

        // 아이템이 생성/활성화될 때 인스펙터에서 설정한 크기와 회전값으로 강제 고정합니다.
        if (applyFix)
        {
            transform.localScale = targetScale;
            transform.localRotation = Quaternion.Euler(targetRotation);
        }
    }

    public void PlayAppear()
    {
        PlayBlend(startWeight, endWeight, appearDuration);
    }

    public void PlayRelease()
    {
        PlayBlend(endWeight, startWeight, releaseDuration);
    }

    public void SetWeight(float weight)
    {
        if (skinnedMeshRenderer == null) return;
        skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, weight);
    }

    private void PlayBlend(float from, float to, float duration)
    {
        if (skinnedMeshRenderer == null) return;

        if (blendRoutine != null)
            StopCoroutine(blendRoutine);

        blendRoutine = StartCoroutine(BlendRoutine(from, to, duration));
    }

    private IEnumerator BlendRoutine(float from, float to, float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);

            // 부드러운 가속/감속
            float eased = Mathf.SmoothStep(0f, 1f, p);

            float weight = Mathf.Lerp(from, to, eased);
            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, weight);

            yield return null;
        }

        skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, to);
        blendRoutine = null;
    }
}