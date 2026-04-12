using Unity.VisualScripting;
using UnityEngine;



public class ItemEarn : MonoBehaviour
{
    private InteractionController ic;
    private InventoryUIController invUI;
    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;
    [SerializeField] private ItemDefinition rewardItem;
    private string itemName;
    [SerializeField] private WorldItemPickup rewardPickup;

    [Header("Tutorial Gate")]
    [SerializeField] private TutorialRunner tutorialRunner;
    [SerializeField] private bool restrictByTutorialStep = false;
    [SerializeField] private int allowedTutorialStepIndex = 0;
    [SerializeField] private bool allowAfterStep = false;

    private bool picked = false;

    private void Awake()
    {
        itemName = gameObject.name;

        ic = FindFirstObjectByType<InteractionController>();
        invUI = FindFirstObjectByType<InventoryUIController>();

        if (tutorialRunner == null)
            tutorialRunner = FindFirstObjectByType<TutorialRunner>();

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
        if (picked) return;
        if (!other.CompareTag("Player")) return;

        if (!CanPickupNow())
        {
            Debug.Log($"{itemName}: 현재 튜토리얼 단계가 아니어서 획득 불가");
            return;
        }
        picked = true;

        if (rewardPickup != null)
        {
            rewardPickup.ShowPickupPopup();
        }
        else if (invUI != null && rewardItem != null)
        {
            bool added = invUI.AddItem(rewardItem);
            Debug.Log($"{itemName} AddItem result = {added}");
        }

        // 인벤토리에 키 추가 튜토리얼 트리거
        itemUseBroadcaster?.RaiseItemUsed("key");
        Destroy(gameObject);
    }

    private bool CanPickupNow()
    {
        if (!restrictByTutorialStep)
            return true;

        if (tutorialRunner == null)
        {
            Debug.LogWarning($"{itemName}: TutorialRunner is null");
            return false;
        }

        if (allowAfterStep)
            return tutorialRunner.IsStepOrLater(allowedTutorialStepIndex);

        return tutorialRunner.IsCurrentStep(allowedTutorialStepIndex);
    }
}
