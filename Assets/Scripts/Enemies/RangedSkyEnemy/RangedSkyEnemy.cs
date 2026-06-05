using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyDamageReceiver))]
[RequireComponent(typeof(EnemyLook))]
public class RangedSkyEnemy : MonoBehaviour, IRitualEnemy
{
    [Header("Spawn From Sky")]
    [SerializeField] private float fallHeight = 12f;
    [SerializeField] private float warningTime = 1.5f;
    [SerializeField] private float fallSpeed = 25f;
    [SerializeField] private GameObject impactWarningPrefab;
    [SerializeField] private float trackingWarningTime = 0.8f;

    [Header("Landing Damage")]
    [SerializeField] private float landingDamageRadius = 3f;
    [SerializeField] private int landingDamage = 1;
    [SerializeField] private float landingKnockbackForce = 3f;
    [SerializeField] private float landingKnockbackTime = 0.2f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float fleeDistance = 9f;
    [SerializeField] private float maxDistance = 15f;
    [SerializeField] private float strafeDistance = 6f;
    [SerializeField] private float repositionInterval = 0.4f;

    [Header("Attack")]
    [SerializeField] private float delayAfterLanding = 1f;
    [SerializeField] private float attackRange = 12f;
    [SerializeField] private float attackCooldown = 2.5f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform shootPoint;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [SerializeField] private AudioClip fallingSound;
    [SerializeField] private float fallingVolume = 1f;

    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootVolume = 1f;

    [SerializeField] private AudioClip attackHitPlayerClip;
    [SerializeField] private float attackHitPlayerVolume = 1f;

    [SerializeField] private float minAttackPitch = 0.95f;
    [SerializeField] private float maxAttackPitch = 1.05f;

    [Header("Line Of Sight")]
    [SerializeField] private LayerMask obstacleLayer;
    [SerializeField] private float lineOfSightHeight = 1.2f;
    [SerializeField] private float chaseWhenNoSightDistance = 8f;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string moveBool = "isMoving";
    [SerializeField] private string shootTrigger = "Shoot";
    [SerializeField] private string landTrigger = "Land";
    [SerializeField] private float landAnimTriggerDistance = 10f;

    [Header("FX")]
    [SerializeField] private GameObject landingExplosionFX;
    [SerializeField] private float landingExplosionFXLifetime = 3f;

    [SerializeField] private GameObject playerHitFX;
    [SerializeField] private float playerHitFXLifetime = 2f;
    [SerializeField] private Vector3 playerHitFXOffset = Vector3.up;

    public event Action<RangedSkyEnemy> OnEnemyDied;
    public event Action<IRitualEnemy> OnRitualEnemyDied;

    public Transform Transform => transform;

    private Transform player;
    private PlayerHealth playerHealth;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private EnemyDamageReceiver damageReceiver;
    private EnemyLook enemyLook;

    private WaveManager waveManager;

    private bool hasLanded;
    private bool isAttacking;
    private bool canAttack;
    private bool deathNotified;

    private float nextAttackTime;
    private float nextRepositionTime;
    private Collider enemyCollider;

    private GameObject warningInstance;

    private bool AgentReady => agent != null && agent.enabled && agent.isOnNavMesh;
    private bool PlayerAlive => playerHealth != null && playerHealth.IsAlive;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        damageReceiver = GetComponent<EnemyDamageReceiver>();
        enemyLook = GetComponent<EnemyLook>();
        enemyCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.isKinematic = true;

