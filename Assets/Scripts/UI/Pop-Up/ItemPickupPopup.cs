using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.AudioSettings;

public class ItemPickupPopup : MonoBehaviour
{
    public static ItemPickupPopup Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Texts")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button acquireButton;

    [Header("Preview")]
    //[SerializeField] private Transform previewRoot;

    [SerializeField] private Transform previewSpawnRoot;
    [SerializeField] private RawImage previewImage;
    [SerializeField] private RenderTexture previewTexture;
    [SerializeField] private Camera previewCamera;

    [Header("Optional UI To Hide")]
    [SerializeField] private GameObject mobileUI;

    [Header("Options")]
    [SerializeField] private bool pauseGameWhileOpen = true;
    [SerializeField] private bool autoEquipOnPickup = false;

    [Header("Tutorial")]
    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;

    [Header("SFX")]
    [SerializeField] private string popupOpenSfx = "SFX/Item Popup";
    [SerializeField] private string clickSfx = "SFX/Click";


    private InventoryUIController inventoryUI;
    private InteractionController interactionController;

    private ItemDefinition currentItem;
    private GameObject currentPreviewInstance;
    private bool alreadyConfirmed = false;
    private float prevTimeScale = 1f;

    public bool IsShowing => root != null && root.activeSelf;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        inventoryUI = FindFirstObjectByType<InventoryUIController>();
        interactionController = FindFirstObjectByType<InteractionController>();

        if (itemUseBroadcaster == null)
            itemUseBroadcaster = FindFirstObjectByType<ItemUseBroadcaster>();

        if (closeButton != null)
            closeButton.onClick.AddListener(ConfirmPickup);

        if (acquireButton != null)
            acquireButton.onClick.AddListener(ConfirmPickup);

        if (root != null)
            root.SetActive(false);

        if (previewImage != null && previewTexture != null)
            previewImage.texture = previewTexture;

        if (previewCamera != null && previewTexture != null)
            previewCamera.targetTexture = previewTexture;
    }

    public void Show(ItemDefinition item)
    {
        if (item == null)
        {
            Debug.LogWarning("[ItemPickupPopup] item is null");
            return;
        }

        currentItem = item;
        alreadyConfirmed = false;

        if (itemNameText != null)
        {
            itemNameText.text = string.IsNullOrEmpty(item.displayName) ? item.name : item.displayName;
        }

        if (descriptionText != null)
        {
            descriptionText.text = item.description;
        }

        CreatePreview(item);

        if (root != null)
            root.SetActive(true);

        if (mobileUI != null)
            mobileUI.SetActive(false);

        if (pauseGameWhileOpen)
        {
            prevTimeScale = Time.timeScale;
            Time.timeScale = 0f;
        }

        // 팝업 등장 사운드
        Managers.Sound.Play(popupOpenSfx);
    }

    private void CreatePreview(ItemDefinition item)
    {
        ClearPreview();

        if (previewSpawnRoot == null)
        {
            Debug.LogWarning("[ItemPickupPopup] previewRoot is null");
            return;
        }

        if (previewImage != null && previewTexture != null)
            previewImage.texture = previewTexture;

        if (previewCamera != null && previewTexture != null)
            previewCamera.targetTexture = previewTexture;

        if (item.previewPrefab == null)
        {
            Debug.LogWarning($"[ItemPickupPopup] previewPrefab missing for {item.name}");
            return;
        }

        currentPreviewInstance = Instantiate(item.previewPrefab, previewSpawnRoot);
        currentPreviewInstance.transform.localPosition = item.previewLocalPosition;
        currentPreviewInstance.transform.localRotation = Quaternion.Euler(item.previewLocalEuler);
        currentPreviewInstance.transform.localScale = item.previewLocalScale;

        Debug.Log($"[ItemPreview] item={item.name}, type={item.itemType}");

        var rotator = currentPreviewInstance.GetComponent<ItemPreviewRotator>();
        if (rotator == null)
            rotator = currentPreviewInstance.AddComponent<ItemPreviewRotator>();

        if (item.itemType == ItemType.Pull)
        {
            // 1. 현재 프리팹의 Local Z축을 월드 위쪽으로 보정
            AlignLocalAxisToWorld(
                currentPreviewInstance.transform,
                Vector3.forward,
                Vector3.up
            );

            Debug.Log($"[ItemPreview] Pull aligned forward={currentPreviewInstance.transform.forward}");

            // 이제 Local Z축 == World Up 이므로 World Up 기준 회전
            rotator.Setup(
                item.previewRotateSpeed,
                Vector3.up,
                Space.World
            );
        }
        else
        {
            // 일반 아이템은 기존처럼 Local Y축 회전
            rotator.Setup(
                item.previewRotateSpeed,
                Vector3.up,
                Space.Self
            );
        }
    }

    private void AlignLocalAxisToWorld(Transform target, Vector3 localAxis, Vector3 worldAxis)
    {
        Vector3 currentWorldAxis = target.TransformDirection(localAxis.normalized);

        Quaternion correction = Quaternion.FromToRotation(
            currentWorldAxis,
            worldAxis.normalized
        );

        target.rotation = correction * target.rotation;
    }

    private void ConfirmPickup()
    {
        if (alreadyConfirmed) return;
        alreadyConfirmed = true;

        // 버튼 클릭음
        Managers.Sound.Play(clickSfx);

        if (currentItem != null && inventoryUI != null)
        {
            bool added = inventoryUI.AddItem(currentItem, autoEquipOnPickup);

            if (added)
            {
                Debug.Log($"[ItemPickupPopup] Added item: {currentItem.name}");
                RaisePickupEventByItemType(currentItem);
                // 획득 사운드 재생
                // Managers.Sound.Play("재생하고자 하는 사운드");
            }
            else
            {
                Debug.Log($"[ItemPickupPopup] Item already exists: {currentItem.name}");
            }
        }
        Close();
    }

    public void Close()
    {
        ClearPreview();

        currentItem = null;

        if (root != null)
            root.SetActive(false);

        if (mobileUI != null)
            mobileUI.SetActive(true);

        if (pauseGameWhileOpen)
            Time.timeScale = prevTimeScale;
    }

    private void ClearPreview()
    {
        if (currentPreviewInstance != null)
        {
            Destroy(currentPreviewInstance);
            currentPreviewInstance = null;
        }

        if (previewSpawnRoot != null)
        {
            for (int i = previewSpawnRoot.childCount - 1; i >= 0; i--)
            {
                Destroy(previewSpawnRoot.GetChild(i).gameObject);
            }
        }
    }

    private void RaisePickupEventByItemType(ItemDefinition item)
    {
        if (itemUseBroadcaster == null || item == null) return;

        switch (item.itemType)
        {
            case ItemType.Ultrasound:
                itemUseBroadcaster.RaiseItemUsed("wave_earned");
                break;
            case ItemType.Magnet:
                itemUseBroadcaster.RaiseItemUsed("magnet_earned");
                break;
            case ItemType.Brush:
                itemUseBroadcaster.RaiseItemUsed("brush_earned");
                break;
            case ItemType.Key:
                itemUseBroadcaster.RaiseItemUsed("key_earned");
                break;
            case ItemType.Hammer:
                itemUseBroadcaster.RaiseItemUsed("hammer_earned");
                break;
            case ItemType.Scissor:
                itemUseBroadcaster.RaiseItemUsed("scissor_earned");
                break;
            case ItemType.Sponge:
                itemUseBroadcaster.RaiseItemUsed("sponge_earned");
                break;
            case ItemType.Pull:
                itemUseBroadcaster.RaiseItemUsed("pull_earned");
                break;
        }
    }
}
