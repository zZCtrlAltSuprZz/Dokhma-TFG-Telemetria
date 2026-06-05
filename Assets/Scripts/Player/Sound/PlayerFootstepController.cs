using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerFootstepController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerAudio playerAudio;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Step Timing")]
    [SerializeField] private float walkStepInterval = 0.42f;
    [SerializeField] private float runStepInterval = 0.26f;

    [Header("Movement Check")]
    [SerializeField] private float minMoveSpeed = 0.2f;
    [SerializeField] private bool onlyWhenGrounded = true;

    private CharacterController controller;
    private float stepTimer;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (playerAudio == null)
            playerAudio = GetComponent<PlayerAudio>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (playerHealth == null)
            playerHealth = GetComponent<PlayerHealth>();
    }

    private void Update()
    {
        if (Time.timeScale == 0f)
            return;

        if (playerHealth != null && !playerHealth.IsAlive)
        {
            stepTimer = 0f;
            return;
        }

        if (playerMovement != null && playerMovement.IsDashing)
        {
            stepTimer = 0f;
            return;
        }

        if (onlyWhenGrounded && !controller.isGrounded)
        {
            stepTimer = 0f;
            return;
        }

        Vector3 horizontalVelocity = controller.velocity;
        horizontalVelocity.y = 0f;

        bool isMoving = horizontalVelocity.magnitude > minMoveSpeed;

        if (!isMoving)
        {
            stepTimer = 0f;
            return;
        }

        bool isRunning = playerMovement != null && playerMovement.IsRunning;
        float interval = isRunning ? runStepInterval : walkStepInterval;

        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            playerAudio.PlayFootstep(isRunning);
            stepTimer = 0f;
        }
    }
}