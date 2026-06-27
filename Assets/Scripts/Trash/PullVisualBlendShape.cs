using System.Collections;
using UnityEngine;

public class PullVisualBlendShape : MonoBehaviour
{
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

            // 何靛矾款 啊加/皑加
            float eased = Mathf.SmoothStep(0f, 1f, p);

            float weight = Mathf.Lerp(from, to, eased);
            skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, weight);

            yield return null;
        }

        skinnedMeshRenderer.SetBlendShapeWeight(blendShapeIndex, to);
        blendRoutine = null;
    }
}