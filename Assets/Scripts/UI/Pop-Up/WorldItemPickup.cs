using UnityEngine;

public class WorldItemPickup : MonoBehaviour
{
    [SerializeField] private ItemDefinition itemDef;
    [SerializeField] private bool destroyWorldObjectAfterPickup = true;

    private bool popupOpened = false;

    public void ShowPickupPopup()
    {
        if (popupOpened) return;
        popupOpened = true;

        if (itemDef == null)
        {
            Debug.LogWarning("[WorldItemPickup] itemDef is null");
            popupOpened = false;
            return;
        }

        if (ItemPickupPopup.Instance == null)
        {
            Debug.LogWarning("[WorldItemPickup] ItemPickupPopup.Instance is null");
            popupOpened = false;
            return;
        }

        ItemPickupPopup.Instance.Show(itemDef);

        if (destroyWorldObjectAfterPickup)
        {
            gameObject.SetActive(false);
        }
    }

}
