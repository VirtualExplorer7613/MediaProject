using UnityEngine;

[System.Serializable]
public class InteractionContext
{
    public Transform player;
    public Camera cam;

    [Header("Ultrasound")]
    public GameObject ultrasoundPrefab;
    public Transform shootPoint;

    [Header("Magnet")]
    public GameObject magnetVisual;

    [Header("Hammer")]
    public HammerVisual hammerVisual;

    [Header("Scissor")]
    public ScissorVisual scissorVisual;

    [Header("Sponge")]
    public GameObject spongeVisual;
    public LayerMask spongeSurfaceMask;
    public LayerMask oilStainMask;


    [Header("Chest")]
    public TreasureChestController currentChest;

    [Header("Layers")]
    public LayerMask trashLayerMask;

    public ItemUseBroadcaster itemUseBroadcaster;

    public InteractionContext(Transform player, Camera cam)
    {
        this.player = player;
        this.cam = cam;
    }

    public void SetMagnetVisual(bool on)
    {
        if (magnetVisual != null)
            magnetVisual.SetActive(on);
    }

    public void SetSpongeVisual(bool on)
    {
        if (spongeVisual != null)
            spongeVisual.SetActive(on);
    }
}