        agent.updateRotation = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = 0.2f;
        agent.enabled = false;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        damageReceiver.OnDeath += HandleDeath;
        damageReceiver.OnHitStart += HandleHitStart;
        damageReceiver.OnHitEnd += HandleHitEnd;
    }

    private void OnDestroy()
    {
        if (damageReceiver == null)
            return;

        damageReceiver.OnDeath -= HandleDeath;
        damageReceiver.OnHitStart -= HandleHitStart;
        damageReceiver.OnHitEnd -= HandleHitEnd;
    }

    private void Start()
    {
        GameObject p = GameObject.FindWithTag("Player");

        if (p != null)
        {
            player = p.transform;
            playerHealth = p.GetComponent<PlayerHealth>();
        }

        StartCoroutine(SpawnFromSkyRoutine());
    }

    public void Init(WaveManager manager)
    {
        waveManager = manager;
    }

    private void Update()
    {
        if (damageReceiver.IsDead || damageReceiver.IsStunned || player == null || !hasLanded)
            return;

        if (!PlayerAlive)
        {
            StopAgent();
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float distance = toPlayer.magnitude;

        enemyLook.FaceDirection(toPlayer);

        bool hasLineOfSight = HasLineOfSight();

        if (!isAttacking)
        {
            if (hasLineOfSight)
                HandleMovement(distance);
            else
                MoveToGoodShootingPosition();
        }

        if (hasLineOfSight)
            HandleAttack(distance);

        UpdateMovementAnimation();
    }

    private IEnumerator SpawnFromSkyRoutine()
    {
        Vector3 groundPosition = transform.position;

        if (enemyCollider != null)
            enemyCollider.enabled = false;

        if (impactWarningPrefab != null)
            warningInstance = Instantiate(impactWarningPrefab, groundPosition, Quaternion.identity);

        float timer = 0f;

        while (timer < trackingWarningTime)
        {
            timer += Time.deltaTime;

            if (player != null)
            {
                groundPosition = player.position;

                if (NavMesh.SamplePosition(groundPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                    groundPosition = hit.position;

                if (warningInstance != null)
                    warningInstance.transform.position = groundPosition;
            }

            yield return null;
        }

        transform.position = groundPosition + Vector3.up * fallHeight;

        yield return new WaitForSeconds(warningTime);

        PlaySound(fallingSound, fallingVolume);

        bool triggeredLandAnim = false;

        while (Vector3.Distance(transform.position, groundPosition) > 0.1f)
        {
            float distanceToGround = Vector3.Distance(transform.position, groundPosition);

            if (!triggeredLandAnim && distanceToGround < landAnimTriggerDistance)
            {
                triggeredLandAnim = true;

                if (animator != null && !string.IsNullOrEmpty(landTrigger))
                    animator.SetTrigger(landTrigger);
            }

            transform.position = Vector3.MoveTowards(
                transform.position,
                groundPosition,
                fallSpeed * Time.deltaTime
            );

            if (player != null)
                enemyLook.FaceTarget(player);

            yield return null;
        }

        transform.position = groundPosition;

        if (enemyCollider != null)
            enemyCollider.enabled = true;

        DoLandingAreaDamage();

        if (warningInstance != null)
            Destroy(warningInstance);

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit finalHit, 3f, NavMesh.AllAreas))
        {
            agent.enabled = true;
            agent.Warp(finalHit.position);
            agent.isStopped = false;
        }

        hasLanded = true;

        StartCoroutine(EnableCombatAfterDelay());
    }

    private IEnumerator EnableCombatAfterDelay()
    {
        canAttack = false;

        yield return new WaitForSeconds(delayAfterLanding);

        canAttack = true;
    }

    private void HandleMovement(float distance)
    {
        if (!AgentReady || Time.time < nextRepositionTime)
            return;

        nextRepositionTime = Time.time + repositionInterval;

        Vector3 targetPosition;

        if (distance < fleeDistance)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            targetPosition = transform.position + dir * strafeDistance;
        }
        else if (distance > maxDistance)
        {
            Vector3 dir = (player.position - transform.position).normalized;
            targetPosition = transform.position + dir * strafeDistance;
        }
        else
        {
            Vector3 side = UnityEngine.Random.value < 0.5f ? transform.right : -transform.right;
            targetPosition = transform.position + side * strafeDistance;
        }

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 6f, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
        }
    }

    private bool HasLineOfSight()
    {
        if (player == null)
            return false;

        Vector3 start = transform.position + Vector3.up * lineOfSightHeight;
        Vector3 end = player.position + Vector3.up * lineOfSightHeight;
        Vector3 dir = end - start;

        return !Physics.Raycast(start, dir.normalized, dir.magnitude, obstacleLayer);
    }

    private void MoveToGoodShootingPosition()
    {
        if (!AgentReady || player == null)
            return;

        if (Time.time < nextRepositionTime)
            return;

        nextRepositionTime = Time.time + repositionInterval;

        Vector3 bestPosition = transform.position;
        bool foundPosition = false;

        int pointsToCheck = 16;
        float desiredDistance = chaseWhenNoSightDistance;

        for (int i = 0; i < pointsToCheck; i++)
        {
            float angle = i * Mathf.PI * 2f / pointsToCheck;
            Vector3 dir = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 testPosition = player.position + dir * desiredDistance;

            if (!NavMesh.SamplePosition(testPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
                continue;

            if (!PositionHasLineOfSight(hit.position))
                continue;

            bestPosition = hit.position;
            foundPosition = true;
            break;
        }

        if (!foundPosition)
            bestPosition = player.position;

        agent.isStopped = false;
        agent.SetDestination(bestPosition);
    }

    private bool PositionHasLineOfSight(Vector3 position)
    {
        if (player == null)
            return false;

        Vector3 start = position + Vector3.up * lineOfSightHeight;
        Vector3 end = player.position + Vector3.up * lineOfSightHeight;
        Vector3 dir = end - start;

        return !Physics.Raycast(start, dir.normalized, dir.magnitude, obstacleLayer);
    }

    private void HandleAttack(float distance)
    {
        if (!CanShoot(distance))
            return;

        nextAttackTime = Time.time + attackCooldown;

        StartCoroutine(ShootRoutine());
    }

    private bool CanShoot(float distance)
    {
        return PlayerAlive
            && canAttack
            && !isAttacking
            && distance <= attackRange
            && Time.time >= nextAttackTime;
    }

    private IEnumerator ShootRoutine()
    {
        isAttacking = true;

        StopAgent();

        if (animator != null && !string.IsNullOrEmpty(shootTrigger))
            animator.SetTrigger(shootTrigger);

        yield return new WaitForSeconds(0.25f);

        Shoot();

        yield return new WaitForSeconds(0.3f);

        isAttacking = false;
        nextRepositionTime = 0f;

        if (AgentReady)
        {
            agent.isStopped = false;
            agent.ResetPath();
        }
    }

    private void Shoot()
    {
        if (projectilePrefab == null || shootPoint == null || player == null || !PlayerAlive)
            return;

        Vector3 targetPos = player.position;
        targetPos.y = shootPoint.position.y;

        Vector3 dir = (targetPos - shootPoint.position).normalized;

        GameObject projectile = Instantiate(
            projectilePrefab,
            shootPoint.position,
            Quaternion.LookRotation(dir)
        );

        PlaySound(shootSound, shootVolume);

        EnemyProjectile enemyProjectile = projectile.GetComponent<EnemyProjectile>();

        if (enemyProjectile != null)
            enemyProjectile.Init(dir, attackDamage);
    }

    private void DoLandingAreaDamage()
    {
        SpawnFX(landingExplosionFX, transform.position, landingExplosionFXLifetime);

        Collider[] hits = Physics.OverlapSphere(transform.position, landingDamageRadius, playerLayer);

        foreach (Collider hit in hits)
        {
            PlayerHealth health = hit.GetComponentInParent<PlayerHealth>();

            if (health == null || !health.IsAlive)
                continue;

            health.TakeDamage(landingDamage);
            PlayAttackHitPlayerSound();
            SpawnPlayerHitFX(health.transform);

            Vector3 knockDir = hit.transform.position - transform.position;
            knockDir.y = 0f;

            if (knockDir.sqrMagnitude < 0.001f)
                knockDir = transform.forward;

            StartCoroutine(DoPlayerKnockback(hit.transform, knockDir.normalized));
        }
    }

    private IEnumerator DoPlayerKnockback(Transform target, Vector3 direction)
    {
        CharacterController controller = target.GetComponentInParent<CharacterController>();

        if (controller == null)
            yield break;

        Vector3 velocity = direction * landingKnockbackForce;

        float timer = 0f;

        while (timer < landingKnockbackTime)
        {
            timer += Time.deltaTime;

            float force = 1f - (timer / landingKnockbackTime);

            controller.Move(velocity * force * Time.deltaTime);

            yield return null;
        }
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null || !AgentReady)
            return;

        bool isMoving = agent.velocity.magnitude > 0.1f && !agent.isStopped;

        animator.SetBool(moveBool, isMoving);
    }

    public void ApplyHit(int damage, Vector3 hitDirection)
    {
        damageReceiver.ApplyHit(damage, hitDirection);
    }

    private void HandleHitStart()
    {
        isAttacking = false;
        canAttack = false;

        StopAgent();
    }

    private void HandleHitEnd()
    {
        canAttack = true;
        nextRepositionTime = 0f;

        if (AgentReady)
            agent.isStopped = false;
    }

    private void HandleDeath()
    {
        if (!deathNotified && waveManager != null)
        {
            waveManager.NotifyEnemyDied();
            deathNotified = true;
        }

        OnEnemyDied?.Invoke(this);
        OnRitualEnemyDied?.Invoke(this);
    }

    private void SpawnFX(GameObject prefab, Vector3 pos, float lifetime)
    {
        if (prefab == null)
            return;

        GameObject fx = Instantiate(prefab, pos, Quaternion.identity);

        Destroy(fx, lifetime);
    }

    private void SpawnPlayerHitFX(Transform target)
    {
        if (playerHitFX == null || target == null)
            return;

        SpawnFX(playerHitFX, target.position + playerHitFXOffset, playerHitFXLifetime);
    }

    private void StopAgent()
    {
        if (AgentReady)
            agent.isStopped = true;
    }

    private void PlaySound(AudioClip clip, float volume)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.pitch = UnityEngine.Random.Range(minAttackPitch, maxAttackPitch);
        audioSource.PlayOneShot(clip, volume);
    }

    private void PlayAttackHitPlayerSound()
    {
        PlaySound(attackHitPlayerClip, attackHitPlayerVolume);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, landingDamageRadius);
    }
}