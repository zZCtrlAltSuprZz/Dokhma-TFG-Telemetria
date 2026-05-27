using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyDamageReceiver))]
[RequireComponent(typeof(EnemyLook))]
public class SimpleEnemy : MonoBehaviour, IRitualEnemy
{
    [Header("Chase")]
    public float speed = 3f;
    public float stopDistance = 2f;
    public float resumeDistance = 2.3f;

    [Header("Attack")]
    public float attackRange = 1.8f;
    public float attackCooldown = 1.2f;
    public int attackDamage = 1;

    public string attackTrigger1 = "Attack1";
    public string attackTrigger2 = "Attack2";

    [Header("Animator")]
    public Animator animator;

    [Header("Player Hit FX")]
    [SerializeField] private GameObject playerHitFX;
    [SerializeField] private float playerHitFXLifetime = 2f;
    [SerializeField] private Vector3 playerHitFXOffset = Vector3.up;

    private WaveManager waveManager;
    private bool deathNotified;

    public event Action<SimpleEnemy> OnEnemyDied;
    public event Action<IRitualEnemy> OnRitualEnemyDied;
    public Transform Transform => transform;

    private Transform player;
    private PlayerHealth playerHealth;

    private Rigidbody rb;
    private NavMeshAgent agent;
    private EnemyDamageReceiver damageReceiver;
    private EnemyLook enemyLook;

    private bool isChasing = true;
    private bool isAttacking;

    private float nextAttackTime;
    private Coroutine attackRoutine;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();
        damageReceiver = GetComponent<EnemyDamageReceiver>();
        enemyLook = GetComponent<EnemyLook>();

        rb.useGravity = false;
        rb.isKinematic = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        agent.updateRotation = false;
        agent.updateUpAxis = true;
        agent.speed = speed;
        agent.acceleration = 30f;
        agent.angularSpeed = 720f;
        agent.autoBraking = false;
        agent.stoppingDistance = stopDistance;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        agent.avoidancePriority = UnityEngine.Random.Range(20, 80);

        damageReceiver.OnDeath += HandleDeath;
        damageReceiver.OnHitStart += HandleHitStart;
        damageReceiver.OnHitEnd += HandleHitEnd;
    }

    private void OnDestroy()
    {
        if (damageReceiver == null) return;

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

        if (resumeDistance <= stopDistance)
            resumeDistance = stopDistance + 0.3f;

        if (attackRange <= 0f)
            attackRange = stopDistance * 0.9f;
    }

    public void Init(WaveManager manager)
    {
        waveManager = manager;
    }

    private void Update()
    {
        if (player == null) return;
        if (damageReceiver.IsDead) return;
        if (damageReceiver.IsStunned) return;

        if (playerHealth != null && !playerHealth.IsAlive)
        {
            StopAgent();
            return;
        }

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;

        float dist = toPlayer.magnitude;

        if (isAttacking)
        {
            StopAgent();
            enemyLook.FaceDirection(toPlayer);
            return;
        }

        if (dist <= attackRange)
        {
            TryAttack(toPlayer);
            return;
        }

        if (isChasing && dist <= stopDistance)
            isChasing = false;
        else if (!isChasing && dist >= resumeDistance)
            isChasing = true;

        if (isChasing)
            ChasePlayer();
        else
        {
            StopAgent();
            FacePlayer(toPlayer);
        }
    }

    private void ChasePlayer()
    {
        if (!agent.enabled || !agent.isOnNavMesh || player == null) return;

        agent.isStopped = false;
        agent.speed = speed;
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(player.position);

        if (agent.desiredVelocity.sqrMagnitude > enemyLook.minTurnSqr)
            enemyLook.FaceDirection(agent.desiredVelocity);
    }

    private void TryAttack(Vector3 toPlayer)
    {
        if (playerHealth != null && !playerHealth.IsAlive) return;

        FacePlayer(toPlayer);

        if (Time.time < nextAttackTime) return;

        nextAttackTime = Time.time + attackCooldown;

        StopAgent();

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = StartCoroutine(AttackRoutine());
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;

        if (animator != null)
        {
            if (!string.IsNullOrEmpty(attackTrigger1))
                animator.ResetTrigger(attackTrigger1);

            if (!string.IsNullOrEmpty(attackTrigger2))
                animator.ResetTrigger(attackTrigger2);

            bool useFirstAttack = UnityEngine.Random.value < 0.5f;
            string selectedTrigger = useFirstAttack ? attackTrigger1 : attackTrigger2;

            if (!string.IsNullOrEmpty(selectedTrigger))
                animator.SetTrigger(selectedTrigger);
        }

        yield return new WaitForSeconds(0.1f);

        isAttacking = false;
        attackRoutine = null;
    }

    public void DealDamage()
    {
        if (player == null) return;
        if (playerHealth != null && !playerHealth.IsAlive) return;

        Vector3 flat = player.position - transform.position;
        flat.y = 0f;

        if (flat.magnitude <= attackRange + 0.4f)
        {
            PlayerHealth health = player.GetComponent<PlayerHealth>();

            if (health != null)
            {
                health.TakeDamage(attackDamage);

                SpawnPlayerHitFX(health.transform);
            }
        }
    }

    public void ApplyHit(int damage, Vector3 hitDirection)
    {
        damageReceiver.ApplyHit(damage, hitDirection);
    }

    private void HandleHitStart()
    {
        StopAgent();

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        attackRoutine = null;
        isAttacking = false;
        nextAttackTime = Time.time + 0.05f;
    }

    private void SpawnPlayerHitFX(Transform target)
    {
        if (playerHitFX == null || target == null) return;

        Vector3 spawnPos = target.position + playerHitFXOffset;

        GameObject fx = Instantiate(
            playerHitFX,
            spawnPos,
            Quaternion.identity
        );

        Destroy(fx, playerHitFXLifetime);
    }
    private void HandleHitEnd()
    {
        if (agent.enabled && agent.isOnNavMesh && player != null)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);
        }
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

        if (attackRoutine != null)
            StopCoroutine(attackRoutine);

        isAttacking = false;

        if (agent != null && agent.enabled)
            agent.enabled = false;

        Collider col = GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }


    private void FacePlayer(Vector3 toPlayer)
    {
        toPlayer.y = 0f;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        enemyLook.FaceDirection(toPlayer);
    }

    private void StopAgent()
    {
        if (agent.enabled && agent.isOnNavMesh)
            agent.isStopped = true;
    }
}