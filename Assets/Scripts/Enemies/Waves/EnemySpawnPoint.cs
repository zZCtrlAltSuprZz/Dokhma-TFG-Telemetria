using UnityEngine;

public enum GameZone
{
    ZoneLobby,
    ZoneLobbyLocked,
    ZoneCentral,
    ZoneWest,
    ZoneEast
}



public class EnemySpawnPoint : MonoBehaviour
{
    [Header("Spawn Point")]
    public bool enabledForSpawning = true;

    [Header("Zone")]
    public GameZone zone;

    public Vector3 GetSpawnPosition()
    {
        return transform.position;
    }

    public float DistanceTo(Vector3 targetPos)
    {
        return Vector3.Distance(transform.position, targetPos);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = enabledForSpawning ? Color.green : Color.gray;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}