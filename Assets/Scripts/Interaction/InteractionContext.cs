using UnityEngine;

[System.Serializable]
public class InteractionContext
{
    public Transform player;
    public Camera cam;

    [Header("Ultrasound")]
    public GameObject ultrasoundPrefab;
    public Transform shootPoint;

    [Header("Layers")]
    public LayerMask trashLayerMask;

    public InteractionContext(Transform player, Camera cam)
    {
        this.player = player;
        this.cam = cam;
    }
}
