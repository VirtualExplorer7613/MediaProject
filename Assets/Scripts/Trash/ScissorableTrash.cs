using UnityEngine;

public class ScissorableTrash : MonoBehaviour
{
    [SerializeField] private float requiredHoldSeconds = 3.0f;

    [Header("VFX")]
    [SerializeField] private GameObject cutVfxPrefab;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float vfxDestroyDelay = 2f;

    [Header("Scissor Visual Placement")]
    [SerializeField] private Transform scissorVisualAnchor;
    [SerializeField] private Vector3 scissorVisualOffset = Vector3.zero;
    [SerializeField] private Vector3 scissorVisualRotationOffsetEuler = Vector3.zero;
    [SerializeField] private Vector3 scissorVisualScaleMultiplier = Vector3.one;

    public float RequiredHoldSeconds => requiredHoldSeconds;

    public bool HasScissorVisualAnchor => scissorVisualAnchor != null;
    public Transform ScissorVisualAnchor => scissorVisualAnchor != null ? scissorVisualAnchor : transform;
    public Vector3 ScissorVisualOffset => scissorVisualOffset;
    public Vector3 ScissorVisualRotationOffsetEuler => scissorVisualRotationOffsetEuler;
    public Vector3 ScissorVisualScaleMultiplier => scissorVisualScaleMultiplier;

    public void CutComplete()
    {
        SpawnCutVfx();
        Destroy(gameObject);
    }

    private void SpawnCutVfx()
    {
        TrashVfxUtility.SpawnOneShot(
            cutVfxPrefab,
            transform.position,
            vfxOffset,
            vfxDestroyDelay
        );
    }
}