using UnityEngine;

namespace SHADERGEAR.OutlineEngine.Samples.Chess
{
    public class CursorManager : MonoBehaviour
    {
        void Update()
        {
            if (Input.GetMouseButton(2))
            {
                Cursor.lockState = CursorLockMode.Confined;
            }
            else 
            {
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
}