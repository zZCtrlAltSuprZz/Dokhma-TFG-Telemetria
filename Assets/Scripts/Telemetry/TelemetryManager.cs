using UnityEngine;
using System.Collections.Generic;


public class TelemetryManager : MonoBehaviour
{
    public static TelemetryManager Instance { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool printWaveSummary = true;

    [Header("Damage Pressure")]
    [SerializeField] private float recoveryWindow => playerHealth.HealDelay;

    [Header("References")]
    [SerializeField] private PlayerHealth playerHealth;

    private WaveTelemetryData currentWaveData;
    private readonly List<WaveTelemetryData> completedWaves = new();

    public IReadOnlyList<WaveTelemetryData> CompletedWaves => completedWaves;
    public WaveTelemetryData CurrentWaveData => currentWaveData;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        GameTelemetryEvents.OnWaveStarted += HandleWaveStarted;
        GameTelemetryEvents.OnWaveCompleted += HandleWaveCompleted;

        GameTelemetryEvents.OnEnemySpawned += HandleEnemySpawned;
        GameTelemetryEvents.OnEnemyKilled += HandleEnemyKilled;

        GameTelemetryEvents.OnPlayerDamaged += HandlePlayerDamaged;
        GameTelemetryEvents.OnPlayerDied += HandlePlayerDied;
        GameTelemetryEvents.OnPlayerRevived += HandlePlayerRevived;
        GameTelemetryEvents.OnPlayerHealed += HandlePlayerHealed;

        GameTelemetryEvents.OnShotFired += HandleShotFired;
        GameTelemetryEvents.OnShotHit += HandleShotHit;

        GameTelemetryEvents.OnMeleeAttackUsed += HandleMeleeAttackUsed;
        GameTelemetryEvents.OnMeleeSuccessfulAttack += HandleMeleeSuccessfulAttack;

        GameTelemetryEvents.OnSoulsGained += HandleSoulsGained;
        GameTelemetryEvents.OnSoulsSpent += HandleSoulsSpent;

        GameTelemetryEvents.OnWeaponChestOpened += HandleWeaponChestOpened;
        GameTelemetryEvents.OnWeaponObtained += HandleWeaponObtained;
        GameTelemetryEvents.OnWeaponSwitched += HandleWeaponSwitched;

        GameTelemetryEvents.OnRoomUnlocked += HandleRoomUnlocked;

        GameTelemetryEvents.OnRitualStarted += HandleRitualStarted;
        GameTelemetryEvents.OnRitualCompleted += HandleRitualCompleted;
        GameTelemetryEvents.OnPerkChosen += HandlePerkChosen;
    }

    private void OnDisable()
    {
        GameTelemetryEvents.OnWaveStarted -= HandleWaveStarted;
        GameTelemetryEvents.OnWaveCompleted -= HandleWaveCompleted;

        GameTelemetryEvents.OnEnemySpawned -= HandleEnemySpawned;
        GameTelemetryEvents.OnEnemyKilled -= HandleEnemyKilled;

        GameTelemetryEvents.OnPlayerDamaged -= HandlePlayerDamaged;
        GameTelemetryEvents.OnPlayerDied -= HandlePlayerDied;
        GameTelemetryEvents.OnPlayerRevived -= HandlePlayerRevived;
        GameTelemetryEvents.OnPlayerHealed -= HandlePlayerHealed;

        GameTelemetryEvents.OnShotFired -= HandleShotFired;
        GameTelemetryEvents.OnShotHit -= HandleShotHit;

        GameTelemetryEvents.OnMeleeAttackUsed -= HandleMeleeAttackUsed;
        GameTelemetryEvents.OnMeleeSuccessfulAttack -= HandleMeleeSuccessfulAttack;

        GameTelemetryEvents.OnSoulsGained -= HandleSoulsGained;
        GameTelemetryEvents.OnSoulsSpent -= HandleSoulsSpent;

        GameTelemetryEvents.OnWeaponChestOpened -= HandleWeaponChestOpened;
        GameTelemetryEvents.OnWeaponObtained -= HandleWeaponObtained;
        GameTelemetryEvents.OnWeaponSwitched -= HandleWeaponSwitched;

        GameTelemetryEvents.OnRoomUnlocked -= HandleRoomUnlocked;

        GameTelemetryEvents.OnRitualStarted -= HandleRitualStarted;
        GameTelemetryEvents.OnRitualCompleted -= HandleRitualCompleted;
        GameTelemetryEvents.OnPerkChosen -= HandlePerkChosen;
    }

