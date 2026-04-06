using UnityEngine;

public class ItemPreviewRotator : MonoBehaviour
{
    private float rotateSpeed = 30f;

    public void SetSpeed(float speed)
    {
        rotateSpeed = speed;
    }

    private void Update()
    {
        transform.Rotate(0f, rotateSpeed * Time.unscaledDeltaTime, 0f, Space.Self);
    }
}