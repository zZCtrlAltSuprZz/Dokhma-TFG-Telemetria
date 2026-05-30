using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public class WaveEnemyEntry
{
    public string enemyName;
    public GameObject enemyPrefab;
    public int amount = 1;
}

[System.Serializable]
public class WaveConfig
{
    public string waveName = "Wave";

    [Header("Enemies")]
    public WaveEnemyEntry[] enemies;

    [Header("Spawn Limits")]
    public int maxAliveEnemies = 10;
    public float spawnInterval = 0.5f;

    [Header("Normal Enemy Spawn Distance")]
    public float maxSpawnDistanceToPlayer = 20f;
    public float minSpawnDistanceToPlayer = 4f;

    [Header("Ranged Sky Enemy Spawn")]
    public float rangedSpawnRadiusAroundPlayer = 2f;
}

public class WaveManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;

    [Header("UI")]
    [SerializeField] private TMP_Text waveText;
    [SerializeField] private TMP_Text remainingEnemiesText;

    [Header("Waves")]
    [SerializeField] private int startingWave = 1;
    [SerializeField] private WaveConfig[] waves;

    [Header("Between Waves")]
    [SerializeField] private float timeBetweenWaves = 3f;

    private EnemySpawnPoint[] allSpawnPoints;
    private GameZone currentZone = GameZone.ZoneLobby;

    private int currentWave;
    private int enemiesToSpawnThisWave;
    private int enemiesSpawnedThisWave;
    private int enemiesKilledThisWave;
    private int aliveEnemies;

    private bool waveInProgress;
    private bool spawningWave;

    private WaveConfig currentWaveConfig;
    private readonly List<GameObject> spawnQueue = new List<GameObject>();

    private void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        allSpawnPoints = FindObjectsByType<EnemySpawnPoint>(FindObjectsSortMode.None);

        currentWave = startingWave - 1;
        StartNextWave();
    }

    private void Update()
    {
        if (!waveInProgress) return;

        bool allSpawned = enemiesSpawnedThisWave >= enemiesToSpawnThisWave;
        bool allDead = aliveEnemies <= 0;

        if (allSpawned && allDead && !spawningWave)
        {
            // Telemetry
            GameTelemetryEvents.WaveCompleted(currentWave);
            waveInProgress = false;
            StartCoroutine(BeginNextWaveAfterDelay());

        }
    }

    private IEnumerator BeginNextWaveAfterDelay()
    {
        float restMultiplier = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentSettings.restTimeMultiplier : 1f;

        yield return new WaitForSeconds(timeBetweenWaves * restMultiplier);
        StartNextWave();
    }

    private void StartNextWave()
    {
        currentWave++;

        currentWaveConfig = GetWaveConfig(currentWave);

        if (currentWaveConfig == null)
        {
            Debug.LogError("No hay configuración para la ronda " + currentWave);
            return;
        }

        WaveConfig adjustedConfig = CreateAdjustedWaveConfig(currentWaveConfig);
        BuildSpawnQueue(adjustedConfig);
        currentWaveConfig = adjustedConfig;

        enemiesToSpawnThisWave = spawnQueue.Count;
        enemiesSpawnedThisWave = 0;
        enemiesKilledThisWave = 0;
        aliveEnemies = 0;

        waveInProgress = true;
        spawningWave = true;

        UpdateWaveUI();

        Debug.Log($"Empieza ronda {currentWave}. Enemigos: {enemiesToSpawnThisWave}");

        // Telemetry
        GameTelemetryEvents.WaveStarted(currentWave);

        StartCoroutine(SpawnWaveRoutine());
    }
    private WaveConfig CreateAdjustedWaveConfig(WaveConfig baseConfig)
    {
        DifficultySettings settings = DifficultyManager.Instance != null ? DifficultyManager.Instance.CurrentSettings : new DifficultySettings();

        WaveConfig adjusted = new WaveConfig
        {
            waveName = baseConfig.waveName,
            maxAliveEnemies = Mathf.Max(1, Mathf.RoundToInt(baseConfig.maxAliveEnemies * settings.maxAliveMultiplier)),
            spawnInterval = Mathf.Max(0.1f, baseConfig.spawnInterval * settings.spawnIntervalMultiplier),
            maxSpawnDistanceToPlayer = baseConfig.maxSpawnDistanceToPlayer,
            minSpawnDistanceToPlayer = baseConfig.minSpawnDistanceToPlayer,
            rangedSpawnRadiusAroundPlayer = baseConfig.rangedSpawnRadiusAroundPlayer
        };

        if (baseConfig.enemies != null)
        {
            adjusted.enemies = new WaveEnemyEntry[baseConfig.enemies.Length];

            for (int i = 0; i < baseConfig.enemies.Length; i++)
            {
                WaveEnemyEntry baseEntry = baseConfig.enemies[i];

                adjusted.enemies[i] = new WaveEnemyEntry
                {
                    enemyName = baseEntry.enemyName,
                    enemyPrefab = baseEntry.enemyPrefab,
                    amount = Mathf.Max(1, Mathf.RoundToInt(baseEntry.amount * settings.enemyCountMultiplier))
                };
            }
        }

        Debug.Log( $"[DDA]  {baseConfig.waveName}\n" + $"MaxAlive: {baseConfig.maxAliveEnemies} -> {adjusted.maxAliveEnemies}\n" + $"SpawnInterval: {baseConfig.spawnInterval} -> {adjusted.spawnInterval}");

        return adjusted;
    }
    private WaveConfig GetWaveConfig(int waveNumber)
    {
        if (waves == null || waves.Length == 0)
            return null;

        int index = waveNumber - 1;

        if (index < waves.Length)
            return waves[index];

        return waves[waves.Length - 1];
    }

    private void BuildSpawnQueue(WaveConfig config)
    {
        spawnQueue.Clear();

        if (config.enemies == null) return;

        for (int i = 0; i < config.enemies.Length; i++)
        {
            WaveEnemyEntry entry = config.enemies[i];

            if (entry == null || entry.enemyPrefab == null || entry.amount <= 0)
                continue;

            for (int j = 0; j < entry.amount; j++)
                spawnQueue.Add(entry.enemyPrefab);
        }

        ShuffleSpawnQueue();
    }

    private void ShuffleSpawnQueue()
    {
        for (int i = 0; i < spawnQueue.Count; i++)
        {
            int randomIndex = Random.Range(i, spawnQueue.Count);

            GameObject temp = spawnQueue[i];
            spawnQueue[i] = spawnQueue[randomIndex];
            spawnQueue[randomIndex] = temp;
        }
    }

    private IEnumerator SpawnWaveRoutine()
    {
        while (enemiesSpawnedThisWave < enemiesToSpawnThisWave)
        {
            if (aliveEnemies < currentWaveConfig.maxAliveEnemies)
            {
                GameObject prefabToSpawn = spawnQueue[enemiesSpawnedThisWave];

                if (prefabToSpawn != null)
                    SpawnEnemy(prefabToSpawn);
            }

            yield return new WaitForSeconds(currentWaveConfig.spawnInterval);
        }

        spawningWave = false;
    }

    private void SpawnEnemy(GameObject prefabToSpawn)
    {
        Vector3 spawnPos;

        if (IsRangedSkyEnemy(prefabToSpawn))
            spawnPos = GetRangedSkySpawnPosition();
        else
            spawnPos = GetNormalEnemySpawnPosition();

        GameObject enemy = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);

        // Telemetry
        GameTelemetryEvents.EnemySpawned();

        enemiesSpawnedThisWave++;
        aliveEnemies++;

        InitEnemy(enemy);
        UpdateWaveUI();
    }

    private bool IsRangedSkyEnemy(GameObject prefab)
    {
        return prefab.GetComponentInChildren<RangedSkyEnemy>() != null;
    }

    private Vector3 GetRangedSkySpawnPosition()
    {
        if (player == null)
            return Vector3.zero;

        Vector2 randomCircle = Random.insideUnitCircle * currentWaveConfig.rangedSpawnRadiusAroundPlayer;

        Vector3 targetPos = player.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            return hit.position;

        return player.position;
    }

    private Vector3 GetNormalEnemySpawnPosition()
    {
        EnemySpawnPoint spawnPoint = GetRandomValidSpawnPoint();

        if (spawnPoint != null)
            return spawnPoint.GetSpawnPosition();

        return player != null ? player.position + player.forward * 8f : Vector3.zero;
    }

    private EnemySpawnPoint GetRandomValidSpawnPoint()
    {
        if (player == null || allSpawnPoints == null || allSpawnPoints.Length == 0)
            return null;

        List<EnemySpawnPoint> validPoints = new List<EnemySpawnPoint>();
        Vector3 playerPos = player.position;

        for (int i = 0; i < allSpawnPoints.Length; i++)
        {
            EnemySpawnPoint point = allSpawnPoints[i];

            if (point == null || !point.enabledForSpawning)
                continue;

            if (point.zone != currentZone)
                continue;

            float dist = point.DistanceTo(playerPos);

            if (dist <= currentWaveConfig.maxSpawnDistanceToPlayer &&
                dist >= currentWaveConfig.minSpawnDistanceToPlayer)
            {
                validPoints.Add(point);
            }
        }

        if (validPoints.Count == 0)
            return null;

        validPoints.Sort((a, b) => a.DistanceTo(playerPos).CompareTo(b.DistanceTo(playerPos)));

        int count = Mathf.Min(5, validPoints.Count);
        return validPoints[Random.Range(0, count)];
    }

    private void InitEnemy(GameObject enemy)
    {
        SimpleEnemy simple = enemy.GetComponentInChildren<SimpleEnemy>();
        if (simple != null)
        {
            simple.Init(this);
            return;
        }

        RangedSkyEnemy ranged = enemy.GetComponentInChildren<RangedSkyEnemy>();
        if (ranged != null)
        {
            ranged.Init(this);
            return;
        }

        Debug.LogWarning("El enemigo spawneado no tiene SimpleEnemy ni RangedSkyEnemy.");
    }

    public void SetCurrentZone(GameZone zone)
    {
        currentZone = zone;
        //Debug.Log("Jugador en zona: " + currentZone);
    }

    public void NotifyEnemyDied()
    {
        aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
        enemiesKilledThisWave++;

        UpdateWaveUI();
    }

    private void UpdateWaveUI()
    {
        int remaining = Mathf.Max(0, enemiesToSpawnThisWave - enemiesKilledThisWave);

        if (waveText != null)
            waveText.text = "Round: " + currentWave;

        if (remainingEnemiesText != null)
            remainingEnemiesText.text = "Enemies left: " + remaining;
    }

    public int GetCurrentWave()
    {
        return currentWave;
    }

    public int GetAliveEnemies()
    {
        return aliveEnemies;
    }

    public int GetRemainingToSpawn()
    {
        return Mathf.Max(0, enemiesToSpawnThisWave - enemiesSpawnedThisWave);
    }

    public int GetRemainingEnemiesInWave()
    {
        return Mathf.Max(0, enemiesToSpawnThisWave - enemiesKilledThisWave);
    }
}