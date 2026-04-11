using UnityEngine;

public class ScissorableTrash : MonoBehaviour
{
    [SerializeField] private float requiredHoldSeconds = 3.0f;

    [Header("VFX")]
    [SerializeField] private GameObject cutVfxPrefab;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float vfxDestroyDelay = 2f;

    public float RequiredHoldSeconds => requiredHoldSeconds;

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