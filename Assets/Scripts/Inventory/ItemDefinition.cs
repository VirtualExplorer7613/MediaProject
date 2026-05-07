using UnityEngine;

public enum ItemType
{
    Ultrasound,
    Magnet,
    Brush,
    Key,
    Hammer,
    Scissor,
    Sponge,
    Pull,
}

[CreateAssetMenu(menuName = "Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public ItemType itemType;
    public Sprite icon;

    [Header("Popup Info")]
    public string displayName;
    [TextArea(2, 4)] public string description;
    public GameObject previewPrefab;

    [Header("Preview Transform")]
    public Vector3 previewLocalPosition = Vector3.zero;
    public Vector3 previewLocalEuler = Vector3.zero;
    public Vector3 previewLocalScale = Vector3.one;
    public float previewRotateSpeed = 50f;

    [Header("Common")]
    public float cooldownSeconds = 0f;

    [Header("Magnet")]
    public float magnetRadius = 8f;
    public int magnetMaxTargets = 3;

    [Header("Brush")]
    public float brushHoldSeconds = 1.5f;
    public float brushCleanRadius = 5f;
    public int brushMaxCleanCount = 10;

    [Header("Pull")]
    public float pullMaxDistance = 20f;
}
