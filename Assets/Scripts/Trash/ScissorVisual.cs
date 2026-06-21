using UnityEngine;

public class ScissorVisual : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource cutLoopAudioSource;

    [Header("Placement")]
    [SerializeField] private Vector3 offset = new Vector3(0.45f, 0.22f, 0f);
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(0f, 180f, 0f);

    [Header("Animator Params")]
    [SerializeField] private string cutBoolName = "IsCutting";

    private Transform target;
    private ScissorableTrash targetTrash;
    private Vector3 defaultLocalScale;

    private void Awake()
    {
        defaultLocalScale = transform.localScale;
    }

    public void SetTarget(Transform newTarget)
    {
        targetTrash = null;
        target = newTarget;
        ApplyScale(Vector3.one);
    }

    public void SetTarget(ScissorableTrash newTarget)
    {
        targetTrash = newTarget;
        target = newTarget != null ? newTarget.transform : null;
        ApplyScale(newTarget != null ? newTarget.ScissorVisualScaleMultiplier : Vector3.one);
    }


    public void SetVisible(bool visible)
    {
        if (visualRoot != null)
            visualRoot.SetActive(visible);

        if (!visible)
            StopCutLoop();
    }

    public void StartCutLoop()
    {
        if (animator != null)
            animator.SetBool(cutBoolName, true);

        if (cutLoopAudioSource != null && !cutLoopAudioSource.isPlaying)
        {
            cutLoopAudioSource.loop = true;
            cutLoopAudioSource.Play();
        }
    }

    public void StopCutLoop()
    {
        if (animator != null)
            animator.SetBool(cutBoolName, false);

        if (cutLoopAudioSource != null && cutLoopAudioSource.isPlaying)
            cutLoopAudioSource.Stop();
    }

    /*private void LateUpdate()
    {
        if (target == null || visualRoot == null || !visualRoot.activeSelf) return;

        transform.position = target.position + offset;

        if (faceCamera && Camera.main != null)
        {
            Vector3 dir = Camera.main.transform.position - transform.position;
            if (dir.sqrMagnitude > 0.001f)
            {
                // transform.rotation = Quaternion.LookRotation(-dir.normalized, Vector3.up);
                Quaternion lookRot = Quaternion.LookRotation(-dir.normalized, Vector3.up);
                transform.rotation = lookRot * Quaternion.Euler(rotationOffsetEuler);
            }
               
        }
    }*/

    private void LateUpdate()
    {
        if (target == null || visualRoot == null || !visualRoot.activeSelf) return;

        bool useCustomAnchor = targetTrash != null && targetTrash.HasScissorVisualAnchor;
        Transform followTarget = useCustomAnchor ? targetTrash.ScissorVisualAnchor : target;
        Vector3 customOffset = targetTrash != null ? targetTrash.ScissorVisualOffset : Vector3.zero;
        Vector3 customRotationOffset = targetTrash != null ? targetTrash.ScissorVisualRotationOffsetEuler : Vector3.zero;

        transform.position = useCustomAnchor
            ? followTarget.position + customOffset
            : followTarget.position + offset + customOffset;

        Quaternion finalRotationOffset = Quaternion.Euler(rotationOffsetEuler + customRotationOffset);

        if (faceCamera && Camera.main != null)
        {
            Vector3 dir = Camera.main.transform.position - transform.position;
            if (dir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(-dir.normalized, Vector3.up);
                transform.rotation = lookRot * finalRotationOffset;
            }
        }
        else if (useCustomAnchor)
        {
            transform.rotation = followTarget.rotation * finalRotationOffset;
        }
    }

    private void ApplyScale(Vector3 multiplier)
    {
        transform.localScale = new Vector3(
            defaultLocalScale.x * multiplier.x,
            defaultLocalScale.y * multiplier.y,
            defaultLocalScale.z * multiplier.z
        );
    }
}