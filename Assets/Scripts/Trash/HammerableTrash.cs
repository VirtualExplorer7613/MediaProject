using UnityEngine;

public class HammerableTrash : MonoBehaviour
{
    [Header("Hammer Hits")]
    [SerializeField] private int hitsToBreak = 6;
    [SerializeField] private float resetWindowSeconds = 1.0f; // 연타 끊기면 누적 초기화

    [Header("SFX")]
    [SerializeField] private string breakSfxPath = "SFX/glass";

    [Header("VFX")]
    [SerializeField] private GameObject breakVfxPrefab;
    [SerializeField] private Vector3 vfxOffset = Vector3.zero;
    [SerializeField] private float vfxDestroyDelay = 1.5f;

    private int hits;
    private float lastHitTime;

    public bool ApplyHit(int damage = 1)
    {
        Debug.Log("Try Hit by Hammer!");
        // 연타 텀이 길면 콤보 끊김 -> 누적 초기화
        if (Time.time - lastHitTime > resetWindowSeconds)
            hits = 0;

        lastHitTime = Time.time;
        hits += damage;

        // 여기서 맞는 VFX/사운드 넣고 싶으면 호출

        if (hits >= hitsToBreak)
        {
            SpawnBreakVfx();
            Managers.Sound.Play(breakSfxPath);
            Destroy(gameObject);
            return true;
        }
        return false;
    }

    public float Progress01 => hitsToBreak <= 0 ? 1f : Mathf.Clamp01((float)hits / hitsToBreak);

    private void SpawnBreakVfx()
    {
        TrashVfxUtility.SpawnOneShot(
            breakVfxPrefab,
            transform.position,
            vfxOffset,
            vfxDestroyDelay
        );
    }
}