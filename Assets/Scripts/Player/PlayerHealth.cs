using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 10;
    [SerializeField] private int currentLives = 10;
    [SerializeField] private float healDelay = 3f;

    public bool IsDeadOrReviving => isDeadOrReviving;
    public bool IsAlive => !isDeadOrReviving && currentLives > 0;

    [Header("Disable On Death")]
    [SerializeField] private MonoBehaviour[] disableWhileDeadOrReviving;

    [Header("Death")]
    [SerializeField] private GameObject deathText;

    [Header("Juggernout")]
    [SerializeField] private float juggerMultiplier = 2f;

    [Header("Quick Revive")]
    [SerializeField] private float quickReviveDelay = 5f;

    [Header("HUD")]
    [SerializeField] private PerkHUD perkHUD;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private PlayerDamageFeedback damageFeedback;
    private Coroutine healRoutine;
    private Coroutine quickReviveRoutine;

    private bool hasJugger;
    private bool hasQuickRevive;
    private bool isDeadOrReviving;

    private Rigidbody rb;
    private CharacterController characterController;

    private void Awake()
    {
        currentLives = maxLives;

        damageFeedback = GetComponent<PlayerDamageFeedback>();
        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();

        if (damageFeedback != null)
            damageFeedback.UpdateBlood(currentLives, maxLives);
    }

    public void TakeDamage(int dmg)
    {
        if (isDeadOrReviving) return;
        if (currentLives <= 0) return;

        currentLives -= dmg;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);

        // Telemetry
        GameTelemetryEvents.PlayerDamaged(dmg, currentLives);

        if (damageFeedback != null)
        {
            damageFeedback.PlayDamageFeedback();
            damageFeedback.UpdateBlood(currentLives, maxLives);
        }

        if (healRoutine != null)
            StopCoroutine(healRoutine);

        if (currentLives > 0)
            healRoutine = StartCoroutine(HealAfterDelay());
        else
            HandleDeath();
    }

    private IEnumerator HealAfterDelay()
    {
        yield return new WaitForSeconds(healDelay);

        currentLives = maxLives;

        if (damageFeedback != null)
            damageFeedback.ClearBloodSmooth();

        // Telemetry
        GameTelemetryEvents.PlayerHealed(maxLives, currentLives);

        if (showDebug)
            Debug.Log("Player fully healed");
    }

    private void HandleDeath()
    {
        if (hasQuickRevive)
        {
            quickReviveRoutine = StartCoroutine(QuickReviveRoutine());
            return;
        }

        Die();
    }

    private IEnumerator QuickReviveRoutine()
    {
        SetPlayerControlEnabled(false);

        hasQuickRevive = false;

        if (perkHUD != null)
            perkHUD.DeactivatePerkIconByType(PerkType.QuickRev);

        isDeadOrReviving = true;

        if (healRoutine != null)
            StopCoroutine(healRoutine);

        if (damageFeedback != null)
            damageFeedback.SetFullBlood();

        if (showDebug)
            Debug.Log("Quick Revive activado. Reanimando en 5 segundos...");

        yield return new WaitForSeconds(quickReviveDelay);

        currentLives = maxLives;

        isDeadOrReviving = false;
        quickReviveRoutine = null;

        if (damageFeedback != null)
        {
            damageFeedback.UpdateBlood(currentLives, maxLives);
            damageFeedback.ClearDeathOverlaySmooth();
        }

        SetPlayerControlEnabled(true);

        if (showDebug)
            Debug.Log("Quick Revive usado. Revives con " + currentLives + " vidas.");

        // Telemetry
        GameTelemetryEvents.PlayerRevived();
    }

    private void Die()
    {
        isDeadOrReviving = true;
        SetPlayerControlEnabled(false);

        if (damageFeedback != null)
            damageFeedback.SetFullBlood();

        if (deathText != null)
            deathText.SetActive(true);

        // Telemetry
        GameTelemetryEvents.PlayerDied();

        Time.timeScale = 0f;

        if (showDebug)
            Debug.Log("Player died");
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        for (int i = 0; i < disableWhileDeadOrReviving.Length; i++)
        {
            if (disableWhileDeadOrReviving[i] != null)
                disableWhileDeadOrReviving[i].enabled = enabled;
        }

        if (!enabled)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;

                rb.angularVelocity = Vector3.zero;
            }

            if (characterController != null)
                characterController.Move(Vector3.zero);
        }
    }

    public void ApplyJugger()
    {
        if (hasJugger) return;

        hasJugger = true;

        maxLives = Mathf.CeilToInt(maxLives * juggerMultiplier);
        currentLives = maxLives;

        if (damageFeedback != null)
            damageFeedback.UpdateBlood(currentLives, maxLives);

        if (showDebug)
            Debug.Log("Juggernout aplicado. MaxLives: " + maxLives);
    }

    public void ApplyQuickRevive()
    {
        if (hasQuickRevive) return;

        hasQuickRevive = true;

        if (showDebug)
            Debug.Log("Quick Revive aplicado.");
    }
}