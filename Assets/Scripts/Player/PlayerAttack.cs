using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private WeaponData currentMeleeWeapon;

    [Header("CameraShake")]
    [SerializeField] private CinemachineCameraShake cameraShake;

    private PlayerAim aim;
    private PlayerCombat combat;
    private Animator animator;

    private float nextAttackTime = 0f;
    private bool nextSwingRight = true;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        combat = GetComponent<PlayerCombat>();
        aim = GetComponent<PlayerAim>();
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

    public void DealMeleeDamage()
    {
        if (currentMeleeWeapon == null) return;

        Vector3 attackDirection = GetAttackDirection();
        Vector3 center = GetMeleeCenter();
        Quaternion rotation = GetMeleeRotation();

        combat?.PlayWeaponVFX(currentMeleeWeapon, center, rotation);
        cameraShake?.Shake(0.06f, 1f, 15f);

        Collider[] hits = Physics.OverlapSphere(center, currentMeleeWeapon.attackRange, currentMeleeWeapon.enemyLayer);

        int enemiesHitThisAttack = 0;
        int totalDamageThisAttack = 0;

        foreach (var hit in hits)
        {
            EnemyDamageReceiver enemy = hit.GetComponentInParent<EnemyDamageReceiver>();

            if (enemy != null)
            {
                if (combat != null)
                {
                    int finalDamage = combat.GetFinalDamage(currentMeleeWeapon.damage);

                    enemy.SetKnockbackStats(combat.KnockbackMultiplier, combat.KnockbackTimeMultiplier);
                    enemy.ApplyHit(finalDamage, attackDirection);

                    enemiesHitThisAttack++;
                    totalDamageThisAttack += finalDamage;
                }
                else
                {
                    Debug.LogWarning("PlayerCombat no encontrado");
                }
            }
        }

        if (enemiesHitThisAttack > 0)
        {
            GameTelemetryEvents.MeleeSuccessfulAttack(currentMeleeWeapon.weaponName, enemiesHitThisAttack, totalDamageThisAttack);
        }
    }

    private Vector3 GetAttackDirection()
    {
        if (aim != null && aim.LastAimDirection.sqrMagnitude > 0.001f)
        {
            Vector3 direction = aim.LastAimDirection;
            direction.y = 0f;
            return direction.normalized;
        }

        return transform.forward;
    }

    private Vector3 GetMeleeCenter()
    {
        return transform.position + GetAttackDirection() * currentMeleeWeapon.forwardOffset;
    }

    private Quaternion GetMeleeRotation()
    {
        return Quaternion.LookRotation(GetAttackDirection(), Vector3.up);
    }

    private void OnDrawGizmosSelected()
    {
        if (currentMeleeWeapon == null) return;

        Gizmos.color = Color.red;

        Vector3 attackDirection = transform.forward;
        PlayerAim aimRef = GetComponent<PlayerAim>();

        if (aimRef != null && aimRef.LastAimDirection.sqrMagnitude > 0.001f)
        {
            attackDirection = aimRef.LastAimDirection;
            attackDirection.y = 0f;
            attackDirection.Normalize();
        }

        Vector3 center = transform.position + attackDirection * currentMeleeWeapon.forwardOffset;

        Gizmos.DrawWireSphere(center, currentMeleeWeapon.attackRange);
    }
}