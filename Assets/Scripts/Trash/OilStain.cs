using UnityEngine;

public class OilStain : MonoBehaviour
{
    [Header("Cleaning")]
    [SerializeField] private float requiredScrubDistance = 1.25f;
    [SerializeField] private float requiredScrubTime = 1.0f;

    [Header("Collect Effect")]
    [SerializeField] private GameObject collectVfxPrefab;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float vfxDestroyDelay = 1.5f;

    private float scrubDistanceAccum = 0f;
    private float scrubTimeAccum = 0f;
    private bool isCleaned = false;

    public bool IsCleaned => isCleaned;
    public float Progress01 => Mathf.Min(scrubDistanceAccum / requiredScrubDistance, scrubTimeAccum / requiredScrubTime);

    public void AddScrub(float movedDistance, float deltaTime)
    {
        if (isCleaned) return;

        scrubDistanceAccum += movedDistance;
        scrubTimeAccum += deltaTime;


        if (scrubDistanceAccum >= requiredScrubDistance &&
            scrubTimeAccum >= requiredScrubTime)
        {
            Clean();
        }
    }

    private void Clean()
    {
        if (isCleaned) return;

        isCleaned = true;
        Debug.Log("[OilStain] Cleaned!");

        SpawnCollectVfx();

        Managers.Sound.Play("Bubble_Surface_Large_01");

        gameObject.SetActive(false);
        // ¶Ç´Â Destroy(gameObject);
    }

    private void SpawnCollectVfx()
    {
        TrashVfxUtility.SpawnOneShot(
            collectVfxPrefab,
            transform.position,
            vfxOffset,
            vfxDestroyDelay
        );
    }

    public void ResetProgress()
    {
        scrubDistanceAccum = 0f;
        scrubTimeAccum = 0f;
    }

}