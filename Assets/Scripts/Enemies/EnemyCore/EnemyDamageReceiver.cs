using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
public class EnemyDamageReceiver : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float maxHealth = 5f;
    private float currentHealth;

    [Header("Soul Rewards")]
    [SerializeField] private int soulsPerHit = 10;
    [SerializeField] private int soulsOnDeath = 50;

    [Header("Knockback")]
    [SerializeField] private bool enableKnockback = true;
    [SerializeField] private float knockbackDistance = 0.6f;
    [SerializeField] private float knockbackTime = 0.12f;
    [SerializeField] private float stunTime = 0.2f;
    [SerializeField]private float knockbackMultiplier = 1f;
    private float knockbackTimeMultiplier = 1f;

    [Header("Hit Flash")]
    [SerializeField] private bool enableHitFlash = true;
    [SerializeField] private Material flashMaterial;
    [SerializeField] private float flashDuration = 0.12f;
    [SerializeField] private float flashInterval = 0.04f;

    [Header("Death FX")]
    [SerializeField] private GameObject deathFX;
    [SerializeField] private Transform deathFXPoint;
    [SerializeField] private float destroyDelay = 0.05f;

    [Header("Hit FX")]
    [SerializeField] private GameObject hitFX;
    [SerializeField] private Transform hitFXPoint;
    [SerializeField] private float hitFXLifetime = 2f;

    public event Action OnDeath;
    public event Action OnHitStart;
    public event Action OnHitEnd;

    public bool IsDead { get; private set; }
    public bool IsStunned { get; private set; }

    private Rigidbody rb;
    private NavMeshAgent agent;

    private Coroutine hitRoutine;
    private Coroutine flashRoutine;

    private Renderer[] renderers;
    private Material[][] originalMaterials;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        agent = GetComponent<NavMeshAgent>();

        currentHealth = maxHealth;

        renderers = GetComponentsInChildren<Renderer>(true);
        originalMaterials = new Material[renderers.Length][];

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                originalMaterials[i] = renderers[i].materials;
        }
    }

    public void ApplyHit(int damage, Vector3 hitDirection)
    {
        if (IsDead) return;

        currentHealth -= damage;
        SpawnHitFX(hitDirection);

        if (SoulManager.Instance != null)
            SoulManager.Instance.AddSouls(soulsPerHit);

        if (currentHealth <= 0f)
        {
            Die();
            return;
        }

        if (enableHitFlash && flashMaterial != null)
        {
            if (flashRoutine != null)
                StopCoroutine(flashRoutine);

            flashRoutine = StartCoroutine(HitFlashRoutine());
        }

        if (enableKnockback)
        {
            if (hitRoutine != null)
                StopCoroutine(hitRoutine);

            hitRoutine = StartCoroutine(HitReaction(hitDirection));
        }
    }

    private void SpawnHitFX(Vector3 hitDirection)
    {
        if (hitFX == null) return;

        Vector3 spawnPos = hitFXPoint != null
            ? hitFXPoint.position
            : transform.position + Vector3.up * 1f;

        Quaternion rotation = Quaternion.LookRotation(-hitDirection.normalized);

        GameObject fx = Instantiate(hitFX, spawnPos, rotation);

        Destroy(fx, hitFXLifetime);
    }

    private IEnumerator HitFlashRoutine()
    {
        float elapsed = 0f;
        bool flashOn = true;

        while (elapsed < flashDuration)
        {
            if (flashOn)
                SetFlashMaterial();
            else
                RestoreOriginalMaterials();

            flashOn = !flashOn;

            yield return new WaitForSeconds(flashInterval);
            elapsed += flashInterval;
        }

        RestoreOriginalMaterials();
        flashRoutine = null;
    }

    private void SetFlashMaterial()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null) continue;

            Material[] mats = r.materials;

            for (int j = 0; j < mats.Length; j++)
                mats[j] = flashMaterial;

            r.materials = mats;
        }
    }

    private void RestoreOriginalMaterials()
    {
        for (int i = 0; i < renderers.Length; i++)
        {
            Renderer r = renderers[i];
            if (r == null || originalMaterials[i] == null) continue;

            r.materials = originalMaterials[i];
        }
    }

    public void SetKnockbackStats(float distanceMultiplier, float timeMultiplier)
    {
        knockbackMultiplier = distanceMultiplier;
        knockbackTimeMultiplier = timeMultiplier;

    }

    private IEnumerator HitReaction(Vector3 hitDirection)
    {
        IsStunned = true;
        OnHitStart?.Invoke();

        if (agent != null && agent.enabled)
            agent.enabled = false;

        Vector3 away = hitDirection;
        away.y = 0f;

        if (away.sqrMagnitude < 0.001f)
            away = -transform.forward;

        away.Normalize();

        Vector3 start = transform.position;
        Vector3 target = start + away * knockbackDistance * knockbackMultiplier;

        float elapsed = 0f;

        float finalKnockbackTime = knockbackTime * knockbackTimeMultiplier;

        while (elapsed < finalKnockbackTime)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / finalKnockbackTime);
            float eased = 1f - Mathf.Pow(1f - t, 3f);

            rb.MovePosition(Vector3.Lerp(start, target, eased));

            yield return null;
        }

        if (stunTime > 0f)
            yield return new WaitForSeconds(stunTime);

        if (agent != null && !agent.enabled)
            agent.enabled = true;

        IsStunned = false;
        OnHitEnd?.Invoke();

        hitRoutine = null;
    }

    private void Die()
    {
        if (IsDead) return;

        IsDead = true;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        RestoreOriginalMaterials();

        if (SoulManager.Instance != null)
            SoulManager.Instance.AddSouls(soulsOnDeath);

        OnDeath?.Invoke();

        // Telemetry
        GameTelemetryEvents.EnemyKilled();

        if (deathFX != null)
        {
            Vector3 spawnPos = deathFXPoint != null ? deathFXPoint.position : transform.position;
            Quaternion spawnRot = deathFXPoint != null ? deathFXPoint.rotation : Quaternion.identity;

            GameObject fx = Instantiate(deathFX, spawnPos, spawnRot);
            Destroy(fx, 3f);
        }

        Destroy(transform.root.gameObject, destroyDelay);
    }
}