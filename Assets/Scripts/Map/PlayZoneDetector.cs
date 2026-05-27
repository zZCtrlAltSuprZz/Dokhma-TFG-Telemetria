using UnityEngine;

public class PlayerZoneDetector : MonoBehaviour
{
    [SerializeField] private GameZone zone;
    [SerializeField] private WaveManager waveManager;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        waveManager.SetCurrentZone(zone);
    }
}