    private void HandleWaveStarted(int waveNumber)
    {
        currentWaveData = new WaveTelemetryData();
        currentWaveData.StartWave(waveNumber);
    }

    private void HandleWaveCompleted(int waveNumber)
    {
        if (currentWaveData == null) return;

        int currentHealth = 0;

        if (playerHealth != null)
        {
            currentHealth = playerHealth.CurrentLives;
        }

        currentWaveData.EndWave(currentHealth);
        completedWaves.Add(currentWaveData);

        if (printWaveSummary)
        {
            Debug.Log(currentWaveData.GetSummary());
        }

        PlayerProfile profile = PlayerProfileAnalyzer.Analyze(currentWaveData);
        Debug.Log($"Player Profile for Wave {waveNumber}: {profile}");

        if (DifficultyManager.Instance != null)
        {
            DifficultyManager.Instance.UpdateDifficulty(profile);
        }

        currentWaveData = null;
    }

    private void HandleEnemySpawned()
    {
        if (currentWaveData == null) return;
        currentWaveData.enemiesSpawned++;
    }

    private void HandleEnemyKilled(EnemyType enemyType)
    {
        if (currentWaveData == null) return;
        currentWaveData.enemiesKilled++;

        switch(enemyType)
        {
            case EnemyType.Melee:
                currentWaveData.meleeEnemiesKilled++;
                break;
            case EnemyType.Ranged:
                currentWaveData.rangedEnemiesKilled++;
                break;
        }
    }

    private void HandlePlayerDamaged(int damage, int currentHealth)
    {
        if (currentWaveData == null) return;
        currentWaveData.RegisterDamageEvent(damage, recoveryWindow);
    }

    private void HandlePlayerDied()
    {
        if (currentWaveData == null) return;
        currentWaveData.playerDeaths++;
    }

    private void HandlePlayerRevived()
    {
        if (currentWaveData == null) return;
        currentWaveData.playerRevives++;
    }

    private void HandlePlayerHealed(int amount, int currentHealth)
    {
        if (currentWaveData == null) return;
        currentWaveData.automaticHeals++;
    }

    private void HandleShotFired(string weaponName)
    {
        if (currentWaveData == null) return;
        currentWaveData.shotsFired++;
    }

    private void HandleShotHit(string weaponName, int damage)
    {
        if (currentWaveData == null) return;
        currentWaveData.shotsHit++;
        currentWaveData.rangedDamageDealt += damage;
        currentWaveData.damageDealt += damage;
    }

    private void HandleMeleeAttackUsed(string weaponName)
    {
        if (currentWaveData == null) return;
        currentWaveData.meleeAttacksUsed++;
    }

  

    private void HandleMeleeSuccessfulAttack(string weaponName, int enemiesHit, int damage)
    {
        if (currentWaveData == null) return;

        currentWaveData.meleeSuccessfulAttacks++;
        currentWaveData.meleeEnemiesHit += enemiesHit;
        currentWaveData.meleeDamageDealt += damage;
        currentWaveData.damageDealt += damage;
    }

    private void HandleSoulsGained(int amount)
    {
        if (currentWaveData == null) return;
        currentWaveData.soulsGained += amount;
    }

    private void HandleSoulsSpent(int amount)
    {
        if (currentWaveData == null) return;
        currentWaveData.soulsSpent += amount;
    }

    private void HandleWeaponChestOpened()
    {
        if (currentWaveData == null) return;
        currentWaveData.weaponChestsOpened++;
    }

    private void HandleWeaponObtained(string weaponName)
    {
        if (currentWaveData == null) return;
        currentWaveData.weaponsObtained++;
    }

    private void HandleWeaponSwitched(string weaponName)
    {
        if (currentWaveData == null) return;
        currentWaveData.weaponSwitches++;
    }

    private void HandleRoomUnlocked(string roomName)
    {
        if (currentWaveData == null) return;
        currentWaveData.roomsUnlocked++;
    }

    private void HandleRitualStarted(string ritualName)
    {
        if (currentWaveData == null) return;
        currentWaveData.ritualsStarted++;
    }

    private void HandleRitualCompleted(string ritualName)
    {
        if (currentWaveData == null) return;
        currentWaveData.ritualsCompleted++;
    }

    private void HandlePerkChosen(string perkName)
    {
        if (currentWaveData == null) return;
        currentWaveData.perksChosen++;
    }
}

