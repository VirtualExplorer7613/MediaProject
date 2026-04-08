using UnityEngine;
using UnityEngine.UI;

public class WaveButtonController : MonoBehaviour
{
    [SerializeField] private Button waveButton;
    [SerializeField] private WhaleController whaleController;
    [SerializeField] private float cooldown = 1f;

    private float nextAvailableTime = 0f;

    private void Awake()
    {
        if (waveButton == null)
            waveButton = GetComponent<Button>();

        if (whaleController == null)
            whaleController = FindFirstObjectByType<WhaleController>();

        if (waveButton != null)
        {
            waveButton.onClick.RemoveAllListeners();
            waveButton.onClick.AddListener(OnClickWave);
        }
    }

    private void OnClickWave()
    {
        if (Time.time < nextAvailableTime) return;

        if (whaleController == null)
        {
            Debug.LogWarning("[WaveButton] WhaleController not found.");
            return;
        }

        whaleController.FireUltrasound();
        nextAvailableTime = Time.time + cooldown;
    }
}
