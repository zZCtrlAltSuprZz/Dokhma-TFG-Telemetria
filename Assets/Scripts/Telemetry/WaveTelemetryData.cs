using UnityEngine;

[System.Serializable]
public class WaveTelemetryData
{
    [Header("Wave")]
    public int waveNumber;
    public float startTime;
    public float endTime;
    public float waveDuration;

    [Header("Enemies")]
    public int enemiesSpawned;
    public int enemiesKilled;
    public int meleeEnemiesKilled;
    public int rangedEnemiesKilled;

    [Header("Player Damage / Pressure")]
    public int damageTaken;
    public int damageEvents;
    public int playerDeaths;
    public int playerRevives;
    public int automaticHeals;

    public float lastDamageTime = -999f;
    public int currentDamageStreak;
    public int maxDamageStreak;

    [Header("Player Combat")]
    public int shotsFired;
    public int shotsHit;
    public int meleeAttacksUsed;
    public int meleeSuccessfulAttacks;
    public int meleeEnemiesHit;
    public int meleeDamageDealt;
    public int rangedDamageDealt;
    public int damageDealt;

    [Header("Weapons")]
    public int weaponChestsOpened;
    public int weaponsObtained;
    public int weaponSwitches;

    [Header("Resources")]
    public int soulsGained;
    public int soulsSpent;

    [Header("Progression")]
    public int roomsUnlocked;
    public int ritualsStarted;
    public int ritualsCompleted;
    public int perksChosen;

    [Header("End State")]
    public int playerHealthEnd;

    public float Accuracy
    {
        get
        {
            if (shotsFired <= 0) return 0f;
            return (float)shotsHit / shotsFired;
        }
    }

    public float MeleeAccuracy
    {
        get
        {
            if (meleeAttacksUsed <= 0) return 0f;
            return (float)meleeSuccessfulAttacks / meleeAttacksUsed;
        }
    }

    public float AverageEnemiesHitPerMelee
    {
        get
        {
            if (meleeAttacksUsed <= 0) return 0f;
            return (float)meleeEnemiesHit / meleeAttacksUsed;
        }
    }

    public float MeleeUseRatio
    {
        get
        {
            int totalAttacks = shotsFired + meleeAttacksUsed;
            if (totalAttacks <= 0) return 0f;
            return (float)meleeAttacksUsed / totalAttacks;
        }
    }

    public float KillRate
    {
        get
        {
            if (waveDuration <= 0f) return 0f;
            return enemiesKilled / waveDuration;
        }
    }

    public float PressureScore
    {
        get
        {
            return damageTaken + (damageEvents * 2f) + (maxDamageStreak * 5f) + playerDeaths * 25f;
        }
    }

    public void StartWave(int wNumber)
    {
        waveNumber = wNumber;
        startTime = Time.time;
    }

    public void RegisterDamageEvent(int damage, float streakWindowSeconds)
    {
        damageTaken += damage;
        damageEvents++;

        if (Time.time - lastDamageTime <= streakWindowSeconds)
        {
            currentDamageStreak++;
        }
        else
        {
            currentDamageStreak = 1;
        }

        if (currentDamageStreak > maxDamageStreak)
        {
            maxDamageStreak = currentDamageStreak;
        }

        lastDamageTime = Time.time;
    }

    public void EndWave(int playerHealth)
    {
        endTime = Time.time;
        waveDuration = endTime - startTime;
        playerHealthEnd = playerHealth;
    }

    // Index´s efficiency, pressure and aggression
    // Los índices se utilizan para analizar tendencias de comportamiento y ayudar a calibrar los perfiles de jugador.
    // Los umbrales definitivos se obtienen posteriormente a partir de las partidas de prueba.
    public float CombatEfficiency
    {
        get
        {
            float normalizedKillRate = Mathf.Clamp01(KillRate / 0.5f);
            return (Accuracy + normalizedKillRate) / 2f;
        }
    }

    public float PressureIndex
    {
        get
        {
            float damageEventsNormalized = Mathf.Clamp01(damageEvents / 10f);
            float damageStreakNormalized = Mathf.Clamp01(maxDamageStreak / 5f);
            float healsNormalized = Mathf.Clamp01(automaticHeals / 5f);

            return (damageEventsNormalized + damageStreakNormalized + healsNormalized) / 3f; 
        }
    }

    public float AggressionIndex
    {
        get
        {
            return (MeleeUseRatio + MeleeAccuracy) / 2f;
        }
    } 

    public string GetSummary()
    {
        return
            $"============================\n" +
            $"TELEMETRÍA OLEADA {waveNumber}\n" +
            $"============================\n\n" +

            // PROGRESIÓN
            $"[PROGRESIÓN]\n" +
            $"Duración: {waveDuration:F2}s\n" +
            $"Salas desbloqueadas: {roomsUnlocked}\n" +
            $"Rituales iniciados: {ritualsStarted}\n" +
            $"Rituales completados: {ritualsCompleted}\n" +
            $"Perks elegidos: {perksChosen}\n\n" +

            // COMBATE
            $"[COMBATE]\n" +
            $"Enemigos eliminados: {enemiesKilled}/{enemiesSpawned}\n" +
            $"   - Melee: {meleeEnemiesKilled}\n" +
            $"   - Ranged: {rangedEnemiesKilled}\n" +
            $"Kills por segundo: {KillRate:F2}\n\n" +

            $"Disparos realizados: {shotsFired}\n" +
            $"Impactos realizados: {shotsHit}\n" +
            $"Precisión disparos: {Accuracy * 100f:F1}%\n\n" +

            $"Ataques melee realizados: {meleeAttacksUsed}\n" +
            $"Uso de melee: {MeleeUseRatio * 100f:F1}%\n" +
            $"Precisión melee: {MeleeAccuracy * 100f:F1}%\n\n" +

            $"Daño total infligido: {damageDealt}\n" +
            $"   - A distancia: {rangedDamageDealt}\n" +
            $"   - Melee: {meleeDamageDealt}\n\n" +

            $"Cambios de arma: {weaponSwitches}\n" +

            // SUPERVIVENCIA
            $"\n[SUPERVIVENCIA]\n" +
            $"Daño recibido: {damageTaken}\n" +
            $"Golpes recibidos: {damageEvents}\n" +
            $"Racha máxima de daño: {maxDamageStreak}\n" +
            $"Regeneraciones automáticas: {automaticHeals}\n" +
            $"Muertes: {playerDeaths}\n" +
            $"Revives: {playerRevives}\n" +
            $"Vida final: {playerHealthEnd}\n" +
            $"Pressure Score: {PressureScore:F1}\n\n" +

            // ECONOMÍA
            $"[ECONOMÍA]\n" +
            $"Almas ganadas: {soulsGained}\n" +
            $"Almas gastadas: {soulsSpent}\n" +
            $"Cofres abiertos: {weaponChestsOpened}\n" +
            $"Armas obtenidas: {weaponsObtained}\n" +

            // INDICES
            $"\n[INDICADORES]\n" +
            $"Combat Efficiency: {CombatEfficiency:F2}\n" +
            $"Pressure Index: {PressureIndex:F2}\n" +
            $"Aggression Index: {AggressionIndex:F2}\n";
    }
}
