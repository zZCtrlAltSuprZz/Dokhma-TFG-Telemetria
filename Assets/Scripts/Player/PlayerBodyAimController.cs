using UnityEngine;

public class PlayerBodyAimController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAim aim;
    [SerializeField] private PlayerInputReader input;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform spineBone;

    [Header("Aim Limits")]
    [SerializeField] private float maxSpineAngle = 90f;

    [Header("Rotation Speeds")]
    [SerializeField] private float bodyTurnSpeed = 10f;
    [SerializeField] private float spineTurnSpeed = 12f;

    [Header("Animator Params")]
    [SerializeField] private string moveXParam = "MoveX";
    [SerializeField] private string moveYParam = "MoveY";

    [SerializeField] private PlayerMovement movement;

    private Quaternion spineStartLocalRotation;

    private void Awake()
    {
        if (aim == null)
            aim = GetComponent<PlayerAim>();

        if (input == null)
            input = GetComponent<PlayerInputReader>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (movement == null) 
            movement = GetComponent<PlayerMovement>();

        if (spineBone != null)
            spineStartLocalRotation = spineBone.localRotation;
    }

    private void Update()
    {
        RotateBodyIfAimExceedsLimit();
        UpdateMovementAnimator();
    }



    private void LateUpdate()
    {
        if (movement != null && movement.IsRunning)
        {
            ResetSpineRotation();
            return;
        }

        RotateSpineTowardsAim();
    }

    private void RotateBodyIfAimExceedsLimit()
    {
        if (aim == null) return;

        Vector3 aimDirection = aim.LastAimDirection;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < 0.001f)
            return;

        aimDirection.Normalize();

        float angle = Vector3.SignedAngle(
            transform.forward,
            aimDirection,
            Vector3.up
        );

        if (Mathf.Abs(angle) <= maxSpineAngle)
            return;

        float extraAngle = angle - Mathf.Sign(angle) * maxSpineAngle;

        Quaternion targetRotation =
            Quaternion.AngleAxis(extraAngle, Vector3.up) * transform.rotation;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            bodyTurnSpeed * Time.deltaTime
        );
    }

    private void RotateSpineTowardsAim()
    {
        if (spineBone == null || aim == null)
            return;

        Vector3 aimDirection = aim.LastAimDirection;
        aimDirection.y = 0f;

        if (aimDirection.sqrMagnitude < 0.001f)
            return;

        aimDirection.Normalize();

        float angle = Vector3.SignedAngle(
            transform.forward,
            aimDirection,
            Vector3.up
        );

        angle = Mathf.Clamp(angle, -maxSpineAngle, maxSpineAngle);

        Debug.Log("AimDir: " + aimDirection + " | Angle: " + angle);

        spineBone.localRotation =
            spineStartLocalRotation * Quaternion.Euler(0f, angle, 0f);
    }

    private void ResetSpineRotation()
    {
        if (spineBone == null) return;

        spineBone.localRotation = Quaternion.Slerp(spineBone.localRotation, spineStartLocalRotation, spineTurnSpeed * Time.deltaTime);
    }

    private void UpdateMovementAnimator()
    {
        if (animator == null || input == null)
            return;

        Vector2 movementInput = input.MovementInput;

        Vector3 worldMoveDirection = new Vector3(
            movementInput.x,
            0f,
            movementInput.y
        );

        if (worldMoveDirection.sqrMagnitude < 0.01f)
        {
            animator.SetFloat(moveXParam, 0f, 0.1f, Time.deltaTime);
            animator.SetFloat(moveYParam, 0f, 0.1f, Time.deltaTime);
            return;
        }

        worldMoveDirection.Normalize();

        float moveX = Vector3.Dot(transform.right, worldMoveDirection);
        float moveY = Vector3.Dot(transform.forward, worldMoveDirection);

        animator.SetFloat(moveXParam, moveX, 0.1f, Time.deltaTime);
        animator.SetFloat(moveYParam, moveY, 0.1f, Time.deltaTime);
    }
}