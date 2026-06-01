using System.IO;
using UnityEngine;
using System.Globalization;

/// <summary>
/// Exporta los resultados de telemetría a un archivo CSV para
/// su posterior análisis estadístico.
/// </summary>
public static class TelemetryCSVExporter
{
    private static string FilePath => Path.Combine(Application.persistentDataPath, "telemetry_results.csv");

    private static string F(float value, string format = "F2")
    {
        return value.ToString(format, CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Añade una nueva fila de datos correspondiente a una oleada.
    /// </summary>
    /// <param name="data">Datos de telemetría registrados.</param>
    /// <param name="profile">Perfil identificado para la oleada.</param>
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

    /// <summary>
    /// Genera la cabecera del archivo CSV.
    /// </summary>
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
            "combatEfficiency;" +
            "pressureIndex;" +
            "aggressionIndex" +
            "profile;" +
            "difficultyMode;" +
            "enemyCountMultiplier;" +
            "maxAliveMultiplier;" +
            "spawnIntervalMultiplier;" +
            "restTimeMultiplier";
    }

    /// <summary>
    /// Convierte los datos de la oleada en una fila compatible con CSV.
    /// </summary>
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
            $"{F(data.waveDuration):F2};" +
            $"{data.enemiesSpawned};" +
            $"{data.enemiesKilled};" +
            $"{F(data.KillRate):F3};" +
            $"{data.shotsFired};" +
            $"{data.shotsHit};" +
            $"{F(data.Accuracy):F3};" +
            $"{data.meleeAttacksUsed};" +
            $"{data.meleeSuccessfulAttacks};" +
            $"{data.meleeEnemiesHit};" +
            $"{F(data.MeleeUseRatio):F3};" +
            $"{F(data.MeleeAccuracy):F3};" +
            $"{data.damageDealt};" +
            $"{data.rangedDamageDealt};" +
            $"{data.meleeDamageDealt};" +
            $"{data.damageTaken};" +
            $"{data.damageEvents};" +
            $"{data.maxDamageStreak};" +
            $"{data.automaticHeals};" +
            $"{F(data.PressureScore):F2};" +
            $"{data.playerHealthEnd};" +
            $"{data.soulsGained};" +
            $"{data.soulsSpent};" +
            $"{data.weaponSwitches};" +
            $"{F(data.CombatEfficiency):F3};" +
            $"{F(data.PressureIndex):F3};" +
            $"{F(data.AggressionIndex):F3};" +
            $"{profile};" +
            $"{mode};" +
            $"{F(settings.enemyCountMultiplier):F2};" +
            $"{F(settings.maxAliveMultiplier):F2};" +
            $"{F(settings.spawnIntervalMultiplier):F2};" +
            $"{F(settings.restTimeMultiplier):F2}";
    }


}
