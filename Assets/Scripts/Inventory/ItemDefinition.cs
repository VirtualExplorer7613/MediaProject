using UnityEngine;

public enum ItemType
{
    Ultrasound,
    Magnet,
    Brush,
    Key,
    Hammer,
    Scissor,
}

[CreateAssetMenu(menuName = "Item Definition")]
public class ItemDefinition : ScriptableObject
{
    public ItemType itemType;
    public Sprite icon;

    [Header("Common")]
    public float cooldownSeconds = 0f;

    [Header("Magnet")]
    public float magnetRadius = 8f;
    public int magnetMaxTargets = 3;

    [Header("Brush")]
    public float brushHoldSeconds = 1.5f;
    public float brushCleanRadius = 5f;
    public int brushMaxCleanCount = 10;
}
