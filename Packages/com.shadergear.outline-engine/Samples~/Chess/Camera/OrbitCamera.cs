using UnityEngine;

namespace SHADERGEAR.OutlineEngine.Samples.Chess
{
    public class OrbitCamera : MonoBehaviour
    {
        [SerializeField]
        [Range(0.1f, 5f)]
        float movementSpeed = 2;

        [SerializeField]
        [Range(0.1f, 2f)]
        float zoomSpeed = 0.5f;

        Transform child;
        Vector2 rotation;
        float zoom;
        float zoomVelocity;
        float zoomSmoothTime = 0.1f;

        void Start()
        {
            child = transform.GetChild(0);

            rotation = transform.localRotation.eulerAngles;
            zoom = child.transform.localPosition.z;
        }

        void Update()
        {
            if (Input.GetMouseButton(2) || Input.GetMouseButton(1))
            {
                rotation.x += -Input.GetAxis("Mouse Y") * movementSpeed;
                rotation.y += Input.GetAxis("Mouse X") * movementSpeed;

                rotation.x = Mathf.Clamp(rotation.x, 0f, 180f);

                transform.rotation = Quaternion.Euler(rotation);
            }

            zoom += Input.mouseScrollDelta.y * zoomSpeed;

            zoom = Mathf.Clamp(zoom, -3f, -1f);

            var current = child.transform.localPosition.z;
            var target = zoom;
            var smoothedZoom = Mathf.SmoothDamp(current, target, ref zoomVelocity, zoomSmoothTime);
            child.transform.localPosition = new Vector3(0f, 0f, smoothedZoom);
        }
    }
}