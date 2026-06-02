using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private enum AimSource
    {
        None,
        Mouse,
        Gamepad
    }

    [Header("Aim Settings")]
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private float rotationSmoothness = 10f;
    [SerializeField] private float gamepadAimDeadzone = 0.2f;
    [SerializeField] private float mouseMoveThreshold = 2f;
    [SerializeField] private bool showDebug = true;

    private Camera mainCamera;
    private Vector3 lastMouseWorldPos;
    private Vector3 lastAimDirection = Vector3.forward;

    [SerializeField] private Transform firePoint;

    private AimSource currentAimSource;
    private Vector2 previousMousePosition;

    public Vector3 LastAimDirection => lastAimDirection;
    public Vector3 LastMouseWorldPos => lastMouseWorldPos;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    public void HandleAim(Vector2 mousePosition, Vector2 aimStickInput)
    {
        bool stickActive = aimStickInput.sqrMagnitude > 0.001f;
        bool mouseMoved = Vector2.Distance(mousePosition, previousMousePosition) > mouseMoveThreshold;

        if (stickActive)
        {
            currentAimSource = AimSource.Gamepad;
            AimWithStick(aimStickInput);
        }
        else if (mouseMoved)
        {
            currentAimSource = AimSource.Mouse;
            AimWithMouse(mousePosition);
        }
        else
        {
            KeepLastAimDirection();
        }

        previousMousePosition = mousePosition;
    }

    public void LookTowardsMovement(Vector2 movementInput)
    {
        Vector3 direction = new Vector3(
            movementInput.x,
            0f,
            movementInput.y
        );

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }

    private void AimWithStick(Vector2 aimStickInput)
    {
        Vector3 aimDirection = new Vector3(aimStickInput.x, 0f, aimStickInput.y).normalized;
        lastAimDirection = aimDirection;

        //RotateToDirection(lastAimDirection);

        if (showDebug)
            Debug.DrawLine(transform.position, transform.position + aimDirection * 3f, Color.cyan);
    }

    private void AimWithMouse(Vector2 mousePosition)
    {
        if (mainCamera == null) return;

        float aimHeight = firePoint != null ? firePoint.position.y : transform.position.y;

        Plane aimPlane = new Plane(Vector3.up, new Vector3(0f, aimHeight, 0f));
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(mousePosition.x, mousePosition.y, 0f));

        if (!aimPlane.Raycast(ray, out float distance))
            return;

        Vector3 targetPosition = ray.GetPoint(distance);
        lastMouseWorldPos = targetPosition;

        Vector3 direction = targetPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.01f) return;

        lastAimDirection = direction.normalized;
        //RotateToDirection(lastAimDirection);

        if (showDebug)
            Debug.DrawLine(transform.position, targetPosition, Color.red, 0.1f);
    }

    private void KeepLastAimDirection()
    {
        if (lastAimDirection.sqrMagnitude <= 0.0001f) return;

        //RotateToDirection(lastAimDirection);
    }


    /*private void RotateToDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.0001f) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSmoothness * Time.deltaTime
        );
    }*/

    public void SetAimDirection(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return;

        lastAimDirection = direction.normalized;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showDebug) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(lastMouseWorldPos, 0.3f);

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(transform.position, lastAimDirection * 3f);
    }
}