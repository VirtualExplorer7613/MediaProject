using UnityEngine;

public class ScissorableTrash : MonoBehaviour
{
    [SerializeField] private float requiredHoldSeconds = 3.0f;

    public float RequiredHoldSeconds => requiredHoldSeconds;

    public void CutComplete()
    {
        Destroy(gameObject);
    }
}