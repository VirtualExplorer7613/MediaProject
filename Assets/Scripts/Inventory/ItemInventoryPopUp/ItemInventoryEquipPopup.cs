using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemInventoryEquipPopup : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;

    [Header("Texts")]
    [SerializeField] private TMP_Text itemNameText;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Buttons")]
    [SerializeField] private Button equipButton;
    [SerializeField] private Button cancelButton;

    [Header("Preview")]
    [SerializeField] private Transform previewSpawnRoot;
    [SerializeField] private RawImage previewImage;
    [SerializeField] private RenderTexture previewTexture;
    [SerializeField] private Camera previewCamera;

    [Header("SFX")]
    //[SerializeField] private string popupOpenSfx = "SFX/Item Popup";
    [SerializeField] private string clickSfx = "SFX/Click";

    private ItemDefinition currentItem;
    private InventoryItemButton currentButton;
    private InventoryUIController owner;

    private GameObject currentPreviewInstance;

    private void Awake()
    {
        if (equipButton != null)
            equipButton.onClick.AddListener(EquipCurrentItem);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(Close);

        if (root != null)
            root.SetActive(false);

        if (previewImage != null && previewTexture != null)
            previewImage.texture = previewTexture;

        if (previewCamera != null && previewTexture != null)
            previewCamera.targetTexture = previewTexture;
    }

    public void Show(ItemDefinition item, InventoryItemButton button, InventoryUIController owner)
    {
        if (item == null || owner == null)
        {
            Debug.LogWarning("[ItemInventoryEquipPopup] item or owner is null");
            return;
        }

        currentItem = item;
        currentButton = button;
        this.owner = owner;

        if (itemNameText != null)
        {
            itemNameText.text = string.IsNullOrEmpty(item.displayName)
                ? item.name
                : item.displayName;
        }

        if (descriptionText != null)
            descriptionText.text = item.description;

        CreatePreview(item);

        if (root != null)
            root.SetActive(true);

        owner.NotifyItemPopupOpened(item);
        //Managers.Sound.Play(popupOpenSfx);
    }

    private void EquipCurrentItem()
    {
        if (currentItem == null || owner == null)
            return;

        Managers.Sound.Play(clickSfx);

        owner.SelectItem(currentItem, currentButton);

        Close();
    }

    public void Close()
    {
        Managers.Sound.Play(clickSfx);

        ClearPreview();

        currentItem = null;
        currentButton = null;
        owner = null;

        if (root != null)
            root.SetActive(false);
    }

    private void CreatePreview(ItemDefinition item)
    {
        ClearPreview();

        if (previewSpawnRoot == null)
            return;

        if (previewImage != null && previewTexture != null)
            previewImage.texture = previewTexture;

        if (previewCamera != null && previewTexture != null)
            previewCamera.targetTexture = previewTexture;

        if (item.previewPrefab == null)
            return;

        currentPreviewInstance = Instantiate(item.previewPrefab, previewSpawnRoot);

        currentPreviewInstance.transform.localPosition = item.previewLocalPosition;
        currentPreviewInstance.transform.localRotation = Quaternion.Euler(item.previewLocalEuler);
        currentPreviewInstance.transform.localScale = item.previewLocalScale;

        var rotator = currentPreviewInstance.GetComponent<ItemPreviewRotator>();
        if (rotator == null)
            rotator = currentPreviewInstance.AddComponent<ItemPreviewRotator>();

        rotator.SetSpeed(item.previewRotateSpeed);
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
}