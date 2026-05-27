using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("Bullet Settings")]
    [SerializeField] private float defaultSpeed = 15f;
    [SerializeField] private float lifetime = 3f;
    [SerializeField] private int damage = 1;
    [SerializeField] private float radius = 0.12f;
    [SerializeField] private LayerMask hitMask = ~0;

    private WeaponData sourceWeapon;

    private float currentSpeed;
    private Vector3 lastPosition;

    private float knockbackMultiplier = 1f;
    private float knockbackTimeMultiplier = 1f;

    private void Start()
    {
        currentSpeed = defaultSpeed;
        lastPosition = transform.position;

        Destroy(gameObject, lifetime);
    }

    public void SetSpeed(float newSpeed)
    {
        currentSpeed = newSpeed;
    }

    private void Update()
    {
        Vector3 currentPosition = transform.position;
        Vector3 direction = transform.forward;
        float distance = currentSpeed * Time.deltaTime;

        if (Physics.SphereCast(currentPosition, radius, direction, out RaycastHit hit, distance, hitMask, QueryTriggerInteraction.Ignore))
        {
            HandleHit(hit.collider);
            transform.position = hit.point;
            return;
        }

        transform.position = currentPosition + direction * distance;
        lastPosition = transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleHit(other);
    }

    public void SetStats(float newSpeed, int newDamage, float newKnockbackMultiplier, float newKnockbackTimeMultiplier, WeaponData weapon)
    {
        currentSpeed = newSpeed;
        damage = newDamage;
        knockbackMultiplier = newKnockbackMultiplier;
        knockbackTimeMultiplier = newKnockbackTimeMultiplier;
        sourceWeapon = weapon;
    }

    private void HandleHit(Collider other)
    {
        if (other == null) return;
        if (other.isTrigger) return;
        if (other.CompareTag("Player")) return;

        EnemyDamageReceiver enemy = other.GetComponentInParent<EnemyDamageReceiver>();

        if (enemy != null)
        {
            enemy.SetKnockbackStats(knockbackMultiplier, knockbackTimeMultiplier);
            enemy.ApplyHit(damage, transform.forward);

            //Telemetry
            GameTelemetryEvents.ShotHit(sourceWeapon.weaponName, damage);
            Destroy(gameObject);
            return;
        }

        Destroy(gameObject);
    }
}