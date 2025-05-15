using UnityEngine;

[System.Serializable]
public class VisualTextStep
{
    public Sprite image;
    [TextArea(2, 5)]
    public string text;
}