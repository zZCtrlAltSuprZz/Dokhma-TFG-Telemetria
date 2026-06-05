using System.Collections.Generic;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(SphereCollider))]
public class RitualAltar : MonoBehaviour
{
    private enum RitualState
    {
        Idle,
        Active,
        Completed,
        RewardClaimed
    }

    [Header("Ritual")]
    [SerializeField] private int killsRequired = 5;
    [SerializeField] private float ritualRadius = 6f;
    [SerializeField] private float resetTimeWithoutKill = 60f;
    [SerializeField] private bool showDebug = true;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip ritualSound;

    [Header("Cost")]
    [SerializeField] private int ritualCost = 500;

    [Header("Animator")]
    [SerializeField] private Animator altarAnimator;

    [Header("Perk Reward")]
    [SerializeField] private PerkSelectionUI perkSelectionUI;
    [SerializeField] private PerkData optionA;
    [SerializeField] private PerkData optionB;

    [Header("Interaction UI")]
    [SerializeField] private GameObject interactUI;
    [SerializeField] private TMP_Text interactText;
    [SerializeField] private string startMessage = "Press E to start ritual";
    [SerializeField] private string claimRewardMessage = "Press E to claim your reward";
    [SerializeField] private string notEnoughSoulsMessage = "Not enough souls";

    [Header("Completion FX")]
    [SerializeField] private GameObject ritualCompleteFX;
    [SerializeField] private Transform ritualFXPoint;
    [SerializeField] private float ritualFXLifetime = 5f;

    private SphereCollider ritualCollider;
    private readonly HashSet<IRitualEnemy> enemiesInside = new HashSet<IRitualEnemy>();

    private PlayerInputReader playerInput;
    private bool playerInside;

    private RitualState state = RitualState.Idle;
    private int currentKills;
    private float timeSinceLastKill;

    public int CurrentKills => currentKills;
    public int KillsRequired => killsRequired;
    public bool RitualActive => state == RitualState.Active;
    public bool RitualCompleted => state == RitualState.Completed || state == RitualState.RewardClaimed;

    private void Awake()
    {
        ritualCollider = GetComponent<SphereCollider>();
        ritualCollider.isTrigger = true;
        ritualCollider.radius = ritualRadius;

        if (altarAnimator == null)
            altarAnimator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        SetState(RitualState.Idle);
    }

    private void Update()
    {
        if (state != RitualState.Active) return;

        timeSinceLastKill += Time.deltaTime;

        if (timeSinceLastKill >= resetTimeWithoutKill)
            SetState(RitualState.Idle);
    }

    private void OnValidate()
    {
        SphereCollider col = GetComponent<SphereCollider>();

        if (col != null)
        {
            col.isTrigger = true;
            col.radius = ritualRadius;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = true;

            playerInput = other.GetComponent<PlayerInputReader>();
            if (playerInput == null)
                playerInput = other.GetComponentInParent<PlayerInputReader>();

            if (playerInput != null)
                playerInput.OnInteractPressed += Interact;

            UpdateInteractUI();
            return;
        }

        IRitualEnemy enemy = other.GetComponentInParent<IRitualEnemy>();
        if (enemy == null) return;

        if (enemiesInside.Add(enemy))
        {
            enemy.OnRitualEnemyDied += HandleEnemyDied;    
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;

            if (playerInput != null)
                playerInput.OnInteractPressed -= Interact;

            playerInput = null;

            UpdateInteractUI();
            return;
        }

        IRitualEnemy enemy = other.GetComponentInParent<IRitualEnemy>();
        if (enemy == null) return;

        RemoveEnemy(enemy);
    }

    private void Interact()
    {
        if (!playerInside) return;

        switch (state)
        {
            case RitualState.Idle:
                TryStartRitual();
                break;

            case RitualState.Completed:
                ClaimReward();
                break;
        }
    }

    private void TryStartRitual()
    {
        if (SoulManager.Instance == null) return;

        if (!SoulManager.Instance.TrySpendSouls(ritualCost))
        {
            if (interactText != null)
                interactText.text = notEnoughSoulsMessage + "(" + ritualCost+")";

            if (showDebug)
                Debug.Log("No tienes suficientes almas para iniciar el ritual");

            return;
        }

        SetState(RitualState.Active);
    }

