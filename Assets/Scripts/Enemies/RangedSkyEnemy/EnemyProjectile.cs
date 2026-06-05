using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 12f;
    [SerializeField] private float lifeTime = 4f;

    [Header("Player Hit FX")]
    [SerializeField] private GameObject playerHitFX;
    [SerializeField] private float playerHitFXLifetime = 2f;
    [SerializeField] private Vector3 playerHitFXOffset = Vector3.up * 1f;

    [Header("Player Hit Audio")]
    [SerializeField] private AudioClip hitPlayerClip;
    [SerializeField] private float hitPlayerVolume = 1f;
    [SerializeField] private float minPitch = 0.95f;
    [SerializeField] private float maxPitch = 1.05f;

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
        PlayerHealth health = other.GetComponentInParent<PlayerHealth>();

        if (health != null && health.IsAlive)
        {
            health.TakeDamage(damage);

            PlayHitPlayerSound(health.transform.position);

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

    private void PlayHitPlayerSound(Vector3 position)
    {
        if (hitPlayerClip == null)
            return;

        GameObject audioObject = new GameObject("Enemy Projectile Hit Audio");
        audioObject.transform.position = position;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = hitPlayerClip;
        source.volume = hitPlayerVolume;
        source.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        source.spatialBlend = 1f;

        source.Play();

        Destroy(audioObject, hitPlayerClip.length + 0.2f);
    }
}