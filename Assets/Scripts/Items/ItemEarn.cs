using Unity.VisualScripting;
using UnityEngine;



public class ItemEarn : MonoBehaviour
{
    private InteractionController ic;
    private InventoryUIController invUI;
    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;
    [SerializeField] private ItemDefinition rewardItem;
    private string itemName;

    private void Awake()
    {
        itemName = gameObject.name;

        ic = FindFirstObjectByType<InteractionController>();
        invUI = FindFirstObjectByType<InventoryUIController>();

        if (ic == null)
        {
            Debug.LogError($"{itemName}: InteractionController not found in scene!");
        }
        if (invUI == null)
        {
            Debug.LogError($"{itemName}: InventoryUIController not found in scene!");
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (invUI != null && rewardItem != null)
        {
            bool added = invUI.AddItem(rewardItem);
            Debug.Log($"{itemName} AddItem result = {added}");
        }

        // 인벤토리에 키 추가 튜토리얼 트리거
        itemUseBroadcaster?.RaiseItemUsed("key");
        Destroy(gameObject);
    }
}
