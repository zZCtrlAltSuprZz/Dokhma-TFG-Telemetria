using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 8f;
    [SerializeField] private float runSpeed = 13f;

    [Header("Gravity")]
    [SerializeField] private float gravity = -35f;
    [SerializeField] private float groundedForce = -5f;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaDrainRate = 25f;
    [SerializeField] private float staminaRegenRate = 40f;
    [SerializeField, Range(0f, 1f)] private float staminaPercentToRunAgain = 0.5f;
    [SerializeField] private Image staminaFillImage;

    [Header("World Stamina UI")]
    [SerializeField] private GameObject staminaWorldUI;
    [SerializeField] private float staminaUIHideDelay = 0.5f;
    [SerializeField] private bool rotateUIToCamera = true;
    [SerializeField] private Vector3 staminaUILocalPosition = new Vector3(0f, 0.2f, 0f);

    [Header("StaminUp Perk")]
    [SerializeField] private float staminUpWalkSpeedMultiplier = 1.2f;
    [SerializeField] private float staminUpRunSpeedMultiplier = 1.25f;
    [SerializeField] private float staminUpMaxStaminaMultiplier = 1.5f;

    [Header("Dash Perk")]
    [SerializeField] private float dashSpeed = 28f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 3f;

    [SerializeField] private Animator animator;
    [SerializeField] private string isDashingBool = "isDashing";

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private CharacterController controller;
    private PlayerInputReader input;


    private bool staminaLocked;
    private float verticalVelocity;

    private bool hasStaminUp;
    private bool hasDash;
    private bool isDashing;
    private bool wasMoving;

    private float nextDashTime;
    private float staminaUIHideTimer;

    private Coroutine dashRoutine;

    public float CurrentStamina { get; private set; }
    public float MaxStamina => maxStamina;

    public bool IsRunning { get; private set; }
    public bool HasDash => hasDash;
    public bool IsDashing => isDashing;

    public float DashCooldownRemaining => Mathf.Max(0f, nextDashTime - Time.time);

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputReader>();

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        CurrentStamina = maxStamina;

        if (staminaWorldUI != null)
        {
            staminaWorldUI.SetActive(false);
        }

        UpdateStaminaUI();
    }

    private void OnEnable()
    {
        if (input != null)
        {
            input.OnDashPressed += HandleDash;
        }
    }

    private void OnDisable()
    {
        if (input != null)
        {
            input.OnDashPressed -= HandleDash;
        }
    }

    private void LateUpdate()
    {
        UpdateWorldStaminaUITransform();
    }

    private void UpdateWorldStaminaUITransform()
    {
        if (staminaWorldUI == null)
        {
            return;
        }

        // Mantener posición fija encima del player
        staminaWorldUI.transform.localPosition = staminaUILocalPosition;

        // Mirar siempre a cámara
        if (rotateUIToCamera && Camera.main != null)
        {
            staminaWorldUI.transform.rotation =
                Quaternion.LookRotation(Camera.main.transform.forward);
        }
    }

    private void HandleDash()
    {
        if (input == null)
        {
            return;
        }

        TryDash(input.MovementInput);
    }

    public void Move(Vector2 movementInput, bool wantsToRun)
    {
        if (isDashing)
        {
            return;
        }

        bool isMoving = movementInput.magnitude > 0.1f;

        if (wasMoving && !isMoving && input != null)
        {
            input.CancelGamepadSprintToggle();
        }

        wasMoving = isMoving;

        float staminaPercent = CurrentStamina / maxStamina;

        if (CurrentStamina <= 0f)
        {
            staminaLocked = true;

            if (input != null)
                input.CancelGamepadSprintToggle();
        }

        if (staminaLocked && staminaPercent >= staminaPercentToRunAgain)
        {
            staminaLocked = false;
        }

        bool canRun =
            wantsToRun &&
            isMoving &&
            !staminaLocked &&
            CurrentStamina > 0f;

        IsRunning = canRun;

        float currentSpeed =
            IsRunning ? runSpeed : walkSpeed;

        Vector3 movement = Vector3.zero;

        if (isMoving)
        {
            movement = new Vector3(
                movementInput.x,
                0f,
                movementInput.y
            );

            movement.Normalize();

            movement *= currentSpeed;
        }

        ApplyGravity(ref movement);

        controller.Move(movement * Time.deltaTime);

        UpdateStamina();
    }

    public void TryDash(Vector2 movementInput)
    {
        if (!hasDash) return;
        if (isDashing) return;

        if (Time.time < nextDashTime)
        {
            if (showDebug)
            {
                Debug.Log(
                    "Dash en cooldown: " +
                    DashCooldownRemaining.ToString("F1") + "s"
                );
            }

            return;
        }

        Vector3 dashDirection;

        if (movementInput.magnitude > 0.1f)
        {
            Vector3 forward = Camera.main.transform.forward;
            Vector3 right = Camera.main.transform.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            dashDirection =
                forward * movementInput.y +
                right * movementInput.x;

            dashDirection.Normalize();
        }
        else
        {
            dashDirection = transform.forward;
        }

        dashDirection.y = 0f;

        transform.forward = dashDirection;

        dashRoutine = StartCoroutine(DashRoutine(dashDirection));
    }

    private IEnumerator DashRoutine(Vector3 direction)
    {
        isDashing = true;

        nextDashTime = Time.time + dashCooldown;

        if (animator != null)
        {
            animator.SetBool(isDashingBool, true);
        }

        float timer = 0f;

        while (timer < dashDuration)
        {
            transform.forward = direction;

            Vector3 movement = direction * dashSpeed;

            ApplyGravity(ref movement);

            controller.Move(movement * Time.deltaTime);

            timer += Time.deltaTime;

            yield return null;
        }

        isDashing = false;

        if (animator != null)
        {
            animator.SetBool(isDashingBool, false);
        }

        dashRoutine = null;
    }

    private void ApplyGravity(ref Vector3 movement)
    {
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = groundedForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        movement.y = verticalVelocity;
    }

    private void UpdateStamina()
    {
        if (IsRunning)
        {
            CurrentStamina = Mathf.Max(
                CurrentStamina -
                staminaDrainRate * Time.deltaTime,
                0f
            );
        }
        else
        {
            CurrentStamina = Mathf.Min(
                CurrentStamina +
                staminaRegenRate * Time.deltaTime,
                maxStamina
            );
        }

        UpdateStaminaUI();
    }

    private void UpdateStaminaUI()
    {
        float staminaPercent = CurrentStamina / maxStamina;

        // Fill radial
        if (staminaFillImage != null)
        {
            staminaFillImage.fillAmount = staminaPercent;
        }

        // Mostrar solo cuando no está llena
        if (staminaWorldUI != null)
        {
            bool shouldShow = staminaPercent < 0.99f;

            if (shouldShow)
            {
                staminaUIHideTimer = staminaUIHideDelay;

                if (!staminaWorldUI.activeSelf)
                {
                    staminaWorldUI.SetActive(true);
                }
            }
            else
            {
                staminaUIHideTimer -= Time.deltaTime;

                if (staminaUIHideTimer <= 0f &&
                    staminaWorldUI.activeSelf)
                {
                    staminaWorldUI.SetActive(false);
                }
            }
        }
    }

    public float GetStaminaPercent()
    {
        return CurrentStamina / maxStamina;
    }

    public void ApplyStaminUp()
    {
        if (hasStaminUp)
        {
            return;
        }

        hasStaminUp = true;

        walkSpeed *= staminUpWalkSpeedMultiplier;
        runSpeed *= staminUpRunSpeedMultiplier;

        maxStamina *= staminUpMaxStaminaMultiplier;

        CurrentStamina = maxStamina;

        UpdateStaminaUI();

        if (showDebug)
        {
            Debug.Log(
                "StaminUp aplicado | " +
                "WalkSpeed: " + walkSpeed +
                " | RunSpeed: " + runSpeed +
                " | MaxStamina: " + maxStamina
            );
        }
    }

    public void ApplyDash()
    {
        if (hasDash)
        {
            return;
        }

        hasDash = true;

        if (showDebug)
        {
            Debug.Log("Dash perk aplicado.");
        }
    }
}