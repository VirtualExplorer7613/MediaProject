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

    [Header("Tutorial")]
    [SerializeField] private TutorialHighlighter tutorialHighlighter;

    [Header("Tutorial Item Button Lock")]
    [SerializeField] private bool useTutorialItemButtonLock = true;

    [SerializeField] private ItemType lockedItemType = ItemType.Key;

    [Tooltip("이 튜토리얼 단계 전에는 해당 아이템 버튼 클릭을 막음")]
    [SerializeField] private int unlockItemButtonAtTutorialStepIndex = 15;

    [SerializeField] private bool showLockedLog = true;

    private TutorialRunner tutorialRunner;

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

        tutorialRunner = FindAnyObjectByType<TutorialRunner>();

        if (tutorialRunner != null)
            tutorialRunner.StepChanged += OnTutorialStepChanged;

        if (inventoryToggleButton != null)
            inventoryToggleButton.onClick.AddListener(ToggleInventory);

        BuildButtons();
        RefreshTutorialItemButtonLocks();

        ItemDefinition defToSelect = interactionController != null ? interactionController.CurrentItemDef : null;

        if (defToSelect == null || IsItemButtonLockedByTutorial(defToSelect))
            defToSelect = FindFirstUnlockedItem();

        /*if (defToSelect == null && items.Count > 0)
            defToSelect = items[0];
*/
        /*if (defToSelect != null && buttonMap.TryGetValue(defToSelect, out var btn))
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
            inventoryPanel.SetActive(false);*/

        if (defToSelect != null && buttonMap.TryGetValue(defToSelect, out var btn))
        {
            SelectItem(defToSelect, btn, false);
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

            RegisterTutorialSlot(def, btn);
        }
    }

    // 아이템 설명 팝업창
    public void OpenItemPopup(ItemDefinition def, InventoryItemButton clicked)
    {
        if (def == null)
            return;

        if (IsItemButtonLockedByTutorial(def))
        {
            if (showLockedLog)
            {
                Debug.Log(
                    $"[InventoryUI] 아직 튜토리얼 단계가 부족해서 아이템 버튼을 사용할 수 없습니다. " +
                    $"item={def.name}, currentStep={GetCurrentTutorialStepIndex()}, requiredStep={unlockItemButtonAtTutorialStepIndex}"
                );
            }

            RefreshTutorialItemButtonLocks();
            return;
        }

        Managers.Sound.Play(clickSfx);

        if (itemEquipPopup != null)
        {
            itemEquipPopup.Show(def, clicked, this);

            if (def.itemType == ItemType.Key)
                itemUseBroadcaster?.RaiseItemUsed("key_clicked");
            else if (def.itemType == ItemType.Magnet)
                itemUseBroadcaster?.RaiseItemUsed("magnet_clicked");
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

            RegisterTutorialSlot(def, btn);
            RefreshTutorialItemButtonLocks();

            if (autoSelect)
            {
                SelectItem(def, btn);
            }
        }

        return true;
    }

    private void RegisterTutorialSlot(ItemDefinition def, InventoryItemButton btn)
    {
        if (tutorialHighlighter == null || def == null || btn == null)
            return;

        string id = GetTutorialSlotHighlightId(def);

        if (string.IsNullOrEmpty(id))
            return;

        tutorialHighlighter.RegisterTarget(
            id,
            btn.RootObject,
            btn.RectTransform,
            null,
            FocusMask.HoleShape.Circle
        );
    }

    private string GetTutorialSlotHighlightId(ItemDefinition def)
    {
        switch (def.itemType)
        {
            case ItemType.Key:
                return "inventory_key_slot";

            case ItemType.Magnet:
                return "inventory_magnet_slot";

            default:
                return "";
        }
    }

    public void NotifyItemPopupOpened(ItemDefinition def)
    {
        if (itemUseBroadcaster == null || def == null)
            return;

        if (def.itemType == ItemType.Key)
        {
            itemUseBroadcaster.RaiseItemUsed("key_popup_opened");
        }
        else if (def.itemType == ItemType.Magnet)
        {
            itemUseBroadcaster.RaiseItemUsed("magnet_popup_opened");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTutorialStepChanged(int stepIndex)
    {
        RefreshTutorialItemButtonLocks();
    }

    private void OnDestroy()
    {
        if (tutorialRunner != null)
            tutorialRunner.StepChanged -= OnTutorialStepChanged;
    }

    private bool IsItemButtonLockedByTutorial(ItemDefinition def)
    {
        if (!useTutorialItemButtonLock)
            return false;

        if (def == null)
            return false;

        if (def.itemType != lockedItemType)
            return false;

        if (tutorialRunner == null)
            tutorialRunner = FindAnyObjectByType<TutorialRunner>();

        if (tutorialRunner == null)
            return false;

        return !tutorialRunner.IsStepOrLater(unlockItemButtonAtTutorialStepIndex);
    }

    private int GetCurrentTutorialStepIndex()
    {
        if (tutorialRunner == null)
            tutorialRunner = FindAnyObjectByType<TutorialRunner>();

        return tutorialRunner != null ? tutorialRunner.CurrentStepIndex : -1;
    }

    private void RefreshTutorialItemButtonLocks()
    {
        foreach (var pair in buttonMap)
        {
            ItemDefinition def = pair.Key;
            InventoryItemButton button = pair.Value;

            if (button == null)
                continue;

            bool locked = IsItemButtonLockedByTutorial(def);
            button.SetInteractable(!locked);
        }
    }

    private ItemDefinition FindFirstUnlockedItem()
    {
        foreach (var def in items)
        {
            if (def == null)
                continue;

            if (!IsItemButtonLockedByTutorial(def))
                return def;
        }

        return null;
    }
}
