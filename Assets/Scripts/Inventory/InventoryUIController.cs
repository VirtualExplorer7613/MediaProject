using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InventoryUIController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button inventoryToggleButton;
    [SerializeField] private GameObject inventoryPanel; // 가로 리스트 패널(켜고 끄기)
    [SerializeField] private Transform contentRoot;     // HorizontalLayoutGroup이 붙은 Content
    [SerializeField] private InventoryItemButton itemButtonPrefab;

    [Header("Popup")]
    [SerializeField] private ItemInventoryEquipPopup itemEquipPopup;

    [Header("Items")]
    [SerializeField] private List<ItemDefinition> items = new();

    [Header("Target")]
    [SerializeField] private InteractionController interactionController;

    [Header("SFX")]
    [SerializeField] private string clickSfx = "SFX/Click";

    private InventoryItemButton selectedButton;

    private readonly Dictionary<ItemDefinition, InventoryItemButton> buttonMap = new();

    [SerializeField] private ItemUseBroadcaster itemUseBroadcaster;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (itemUseBroadcaster == null)
        {
            itemUseBroadcaster = FindAnyObjectByType<ItemUseBroadcaster>();
        }

        if (inventoryToggleButton != null)
            inventoryToggleButton.onClick.AddListener(ToggleInventory);

        BuildButtons();

        ItemDefinition defToSelect = interactionController != null ? interactionController.CurrentItemDef : null;

        if (defToSelect == null && items.Count > 0)
            defToSelect = items[0];

        if (defToSelect != null && buttonMap.TryGetValue(defToSelect, out var btn))
        {
            // 버튼 레퍼런스를 넘겨서 선택 표시가 켜지게 함
            SelectItem(defToSelect, btn, false);
        }
        else if (items.Count > 0 && buttonMap.TryGetValue(items[0], out var firstBtn))
        {
            // 마지막 안전장치
            SelectItem(items[0], firstBtn, false);
        }

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);
    }

    private void ToggleInventory()
    {
        if (inventoryPanel == null) return;
        Managers.Sound.Play(clickSfx);
        inventoryPanel.SetActive(!inventoryPanel.activeSelf);
        itemUseBroadcaster?.RaiseItemUsed("inventory_open");
    }

    private void BuildButtons()
    {
        if (contentRoot == null || itemButtonPrefab == null) return;

        buttonMap.Clear();

        // 기존 버튼 제거(에디터 반복 실행 대비)
        for (int i = contentRoot.childCount - 1; i >= 0; i--)
            Destroy(contentRoot.GetChild(i).gameObject);

        foreach (var def in items)
        {
            var btn = Instantiate(itemButtonPrefab, contentRoot);
            btn.Bind(def, this);

            buttonMap[def] = btn;
        }
    }

    // 아이템 설명 팝업창
    public void OpenItemPopup(ItemDefinition def, InventoryItemButton clicked)
    {
        if (def == null)
            return;

        Managers.Sound.Play(clickSfx);

        if (itemEquipPopup != null)
        {
            itemEquipPopup.Show(def, clicked, this);
        }
        else
        {
            Debug.LogWarning("[InventoryUIController] itemEquipPopup is not assigned");

            // 팝업이 없을 때만 기존 방식으로 fallback
            SelectItem(def, clicked);
        }
    }

    public void SelectItem(ItemDefinition def, InventoryItemButton clicked, bool playSfx = true)
    {
        // UI 선택 표시
        if (selectedButton != null) selectedButton.SetSelected(false);
        selectedButton = clicked;
        if (selectedButton != null) selectedButton.SetSelected(true);

        // 상호작용 교체
        if (interactionController != null && def != null)
            interactionController.SetItem(def);

        // 선택 후 자동 접힘
        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (playSfx)
            Managers.Sound.Play(clickSfx);

        Debug.Log($"{def.name} equipped!! this is button");

        if (def.itemType == ItemType.Key)
        {
            itemUseBroadcaster?.RaiseItemUsed("key_equipped");
        }
        else if (def.itemType == ItemType.Magnet)
        {
            itemUseBroadcaster?.RaiseItemUsed("magnet_equipped");
        }
    }

    public bool AddItem(ItemDefinition def, bool autoSelect = false)
    {
        if (def == null) return false;

        // 중복방지
        if (items.Contains(def)) return false;

        items.Add(def);

        if (contentRoot != null && itemButtonPrefab != null)
        {
            var btn = Instantiate(itemButtonPrefab, contentRoot);
            btn.Bind(def, this);
            buttonMap[def] = btn;

            if (autoSelect)
            {
                SelectItem(def, btn);
            }
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
