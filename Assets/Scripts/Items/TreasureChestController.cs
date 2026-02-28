using System.Collections;
using UnityEngine;

public class TreasureChestController : MonoBehaviour
{

    private InteractionController ic;
    private InventoryUIController invUI;
    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;

    public enum ChestState { Closed, Opening, Opened, Looted }
    public ChestState State => state;

    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private string openTriggerName = "Open";
    [Header("Distances")]
    [SerializeField] private float useKeyDistance = 3f;
    [SerializeField] private float collectDistance = 3f;

    [Header("Reward (옵션)")]
    [SerializeField] private ItemDefinition rewardItem;

    [Header("Reward VFX")]
    [SerializeField] private GameObject rewardPrefab;
    [SerializeField] private Transform rewardSpawnPoint;
    [SerializeField] private float rewardVfxLifeTime = 1.5f;

    [Header("Collect Options")]
    [SerializeField] private bool autoCollectOnOpen = true;
    [SerializeField] private bool spawnRewardOnOpen = true;

    private bool rewardSpawned = false;

    private ChestState state = ChestState.Closed;

    private void Awake()
    {
        if (itemUseBroadcaster == null)
        {
            itemUseBroadcaster = FindAnyObjectByType<ItemUseBroadcaster>();
        }
        if (animator == null) animator = GetComponentInChildren<Animator>();

        ic = FindFirstObjectByType<InteractionController>();
        invUI = FindFirstObjectByType<InventoryUIController>();

        if (ic == null)
        {
            Debug.LogError("[Chest] InteractionController not found in scene!");
        }
        if (invUI == null)
        {
            Debug.LogError("[Chest] InventoryUIController not found in scene!");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[Chest] TriggerEnter with: {other.name}, tag={other.tag}");

        if (!other.CompareTag("Player")) return;

        if (ic == null) return;

        ic.CTX.currentChest = this;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (ic == null) return;

        if (ic.CTX.currentChest == this)
            ic.CTX.currentChest = null;
    }

    public bool TryUseKey(Transform player)
    {
        if (player == null) return false;
        if (state != ChestState.Closed) return false;
        if (Vector3.Distance(transform.position, player.position) > useKeyDistance) return false;

        StartCoroutine(OpenRoutine());
        // 상자 열림 트리거
        ic.CTX.itemUseBroadcaster?.RaiseItemUsed("chest_open");
        return true;
    }

    IEnumerator OpenRoutine()
    {
        state = ChestState.Opening;

        animator.ResetTrigger(openTriggerName);
        animator.SetTrigger(openTriggerName);
        
        yield return new WaitForSeconds(0.3f);

        state = ChestState.Opened;

        if (spawnRewardOnOpen)
        {
            SpawnRewardVFXOnce();
        }

        if (autoCollectOnOpen)
        {
            TryCollect(ic != null ? ic.CTX.player : null);
        }
    }

    public bool TryCollect(Transform player)
    {
        Debug.Log($"[Chest] TryCollect called. state={state}, rewardItem={(rewardItem ? rewardItem.name : "null")}, invUI={(invUI ? "ok" : "null")}");

        if (player == null) return false;
        if (state != ChestState.Opened) return false;
        if (Vector3.Distance(transform.position, player.position) > collectDistance) return false;

        state = ChestState.Looted;

        if (rewardPrefab != null)
        {
            SpawnRewardVFXOnce();
        }

        if (invUI != null && rewardItem != null)
        {
            bool added = invUI.AddItem(rewardItem);
            Debug.Log($"[Chest] AddItem result = {added}");
            if (added)
                itemUseBroadcaster?.RaiseItemUsed("magnet_earned");

            // 아이템 획득 트리거
        }
        else
        {
            Debug.LogWarning("[Chest] rewardItem or InventoryUIController is missing.");
        }

        Debug.Log($"[Chest] TryCollect called. state={state}, rewardItem={(rewardItem ? rewardItem.name : "null")}, invUI={(invUI ? "ok" : "null")}");
        return true;
    }

    private void SpawnRewardVFXOnce()
    {
        if (rewardSpawned) return;
        if (rewardPrefab == null) return;

        var pos = rewardSpawnPoint ? rewardSpawnPoint.position : (transform.position + Vector3.up * 0.5f);
        var baseRot = rewardSpawnPoint ? rewardSpawnPoint.rotation : transform.rotation;
        Instantiate(rewardPrefab, pos, baseRot);
        rewardSpawned = true;
    }

    void Update()
    {
        
    }
}
