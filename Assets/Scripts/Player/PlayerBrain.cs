using UnityEngine;

public class PlayerBrain : MonoBehaviour
{
    [SerializeField] private bool showDebug = true;

    [Header("Animator")]
    [SerializeField] private Animator animator;
    [SerializeField] private string isMovingBool = "isMoving";
    [SerializeField] private string isPistolBool = "isPistol";
    [SerializeField] private string isRunningBool = "isRunning";
    [SerializeField] private string isCombatBool = "isCombat";

    [SerializeField] private float combatExitDelay = 1f;
    private float lastCombatTime;
    private bool isInCombat;
    private bool wasRunning;
    bool aimingWithStick;
    private PlayerInputReader inputReader;
    private PlayerMovement movement;
    private PlayerAim aim;
    private PlayerCombat combat;
    private PlayerVisuals visuals;
    private PlayerHealth health;

    private void Awake()
    {
        inputReader = GetComponent<PlayerInputReader>();
        movement = GetComponent<PlayerMovement>();
        aim = GetComponent<PlayerAim>();
        combat = GetComponent<PlayerCombat>();
        visuals = GetComponent<PlayerVisuals>();
        health = GetComponent<PlayerHealth>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (inputReader != null)
        {
            inputReader.OnFirePressed += HandleFirePressed;
            inputReader.OnSwitchWeaponPressed += HandleSwitchWeaponPressed;
        }

        if (combat != null)
        {
            combat.OnWeaponChanged += HandleWeaponChanged;
        }

        inputReader.OnReloadPressed += HandleReloadPressed;
    }

    private void Start()
    {
        if (combat != null && visuals != null)
            visuals.UpdateWeaponVisuals(combat.CurrentWeapon);

        if (animator != null && combat != null)
            animator.SetBool(isPistolBool, combat.CurrentWeapon.usesPistolPose);

    }

    private void Update()
    {
        if (inputReader == null) return;

        movement?.Move(inputReader.MovementInput, inputReader.SprintHeld); 
        UpdateMovementAnimation();
        
        if (wasRunning && movement != null && !movement.IsRunning)
        {
            aim?.SetAimDirection(transform.forward);
        }

        wasRunning = movement != null && movement.IsRunning;

        if (movement != null && movement.IsRunning)
        {
            aim?.LookTowardsMovement(inputReader.MovementInput);
        }
        else
        {
            aim?.HandleAim(inputReader.MousePosition, inputReader.AimStickInput);
        }

        if (combat != null && movement != null && !movement.IsRunning && !combat.IsReloading() && combat.CurrentWeaponIsAutomatic() && inputReader.FireHeld)
        {
            EnterCombat();
            combat.UseCurrentWeapon();
        }

        aimingWithStick =inputReader.AimStickInput.sqrMagnitude > 0.1f;

        if (aimingWithStick)
        {
            EnterCombat();
        }
        UpdateCombatState();
    }

    private void UpdateMovementAnimation()
    {
        if (animator == null || inputReader == null) return;

        bool isMoving = inputReader.MovementInput.magnitude > 0.1f;

        animator.SetBool(isMovingBool, isMoving);

        if (movement != null)
            animator.SetBool(isRunningBool, movement.IsRunning);
    }

    private void HandleFirePressed()
    {
        if (combat == null) return;
        if (combat.IsReloading()) return;

        if (movement != null && movement.IsRunning)
            return;

        EnterCombat();

        if (combat.CurrentWeaponIsMelee() || !combat.CurrentWeaponIsAutomatic())
            combat.UseCurrentWeapon();
    }

    private void HandleReloadPressed()
    {
        combat?.TryReload();
    }

    private void HandleSwitchWeaponPressed()
    {
        if (combat != null && combat.IsReloading())
            return;

        combat?.SwitchWeapon();
    }

    private void HandleWeaponChanged(WeaponData weapon)
    {
        if (weapon == null) return;

        visuals?.UpdateWeaponVisuals(weapon);

        if (animator != null)
            animator.SetBool(isPistolBool, weapon.usesPistolPose);
    }

    private void OnDestroy()
    {
        if (inputReader != null)
        {
            inputReader.OnFirePressed -= HandleFirePressed;
            inputReader.OnSwitchWeaponPressed -= HandleSwitchWeaponPressed;
        }

        if (combat != null)
        {
            combat.OnWeaponChanged -= HandleWeaponChanged;
        }

        inputReader.OnReloadPressed -= HandleReloadPressed;
    }

    private void EnterCombat()
    {
        lastCombatTime = Time.time;

        if (isInCombat)
            return;

        isInCombat = true;

        if (animator != null)
            animator.SetBool(isCombatBool, true);
    }

    private void UpdateCombatState()
    {
        if (!isInCombat)
            return;

        bool timeout =
            Time.time > lastCombatTime + combatExitDelay;

        if (timeout)
        {
            isInCombat = false;

            if (animator != null)
                animator.SetBool(isCombatBool, false);
        }
    }

}