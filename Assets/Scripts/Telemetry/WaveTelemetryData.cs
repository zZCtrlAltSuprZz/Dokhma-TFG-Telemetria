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
    public int meleeHits;
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

    public string GetSummary()
    {
        return
            $"--- TELEMETRÍA OLEADA {waveNumber} ---\n" +
            $"Duración: {waveDuration:F2}s\n" +
            $"Enemigos generados: {enemiesSpawned}\n" +
            $"Enemigos eliminados: {enemiesKilled}\n" +
            $"Melee eliminados: {meleeEnemiesKilled}\n" +
            $"Ranged eliminados: {rangedEnemiesKilled}\n" +
            $"Daño recibido: {damageTaken}\n" +
            $"Golpes recibidos: {damageEvents}\n" +
            $"Racha máxima de daño: {maxDamageStreak}\n" +
            $"Regeneraciones automáticas: {automaticHeals}\n" +
            $"Muertes: {playerDeaths}\n" +
            $"Revives: {playerRevives}\n" +
            $"Disparos: {shotsFired}\n" +
            $"Impactos: {shotsHit}\n" +
            $"Precisión: {Accuracy * 100f:F1}%\n" +
            $"Melee usados: {meleeAttacksUsed}\n" +
            $"Melee impactos: {meleeHits}\n" +
            $"Ratio melee: {MeleeUseRatio * 100f:F1}%\n" +
            $"Daño infligido: {damageDealt}\n" +
            $"Almas ganadas: {soulsGained}\n" +
            $"Almas gastadas: {soulsSpent}\n" +
            $"Cofres abiertos: {weaponChestsOpened}\n" +
            $"Armas obtenidas: {weaponsObtained}\n" +
            $"Cambios de arma: {weaponSwitches}\n" +
            $"Salas desbloqueadas: {roomsUnlocked}\n" +
            $"Rituales iniciados: {ritualsStarted}\n" +
            $"Rituales completados: {ritualsCompleted}\n" +
            $"Perks elegidos: {perksChosen}\n" +
            $"Vida final: {playerHealthEnd}\n" +
            $"Pressure Score: {PressureScore:F1}";
    }
}
