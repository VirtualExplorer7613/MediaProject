using UnityEngine;

public class OilStain : MonoBehaviour
{
    [Header("Cleaning")]
    [SerializeField] private float requiredScrubDistance = 2.5f;
    [SerializeField] private float requiredScrubTime = 2.0f;

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

        // 필요하면 이펙트/사운드
        // Managers.Sound.Play("Clean_Sponge");

        gameObject.SetActive(false);
        // 또는 Destroy(gameObject);
    }

    public void ResetProgress()
    {
        scrubDistanceAccum = 0f;
        scrubTimeAccum = 0f;
    }

}