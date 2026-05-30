using System.IO;
using UnityEngine;

public static class TelemetryCSVExporter
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "telemetry_results.csv");
    public static void ExportWave(WaveTelemetryData data, PlayerProfile profile)
    {
        bool fileExist = File.Exists(FilePath);

        using StreamWriter writer = new StreamWriter(FilePath, true);

        if (!fileExist)
        {
            writer.WriteLine(GetHeader());
        }

        writer.WriteLine(GetRow(data, profile));

        Debug.Log($"[CSV] Datos exportados en: {FilePath}");
    }

    private static string GetHeader()
    {
        return
            "waveNumber;" +
            "waveDuration;" +
            "enemiesSpawned;" +
            "enemiesKilled;" +
            "killRate;" +
            "shotsFired;" +
            "shotsHit;" +
            "accuracy;" +
            "meleeAttacksUsed;" +
            "meleeSuccessfulAttacks;" +
            "meleeEnemiesHit;" +
            "meleeUseRatio;" +
            "meleeAccuracy;" +
            "damageDealt;" +
            "rangedDamageDealt;" +
            "meleeDamageDealt;" +
            "damageTaken;" +
            "damageEvents;" +
            "maxDamageStreak;" +
            "automaticHeals;" +
            "pressureScore;" +
            "playerHealthEnd;" +
            "soulsGained;" +
            "soulsSpent;" +
            "weaponSwitches;" +
            "profile;" +
            "difficultyMode;" +
            "enemyCountMultiplier;" +
            "maxAliveMultiplier;" +
            "spawnIntervalMultiplier;" +
            "restTimeMultiplier";
    }

    private static string GetRow(WaveTelemetryData data, PlayerProfile profile)
    {
        DifficultySettings settings = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.CurrentSettings 
            : new DifficultySettings();

        string mode = DifficultyManager.Instance != null
            ? DifficultyManager.Instance.CurrentMode.ToString()
            : "Unknown";

        return
            $"{data.waveNumber};" +
            $"{data.waveDuration:F2};" +
            $"{data.enemiesSpawned};" +
            $"{data.enemiesKilled};" +
            $"{data.KillRate:F3};" +
            $"{data.shotsFired};" +
            $"{data.shotsHit};" +
            $"{data.Accuracy:F3};" +
            $"{data.meleeAttacksUsed};" +
            $"{data.meleeSuccessfulAttacks};" +
            $"{data.meleeEnemiesHit};" +
            $"{data.MeleeUseRatio:F3};" +
            $"{data.MeleeAccuracy:F3};" +
            $"{data.damageDealt};" +
            $"{data.rangedDamageDealt};" +
            $"{data.meleeDamageDealt};" +
            $"{data.damageTaken};" +
            $"{data.damageEvents};" +
            $"{data.maxDamageStreak};" +
            $"{data.automaticHeals};" +
            $"{data.PressureScore:F2};" +
            $"{data.playerHealthEnd};" +
            $"{data.soulsGained};" +
            $"{data.soulsSpent};" +
            $"{data.weaponSwitches};" +
            $"{profile};" +
            $"{mode};" +
            $"{settings.enemyCountMultiplier:F2};" +
            $"{settings.maxAliveMultiplier:F2};" +
            $"{settings.spawnIntervalMultiplier:F2};" +
            $"{settings.restTimeMultiplier:F2}";
    }
}
