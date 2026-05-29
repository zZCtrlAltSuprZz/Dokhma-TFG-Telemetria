using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public static DifficultyManager Instance { get; private set; }

    public DifficultySettings CurrentSettings { get; private set; } = new DifficultySettings();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void UpdateDifficulty(PlayerProfile profile)
    {
        CurrentSettings = GenerateSettings(profile);

        Debug.Log(
           $"[DDA] Perfil: {profile}\n" +
           $"Enemy Count x{CurrentSettings.enemyCountMultiplier}\n" +
           $"Max Alive x{CurrentSettings.maxAliveMultiplier}\n" +
           $"Spawn Interval x{CurrentSettings.spawnIntervalMultiplier}\n" +
           $"Rest Time x{CurrentSettings.restTimeMultiplier}"
       );
    }

    private DifficultySettings GenerateSettings(PlayerProfile profile)
    {
        switch (profile)
        {
            case PlayerProfile.Dominant:
                return new DifficultySettings
                {
                    enemyCountMultiplier = 1.15f,
                    maxAliveMultiplier = 1.15f,
                    spawnIntervalMultiplier = 0.85f,
                    restTimeMultiplier = 0.85f
                };

            case PlayerProfile.Aggressive:
                return new DifficultySettings
                {
                    enemyCountMultiplier = 1.05f,
                    maxAliveMultiplier = 0.95f,
                    spawnIntervalMultiplier = 0.95f,
                    restTimeMultiplier = 1f
                };

            case PlayerProfile.Overwhelmed:
                return new DifficultySettings
                {
                    enemyCountMultiplier = 0.85f,
                    maxAliveMultiplier = 0.85f,
                    spawnIntervalMultiplier = 1.25f,
                    restTimeMultiplier = 1.25f
                };
            case PlayerProfile.Defensive:
            default:
                return new DifficultySettings
                {
                    enemyCountMultiplier = 1f,
                    maxAliveMultiplier = 1f,
                    spawnIntervalMultiplier = 0.95f,
                    restTimeMultiplier = 0.95f
                };
        }
    }
}