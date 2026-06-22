using UnityEngine;

public class ItemPreviewRotator : MonoBehaviour
{
    private float rotateSpeed = 30f;
    private Vector3 rotateAxis = Vector3.up;
    private Space rotateSpace = Space.Self;

    public void SetSpeed(float speed)
    {
        rotateSpeed = speed;
    }

    public void SetAxis(Vector3 axis)
    {
        rotateAxis = axis.sqrMagnitude <= 0.0001f
            ? Vector3.up
            : axis.normalized;
    }

    public void Setup(float speed, Vector3 axis, Space space = Space.Self)
    {
        SetSpeed(speed);
        SetAxis(axis);
        rotateSpace = space;
    }

    private void Update()
    {
        transform.Rotate(
            rotateAxis,
            rotateSpeed * Time.unscaledDeltaTime,
            rotateSpace
        );
    }
}