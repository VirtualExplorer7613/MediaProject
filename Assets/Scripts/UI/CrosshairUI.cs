using UnityEngine;
using UnityEngine.UI;

public class CrosshairUI : MonoBehaviour
{
    public Texture2D crosshairTexture;
    [SerializeField]
    private float size = 100f;

    void OnGUI()
    {
        float x = (Screen.width - size) / 2;
        float y = (Screen.height - size) / 2;
        GUI.DrawTexture(new Rect(x, y, size, size), crosshairTexture);
    }
}
