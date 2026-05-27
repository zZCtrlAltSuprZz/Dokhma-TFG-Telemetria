using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputReader : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    public Vector2 MovementInput { get; private set; }
    public Vector2 MousePosition { get; private set; }
    public Vector2 AimStickInput { get; private set; }

    public bool FireHeld => fireAction != null && fireAction.IsPressed();
    public bool SprintHeld => sprintAction != null && sprintAction.IsPressed();

    public System.Action OnFirePressed;
    public System.Action OnSwitchWeaponPressed;
    public System.Action OnInteractPressed;
    public System.Action OnReloadPressed;
    public System.Action OnDashPressed;

    private InputAction moveAction;
    private InputAction mouseAction;
    private InputAction aimAction;
    private InputAction fireAction;
    private InputAction switchWeaponAction;
    private InputAction interactAction;
    private InputAction sprintAction;
    private InputAction reloadAction;
    private InputAction dashAction;

    private void Awake()
    {
        SetupInputSystem();
    }

    private void Update()
    {
        MovementInput = moveAction.ReadValue<Vector2>();
        MousePosition = mouseAction.ReadValue<Vector2>();
        AimStickInput = aimAction.ReadValue<Vector2>();
    }

    private void SetupInputSystem()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w")
            .With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a")
            .With("Right", "<Keyboard>/d");

        moveAction.AddBinding("<Gamepad>/leftStick");

        sprintAction = new InputAction("Sprint", InputActionType.Button);
        sprintAction.AddBinding("<Keyboard>/leftShift");
        sprintAction.AddBinding("<Gamepad>/leftStickPress");

        mouseAction = new InputAction("MousePosition", InputActionType.Value, "<Mouse>/position");

        aimAction = new InputAction("Aim", InputActionType.Value);
        aimAction.AddBinding("<Gamepad>/rightStick");

        fireAction = new InputAction("Fire", InputActionType.Button);
        fireAction.AddBinding("<Mouse>/leftButton");
        fireAction.AddBinding("<Gamepad>/rightTrigger").WithInteraction("press(pressPoint=0.5)");
        fireAction.performed += HandleFirePerformed;

        reloadAction = new InputAction("Reload", InputActionType.Button);
        reloadAction.AddBinding("<Keyboard>/r");
        reloadAction.AddBinding("<Gamepad>/buttonEast");
        reloadAction.performed += HandleReloadPerformed;

        switchWeaponAction = new InputAction("SwitchWeapon", InputActionType.Button);
        switchWeaponAction.AddBinding("<Keyboard>/q");
        switchWeaponAction.AddBinding("<Gamepad>/buttonNorth");
        switchWeaponAction.performed += HandleSwitchWeaponPerformed;

        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Gamepad>/buttonWest");
        interactAction.performed += HandleInteractPerformed;

        dashAction = new InputAction("Dash", InputActionType.Button);
        dashAction.AddBinding("<Keyboard>/space");
        dashAction.AddBinding("<Gamepad>/leftShoulder");
        dashAction.performed += HandleDashPerformed;

        EnableActions();

        if (showDebug)
            Debug.Log("Input configurado correctamente (teclado + mando)");
    }

    private void HandleFirePerformed(InputAction.CallbackContext context)
    {
        OnFirePressed?.Invoke();
    }

    private void HandleReloadPerformed(InputAction.CallbackContext context)
    {
        OnReloadPressed?.Invoke();
    }

    private void HandleSwitchWeaponPerformed(InputAction.CallbackContext context)
    {
        OnSwitchWeaponPressed?.Invoke();
    }

    private void HandleInteractPerformed(InputAction.CallbackContext context)
    {
        OnInteractPressed?.Invoke();
    }

    private void HandleDashPerformed(InputAction.CallbackContext context)
    {
        OnDashPressed?.Invoke();
    }

    private void OnEnable()
    {
        EnableActions();
    }

    private void OnDisable()
    {
        DisableActions();
    }

    private void OnDestroy()
    {
        if (fireAction != null)
            fireAction.performed -= HandleFirePerformed;

        if (reloadAction != null)
            reloadAction.performed -= HandleReloadPerformed;

        if (switchWeaponAction != null)
            switchWeaponAction.performed -= HandleSwitchWeaponPerformed;

        if (interactAction != null)
            interactAction.performed -= HandleInteractPerformed;

        if (dashAction != null)
            dashAction.performed -= HandleDashPerformed;

        DisableActions();
    }

    private void EnableActions()
    {
        moveAction?.Enable();
        mouseAction?.Enable();
        aimAction?.Enable();
        fireAction?.Enable();
        switchWeaponAction?.Enable();
        interactAction?.Enable();
        sprintAction?.Enable();
        reloadAction?.Enable();
        dashAction?.Enable();
    }

    private void DisableActions()
    {
        sprintAction?.Disable();
        moveAction?.Disable();
        mouseAction?.Disable();
        aimAction?.Disable();
        fireAction?.Disable();
        switchWeaponAction?.Disable();
        interactAction?.Disable();
        reloadAction?.Disable();
        dashAction?.Disable();
    }
}