using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Lives")]
    [SerializeField] private int maxLives = 10;
    [SerializeField] private int currentLives = 10;
    [SerializeField] private float healDelay = 3f;

    public bool IsDeadOrReviving => isDeadOrReviving;
    public int CurrentLives => currentLives;
    public float HealDelay => healDelay;
    public bool IsAlive => !isDeadOrReviving && currentLives > 0;

    [Header("Disable On Death")]
    [SerializeField] private MonoBehaviour[] disableWhileDeadOrReviving;

    [Header("Death")]
    [SerializeField] private GameObject deathText;
    [SerializeField] private Animator animator;
    [SerializeField] private string deathTrigger = "Death";

    [Header("Juggernout")]
    [SerializeField] private float juggerMultiplier = 2f;

    [Header("Quick Revive")]
    [SerializeField] private float quickReviveDelay = 5f;

    [Header("HUD")]
    [SerializeField] private PerkHUD perkHUD;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private PlayerDamageFeedback damageFeedback;
    private PlayerAudio playerAudio;
    private PlayerInputReader inputReader;
    private PlayerMovement playerMovement;

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
        playerAudio = GetComponent<PlayerAudio>();
        inputReader = GetComponent<PlayerInputReader>();
        playerMovement = GetComponent<PlayerMovement>();

        rb = GetComponent<Rigidbody>();
        characterController = GetComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (damageFeedback != null)
            damageFeedback.UpdateBlood(currentLives, maxLives);
    }

    public void TakeDamage(int dmg)
    {
        if (isDeadOrReviving) return;
        if (currentLives <= 0) return;

        currentLives -= dmg;
        currentLives = Mathf.Clamp(currentLives, 0, maxLives);

        

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
        isDeadOrReviving = true;

        // Corta input, dash y movimiento residual antes de desactivar scripts
        StopPlayerCompletely();

        SetPlayerControlEnabled(false);

        hasQuickRevive = false;

        if (perkHUD != null)
            perkHUD.DeactivatePerkIconByType(PerkType.QuickRev);

        if (healRoutine != null)
            StopCoroutine(healRoutine);

        if (damageFeedback != null)
            damageFeedback.SetFullBlood();

        // Solo reproduce animación de muerte
        PlayDeathAnimation();

        if (showDebug)
            Debug.Log("Quick Revive activado. Reanimando en " + quickReviveDelay + " segundos...");

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

        GameTelemetryEvents.PlayerRevived();
    }

    private void Die()
    {
        isDeadOrReviving = true;

        StopPlayerCompletely();
        SetPlayerControlEnabled(false);

        if (playerAudio != null)
            playerAudio.PlayDeath();

        if (damageFeedback != null)
            damageFeedback.SetFullBlood();

        if (deathText != null)
            deathText.SetActive(true);

        PlayDeathAnimation();

        GameTelemetryEvents.PlayerDied();

        

        if (showDebug)
            Debug.Log("Player died");

    }

    private void StopPlayerCompletely()
    {
        if (inputReader != null)
            inputReader.ClearInput();

        if (playerMovement != null)
            playerMovement.ForceStopMovement();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        if (characterController != null)
            characterController.Move(Vector3.zero);
    }

    private void PlayDeathAnimation()
    {
        if (animator == null) return;

        animator.ResetTrigger(deathTrigger);
        animator.SetTrigger(deathTrigger);
    }

    private void SetPlayerControlEnabled(bool enabled)
    {
        for (int i = 0; i < disableWhileDeadOrReviving.Length; i++)
        {
            if (disableWhileDeadOrReviving[i] != null)
                disableWhileDeadOrReviving[i].enabled = enabled;
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