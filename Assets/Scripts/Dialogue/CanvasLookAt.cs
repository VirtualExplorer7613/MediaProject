using UnityEngine;

public class CanvasLookAt : MonoBehaviour
{
    void Update()
    {
        if (Camera.main != null)
        {
            transform.LookAt(Camera.main.transform);
            transform.Rotate(0, 180, 0); // 뒤집힘 보정
        }
    }
}
