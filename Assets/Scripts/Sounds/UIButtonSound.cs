using UnityEngine;

public class UIButtonSound : MonoBehaviour
{
    [SerializeField]
    private string effectName;

    public void PlaySound()
    {
        Managers.Sound.Play(effectName);
    }
}
