using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private WeaponData currentMeleeWeapon;

    [Header("CameraShake")]
    [SerializeField] private CinemachineCameraShake cameraShake;


    private float nextAttackTime = 0f;
    private Animator animator;
    private bool nextSwingRight = true;

    private PlayerCombat combat;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();
    }

    public void TryMeleeAttack(WeaponData weapon)
    {
        if (weapon == null) return;
        if (Time.time < nextAttackTime) return;

        currentMeleeWeapon = weapon;

        nextAttackTime = Time.time + combat.GetFinalMeleeCooldown(weapon);

        if (nextSwingRight)
        {
            animator.ResetTrigger("Attack2");
            animator.SetTrigger("Attack1");
        }
        else
        {
            animator.ResetTrigger("Attack1");
            animator.SetTrigger("Attack2");
        }
        nextSwingRight = !nextSwingRight;
    }

    // Animation Event
    public void DealMeleeDamage()
    {
        if (currentMeleeWeapon == null) return;

        combat?.PlayWeaponVFX(currentMeleeWeapon);
        cameraShake?.Shake(0.06f, 1f, 15f);

        Vector3 center = transform.position + transform.forward * currentMeleeWeapon.forwardOffset;

        Collider[] hits = Physics.OverlapSphere(
            center,
            currentMeleeWeapon.attackRange,
            currentMeleeWeapon.enemyLayer
        );

        foreach (var hit in hits)
        {
            EnemyDamageReceiver enemy = hit.GetComponentInParent<EnemyDamageReceiver>();

            if (enemy != null)
            {
                Vector3 hitDir = enemy.transform.position - transform.position;
                hitDir.y = 0f;

                if (combat != null)
                {
                    int finalDamage = combat.GetFinalDamage(currentMeleeWeapon.damage);

                    enemy.SetKnockbackStats(
                        combat.KnockbackMultiplier,
                        combat.KnockbackTimeMultiplier
                    );

                    enemy.ApplyHit(finalDamage, hitDir);
                }
                else
                {
                    Debug.LogWarning("PlayerCombat no encontrado");

                    enemy.ApplyHit(currentMeleeWeapon.damage, hitDir);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (currentMeleeWeapon == null) return;

        Gizmos.color = Color.red;

        Vector3 center = transform.position + transform.forward * currentMeleeWeapon.forwardOffset;

        Gizmos.DrawWireSphere(center, currentMeleeWeapon.attackRange);
    }
}