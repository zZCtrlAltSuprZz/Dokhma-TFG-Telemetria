using System;
using UnityEngine;

public static class GameTelemetryEvents
{
    public static event Action<int, int> OnPlayerDamaged;
    public static event Action OnPlayerDied;
    public static event Action OnPlayerRevived;
    public static event Action<int, int> OnPlayerHealed;

    public static event Action<string> OnShotFired;
    public static event Action<string, int> OnShotHit;
    public static event Action<string> OnMeleeAttackUsed;
    public static event Action<string, int> OnMeleeHit;

    public static event Action OnEnemySpawned;
    public static event Action OnEnemyKilled;

    public static event Action<int> OnWaveStarted;
    public static event Action<int> OnWaveCompleted;

    public static event Action<int> OnSoulsGained;
    public static event Action<int> OnSoulsSpent;

    public static void PlayerDamaged(int damage, int currentHealth) => OnPlayerDamaged?.Invoke(damage, currentHealth);

    public static void PlayerDied() => OnPlayerDied?.Invoke();

    public static void PlayerRevived() => OnPlayerRevived?.Invoke();

    public static void PlayerHealed(int amount, int currentHealth) => OnPlayerHealed?.Invoke(amount, currentHealth);

    public static void ShotFired(string weaponName) => OnShotFired?.Invoke(weaponName);

    public static void ShotHit(string weaponName, int damage) => OnShotHit?.Invoke(weaponName, damage);

    public static void MeleeAttackUsed(string weaponName) => OnMeleeAttackUsed?.Invoke(weaponName);

    public static void MeleeHit(string weaponName, int damage) => OnMeleeHit?.Invoke(weaponName, damage);

    public static void EnemySpawned() => OnEnemySpawned?.Invoke();

    public static void EnemyKilled() => OnEnemyKilled?.Invoke();

    public static void WaveStarted(int waveNumber) => OnWaveStarted?.Invoke(waveNumber);

    public static void WaveCompleted(int waveNumber) => OnWaveCompleted?.Invoke(waveNumber);

    public static void SoulsGained(int amount) => OnSoulsGained?.Invoke(amount);

    public static void SoulsSpent(int amount) => OnSoulsSpent?.Invoke(amount);
}