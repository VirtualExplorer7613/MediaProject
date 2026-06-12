using UnityEngine;
using UnityEngine.UI;

public class InventoryItemButton : MonoBehaviour
{
    [SerializeField] private Button button;
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject selectedOutline; // 선택 테두리

    private ItemDefinition def;
    private InventoryUIController owner;

    public ItemDefinition Item => def;
    public RectTransform RectTransform => transform as RectTransform;
    public GameObject RootObject => gameObject;

    public void Bind(ItemDefinition def, InventoryUIController owner)
    {
        this.def = def;
        this.owner = owner;

        if (iconImage != null)
            iconImage.sprite = def.icon;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            //button.onClick.AddListener(() => owner.SelectItem(def, this));
            button.onClick.AddListener(() => owner.OpenItemPopup(def, this));
        }

        SetSelected(false);
    }

    // 선택되면 테두리 변경(추후 구현)
    public void SetSelected(bool selected)
    {
        if (selectedOutline != null)
            selectedOutline.SetActive(selected);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
