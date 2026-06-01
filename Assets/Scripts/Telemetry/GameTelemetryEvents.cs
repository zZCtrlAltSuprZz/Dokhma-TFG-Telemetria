using System;
using UnityEngine;

/// <summary>
/// Punto centralizado de comunicación entre los distintos sistemas
/// del juego para registrar eventos de telemetría.
/// </summary>
public static class GameTelemetryEvents
{
    // Supervivencia
    public static event Action<int, int> OnPlayerDamaged;
    public static event Action OnPlayerDied;
    public static event Action OnPlayerRevived;
    public static event Action<int, int> OnPlayerHealed;

    // Combate 
    public static event Action<string> OnShotFired;
    public static event Action<string, int> OnShotHit;
    public static event Action<string> OnMeleeAttackUsed;
    public static event Action<string, int> OnMeleeHit;
    public static event Action<string, int, int> OnMeleeSuccessfulAttack;

    public static event Action OnEnemySpawned;
    public static event Action<EnemyType> OnEnemyKilled;

    // Progresion y economía
    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveCompleted;

    public static event Action<int> OnSoulsGained;
    public static event Action<int> OnSoulsSpent;

    public static event Action OnWeaponChestOpened;
    public static event Action<string> OnWeaponObtained;
    public static event Action<string> OnWeaponSwitched;

    public static event Action<string> OnRoomUnlocked;

    public static event Action<string> OnRitualStarted;
    public static event Action<string> OnRitualCompleted;
    public static event Action<string> OnPerkChosen;

    public static void PlayerDamaged(int damage, int currentHealth) => OnPlayerDamaged?.Invoke(damage, currentHealth);

    public static void PlayerDied() => OnPlayerDied?.Invoke();

    public static void PlayerRevived() => OnPlayerRevived?.Invoke();

    public static void PlayerHealed(int amount, int currentHealth) => OnPlayerHealed?.Invoke(amount, currentHealth);

    public static void ShotFired(string weaponName) => OnShotFired?.Invoke(weaponName);

    public static void ShotHit(string weaponName, int damage) => OnShotHit?.Invoke(weaponName, damage);

    public static void MeleeAttackUsed(string weaponName) => OnMeleeAttackUsed?.Invoke(weaponName);

    public static void MeleeHit(string weaponName, int damage) => OnMeleeHit?.Invoke(weaponName, damage);

    public static void EnemySpawned() => OnEnemySpawned?.Invoke();

    public static void EnemyKilled(EnemyType enemyType) => OnEnemyKilled?.Invoke(enemyType);

    /// <summary>
    /// Notifica el inicio de una nueva oleada.
    /// </summary>
    public static void WaveStarted(int waveNumber) => OnWaveStarted?.Invoke(waveNumber);

    /// <summary>
    /// Notifica la finalización de una oleada y desencadena
    /// el procesamiento de las métricas registradas.
    /// </summary>
    public static void WaveCompleted(int waveNumber) => OnWaveCompleted?.Invoke(waveNumber);

    public static void SoulsGained(int amount) => OnSoulsGained?.Invoke(amount);

    public static void SoulsSpent(int amount) => OnSoulsSpent?.Invoke(amount);

    public static void WeaponChestOpened() => OnWeaponChestOpened?.Invoke();

    public static void WeaponObtained(string weaponName) => OnWeaponObtained?.Invoke(weaponName);

    public static void WeaponSwitched(string weaponName) => OnWeaponSwitched?.Invoke(weaponName);

    public static void RoomUnlocked(string roomName) => OnRoomUnlocked?.Invoke(roomName);

    public static void RitualStarted(string ritualName) => OnRitualStarted?.Invoke(ritualName);

    public static void RitualCompleted(string ritualName) => OnRitualCompleted?.Invoke(ritualName);

    public static void PerkChosen(string perkName) => OnPerkChosen?.Invoke(perkName);

    public static void MeleeSuccessfulAttack(string weaponName, int enemiesHit, int totalDamage) => OnMeleeSuccessfulAttack?.Invoke(weaponName, enemiesHit, totalDamage);

}