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
        controls.Player.Move.performed += OnMoveInputPerformed;
        controls.Player.Move.canceled += OnMoveInputCancelled;
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

    // Method for handling movement input from absolute sources e.g. we should always aim to match
    // the current position of the gamepad joystick.
    private void OnMoveInputPerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();

        // Adjust mouse / touchscreen inputs to be relative to the screen
        switch (context.control.device){
            case Mouse:
            case Touchscreen:
                moveInput /= Screen.height;
                break;
            default:
                break;
        }

        targetPosition = new Vector3(
            paddle.transform.position.x,
            settings.yMinimum + ((moveInput + 1.0f) * 0.5f * (settings.yMaxmium - settings.yMinimum)),
            paddle.transform.position.z
        );
    }
    // When input stops always maintain current position
    private void OnMoveInputCancelled(InputAction.CallbackContext context)
    {
        targetPosition = new Vector3(
            paddle.transform.position.x,
            paddle.transform.position.y,
            paddle.transform.position.z
        );
    }
}
