using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 4f;

    [SerializeField] private GameObject playerHitFX;
    [SerializeField] private float playerHitFXLifetime = 2f;
    [SerializeField] private Vector3 playerHitFXOffset = Vector3.up * 1f;

    private Vector3 direction;
    private int damage;

    public void Init(Vector3 dir, int dmg)
    {
        direction = dir.normalized;
        damage = dmg;

        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null && health.IsAlive)
        {
            health.TakeDamage(damage);

            if (playerHitFX != null)
            {
                GameObject fx = Instantiate(
                    playerHitFX,
                    health.transform.position + playerHitFXOffset,
                    Quaternion.identity
                );

                Destroy(fx, playerHitFXLifetime);
            }

            Destroy(gameObject);
        }
    }
}