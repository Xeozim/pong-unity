using UnityEngine;
using UnityEngine.InputSystem;

public class PongPlayer : PongPaddle
{
    private PongControls controls;
    private float moveInput;

    private void Awake(){
        controls = new PongControls();
    }

    private void OnEnable()
    {
        // Enable input actions
        controls.Player.Enable();

        // Subscribe to the input actions
        controls.Player.MoveRelative.performed += OnRelativeMoveInput;
        controls.Player.MoveRelative.canceled += OnRelativeMoveInput;

        controls.Player.MoveAbsolute.performed += OnAbsoluteMoveInput;
        controls.Player.MoveAbsolute.canceled += OnAbsoluteMoveInput;
    }

    // For paddle classes, update is used to set the target position. The parent class will move
    // the paddle to reach this position in it's FixedUpdate function.
    private void Update(){
        SettingsRefresh();
    }

    private void OnDisable()
    {
        // Disable input actions
        controls.Player.Disable();
    }

    // Method for handling movement input from relative sources e.g. when the up arrow is pressed
    // we should always try to move the paddle up relative to where it is now.
    private void OnRelativeMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
        // Target position is relative to current position
        targetPosition = new Vector3(
            paddle.transform.position.x,
            paddle.transform.position.y + (settings.paddleSpeed * moveInput),
            paddle.transform.position.z
        );
    }

    // Method for handling movement input from absolute sources e.g. we should always aim to match
    // the current position of the gamepad joystick.
    private void OnAbsoluteMoveInput(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
        targetPosition = new Vector3(
            paddle.transform.position.x,
            settings.yMinimum + ((moveInput + 1.0f) * 0.5f * (settings.yMaxmium - settings.yMinimum)),
            paddle.transform.position.z
        );
    }
}
