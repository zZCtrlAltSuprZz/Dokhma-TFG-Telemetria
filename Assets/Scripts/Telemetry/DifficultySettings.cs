[System.Serializable]

/// <summary>
/// Conjunto de modificadores utilizados por el sistema
/// de dificultad dinámica para ajustar el ritmo de juego.
/// </summary>
public class DifficultySettings
{
    public float maxAliveMultiplier = 1f;
    public float restTimeMultiplier = 1f;
    public float spawnIntervalMultiplier = 1f;
    public float enemyCountMultiplier =1f;
}