    private void HandleEnemyDied(IRitualEnemy enemy)
    {
        if (state != RitualState.Active) return;
        if (!enemiesInside.Contains(enemy)) return;

        currentKills++;
        timeSinceLastKill = 0f;

        RemoveEnemy(enemy);

        if (showDebug)
            Debug.Log("Ritual progress: " + currentKills + "/" + killsRequired);

        if (currentKills >= killsRequired)
            SetState(RitualState.Completed);
    }

    private void ClaimReward()
    {
        SetState(RitualState.RewardClaimed);

        if (perkSelectionUI != null)
            perkSelectionUI.OpenSelection(optionA, optionB);
        else
            Debug.LogError("PerkSelectionUI no asignado en RitualAltar");
    }

    private void SetState(RitualState newState)
    {
        state = newState;

        switch (state)
        {
            case RitualState.Idle:
                EnterIdleState();
                break;

            case RitualState.Active:
                EnterActiveState();
                break;

            case RitualState.Completed:
                EnterCompletedState();
                break;

            case RitualState.RewardClaimed:
                EnterRewardClaimedState();
                break;
        }

        UpdateInteractUI();
    }

    private void EnterIdleState()
    {
        currentKills = 0;
        timeSinceLastKill = 0f;
        UpdateAnimator();
    }

    private void EnterActiveState()
    {
        currentKills = 0;
        timeSinceLastKill = 0f;

        PlayRitualSound();
        UpdateAnimator();
    }

    private void EnterCompletedState()
    {
        timeSinceLastKill = 0f;

        UpdateAnimator();
        PlayRitualSound();


        SpawnCompletionFX();

        if (showDebug)
            Debug.Log("Ritual completado. Vuelve a interactuar para reclamar perk.");
    }

    private void EnterRewardClaimedState()
    {
        UpdateAnimator();
    }

    private void UpdateAnimator()
    {
        if (altarAnimator == null) return;

        bool isActive = state == RitualState.Active;
        altarAnimator.SetBool("isActive", isActive);
    }

    private void PlayRitualSound()
    {
        if (audioSource != null && ritualSound != null)
            audioSource.PlayOneShot(ritualSound);
    }
    private void UpdateInteractUI()
    {
        bool shouldShow =
            playerInside &&
            (state == RitualState.Idle || state == RitualState.Completed);

        if (interactUI != null)
            interactUI.SetActive(shouldShow);

        if (interactText != null)
            interactText.gameObject.SetActive(shouldShow);

        if (!shouldShow)
        {
            if (interactText != null)
                interactText.text = "";

            return;
        }

        if (interactText == null) return;

        switch (state)
        {
            case RitualState.Idle:
                if (SoulManager.Instance != null &&
                    SoulManager.Instance.currentSouls >= ritualCost)
                {
                    interactText.text = startMessage + "(" + ritualCost + ")";
                }
                /*else
                {
                    interactText.text = notEnoughSoulsMessage + " - Need " + ritualCost;
                }*/
                break;

            case RitualState.Completed:
                interactText.text = claimRewardMessage;
                break;
        }
    }

    private void RemoveEnemy(IRitualEnemy enemy)
    {
        if (enemy == null) return;

        if (enemiesInside.Contains(enemy))
        {
            enemy.OnRitualEnemyDied -= HandleEnemyDied;
            enemiesInside.Remove(enemy);
        }
    }

    public void ResetRitual()
    {
        foreach (IRitualEnemy enemy in enemiesInside)
        {
            if (enemy != null)
                enemy.OnRitualEnemyDied -= HandleEnemyDied;
        }

        enemiesInside.Clear();

        SetState(RitualState.Idle);
    }

    private void SpawnCompletionFX()
    {
        if (ritualCompleteFX == null) return;

        Vector3 spawnPos = ritualFXPoint != null
            ? ritualFXPoint.position
            : transform.position;

        Quaternion spawnRot = ritualFXPoint != null
            ? ritualFXPoint.rotation
            : Quaternion.identity;

        GameObject fx = Instantiate( ritualCompleteFX,spawnPos,spawnRot);

        Destroy(fx, ritualFXLifetime);
    }

    private void OnDisable()
    {
        if (playerInput != null)
            playerInput.OnInteractPressed -= Interact;

        foreach (IRitualEnemy enemy in enemiesInside)
        {
            if (enemy != null)
                enemy.OnRitualEnemyDied -= HandleEnemyDied;
        }

        enemiesInside.Clear();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = RitualActive ? Color.red : Color.magenta;
        Gizmos.DrawWireSphere(transform.position, ritualRadius);
    }
}