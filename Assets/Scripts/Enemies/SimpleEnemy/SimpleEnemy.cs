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
    public float stopDistance = 1.4f;
    public float resumeDistance = 1.8f;

    [SerializeField] private float repathRate = 0.2f;
    private float nextRepathTime;

    [Header("Natural Chase")]
    [SerializeField] private float surroundRadius = 2.5f;
    [SerializeField] private float surroundOffsetChangeRate = 2.5f;

    private Vector3 personalChaseOffset;
    private float nextOffsetChangeTime;

    [Header("Attack")]
    public float attackRange = 2f;
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

    [Header("Attack Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip attackHitPlayerClip;
    [SerializeField] private float attackHitPlayerVolume = 1f;
    [SerializeField] private float minAttackPitch = 0.95f;
    [SerializeField] private float maxAttackPitch = 1.05f;

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
        rb.interpolation = RigidbodyInterpolation.None;
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        agent.updateRotation = false;
        agent.updateUpAxis = true;
        agent.speed = speed;
        agent.acceleration = 30f;
        agent.angularSpeed = 720f;
        agent.autoBraking = true;
        agent.stoppingDistance = stopDistance;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        agent.avoidancePriority = UnityEngine.Random.Range(20, 80);
        agent.autoRepath = true;

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

        GeneratePersonalChaseOffset();
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

        if (Time.time >= nextOffsetChangeTime)
        {
            GeneratePersonalChaseOffset();
        }

        if (Time.time >= nextRepathTime)
        {
            nextRepathTime = Time.time + repathRate;

            Vector3 targetPosition = player.position + personalChaseOffset;

            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
            else
            {
                agent.SetDestination(player.position);
            }
        }

        if (agent.desiredVelocity.sqrMagnitude > enemyLook.minTurnSqr)
            enemyLook.FaceDirection(agent.desiredVelocity);
    }

    private void GeneratePersonalChaseOffset()
    {
        Vector2 randomCircle = UnityEngine.Random.insideUnitCircle.normalized * surroundRadius;

        personalChaseOffset = new Vector3(
            randomCircle.x,
            0f,
            randomCircle.y
        );

        nextOffsetChangeTime = Time.time + UnityEngine.Random.Range(
            surroundOffsetChangeRate * 0.7f,
            surroundOffsetChangeRate * 1.3f
        );
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
                PlayAttackHitPlayerSound();
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

    private void PlayAttackHitPlayerSound()
    {
        if (audioSource == null || attackHitPlayerClip == null)
            return;

        audioSource.pitch = UnityEngine.Random.Range(minAttackPitch, maxAttackPitch);
        audioSource.PlayOneShot(attackHitPlayerClip, attackHitPlayerVolume);
    }
}