using TMPro;
using UnityEngine;

public class DeathScreenUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TMP_Text soulsText;
    [SerializeField] private TMP_Text killsText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text waveText;

    private float gameStartTime;
    private bool shown;

    private void Awake()
    {
        gameStartTime = Time.time;

        if (deathPanel != null)
            deathPanel.SetActive(false);
    }

    private void OnEnable()
    {
        GameTelemetryEvents.OnPlayerDied += ShowDeathScreen;
    }

    private void OnDisable()
    {
        GameTelemetryEvents.OnPlayerDied -= ShowDeathScreen;
    }

    private void ShowDeathScreen()
    {
        Debug.Log("DEATH SCREEN CALLED");

        if (shown) return;
        shown = true;

        int totalSouls = 0;
        int totalKills = 0;
        int reachedWave = 0;

        if (TelemetryManager.Instance != null)
        {
            foreach (WaveTelemetryData wave in TelemetryManager.Instance.CompletedWaves)
            {
                totalSouls += wave.soulsGained;
                totalKills += wave.enemiesKilled;
                reachedWave = Mathf.Max(reachedWave, wave.waveNumber);
            }

            WaveTelemetryData currentWave = TelemetryManager.Instance.CurrentWaveData;

            if (currentWave != null)
            {
                totalSouls += currentWave.soulsGained;
                totalKills += currentWave.enemiesKilled;
                reachedWave = Mathf.Max(reachedWave, currentWave.waveNumber);
            }
        }

        float totalTime = Time.time - gameStartTime;

        if (deathPanel != null)
            deathPanel.SetActive(true);

        if (soulsText != null)
            soulsText.text = "Souls obtained: " + totalSouls;

        if (killsText != null)
            killsText.text = "Enemies killed: " + totalKills;

        if (timeText != null)
            timeText.text = "Game duration: " + FormatTime(totalTime);

        if (waveText != null)
            waveText.text = "Round reached: " + reachedWave;

        if (MusicManager.Instance != null)
            MusicManager.Instance.PlayDeathMusic();

        Time.timeScale = 0f;
    }

    private string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        int secs = Mathf.FloorToInt(seconds % 60f);

        return minutes.ToString("00") + ":" + secs.ToString("00");
    }
}