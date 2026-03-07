using UnityEngine;

public class ScissorVisual : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject visualRoot;
    [SerializeField] private Animator animator;

    [Header("Placement")]
    [SerializeField] private Vector3 offset = new Vector3(0.45f, 0.22f, 0f);
    [SerializeField] private bool faceCamera = true;
    [SerializeField] private Vector3 rotationOffsetEuler = new Vector3(0f, 180f, 0f);

    [Header("Animator Params")]
    [SerializeField] private string cutBoolName = "IsCutting";

    private Transform target;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public void SetVisible(bool visible)
    {
        if (visualRoot != null)
            visualRoot.SetActive(visible);
    }

    public void StartCutLoop()
    {
        if (animator != null)
            animator.SetBool(cutBoolName, true);
    }

    public void StopCutLoop()
    {
        if (animator != null)
            animator.SetBool(cutBoolName, false);
    }

    private void LateUpdate()
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
    }
}