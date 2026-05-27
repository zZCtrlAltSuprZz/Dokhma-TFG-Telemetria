using UnityEngine;

public class EnemyLook : MonoBehaviour
{
    [SerializeField] private float turnSpeedDegrees = 720f;
    [SerializeField] public float minTurnSqr = 0.0004f;

    private Vector3 lastFacing = Vector3.forward;

    public Vector3 LastFacing => lastFacing;

    public void FaceDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < minTurnSqr)
            return;

        lastFacing = direction.normalized;

        Quaternion targetRotation = Quaternion.LookRotation(lastFacing, Vector3.up);

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            turnSpeedDegrees * Time.deltaTime
        );
    }

    public void FaceTarget(Transform target)
    {
        if (target == null) return;

        Vector3 direction = target.position - transform.position;
        FaceDirection(direction);
    }

    public void FacePosition(Vector3 position)
    {
        Vector3 direction = position - transform.position;
        FaceDirection(direction);
    }
}