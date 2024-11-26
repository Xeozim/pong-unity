using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BreakoutPlayer : MonoBehaviour
{
    [SerializeField] protected Transform paddle;
    [SerializeField] protected List<MeshRenderer> renderers;
    [SerializeField] protected List<Collider> colliders;
    [SerializeField] protected BreakoutSettings settings;

    protected Vector3 targetPosition;
    private BreakoutControls controls;
    private float moveInput;
    private float rotateInput;

    private void Awake(){
        controls = new BreakoutControls();
    }

    private void OnEnable()
    {
        // Enable input actions
        controls.Player.Enable();

        // Subscribe to the input actions
        controls.Player.Move.performed += OnMoveInputPerformed;
        controls.Player.Move.canceled += OnMoveInputCancelled;
        controls.Player.Rotate.performed += OnRotateInputPerformed;
        controls.Player.Rotate.canceled += OnRotateInputCancelled;
    }

    private void OnDisable()
    {
        // Disable input actions
        controls.Player.Disable();
    }

    // DEBUG
    private void OnRotateInputPerformed(InputAction.CallbackContext context)
    {
        rotateInput = context.ReadValue<float>();
    }
    private void OnRotateInputCancelled(InputAction.CallbackContext context)
    {
        rotateInput = 0;
    }

    // Method for handling movement input from absolute sources e.g. we should always aim to match
    // the current position of the gamepad joystick.
    private void OnMoveInputPerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();

        // Target y in range 0 (target = yMinimum) to 1 (target = yMaximum)
        // Adjust mouse / touchscreen inputs to be relative to the screen
        var xTarget01 = context.control.device switch
        {
            Mouse or Touchscreen => moveInput / Screen.height,
            _ => (moveInput + 1.0f) * 0.5f,
        };

        // Set target position
        targetPosition = new Vector3(
            settings.xMinimum + (xTarget01 * (settings.xMaxmium - settings.xMinimum)),
            paddle.transform.position.y,
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

    // Start is called before the first frame update
    void Start()
    {
        targetPosition = paddle.transform.position;
    }

    protected void SettingsRefresh(){
        paddle.transform.localScale = new Vector3(settings.paddleWidth,settings.paddleHeight,1);
    }

    // For paddle classes, update is used to set the target position. The parent class will move
    // the paddle to reach this position in it's FixedUpdate function.
    private void Update(){
        SettingsRefresh();
    }

    public void OnGameOverStateUpdated(bool isGameOver)
    {
        // Debug.Log($"OnGameOverStateUpdated ({isGameOver}) called on {transform.name}");
        foreach (var renderer in renderers)
        {
            renderer.enabled = !isGameOver;
        }
        foreach (var collider in colliders)
        {
            collider.enabled = !isGameOver;
        }

        if (!isGameOver) {
            // Game was restarted, reset to the centre of the screen
            paddle.transform.position = new Vector3(0,paddle.transform.position.y,paddle.transform.position.z);
        }
    }

    // FixedUpdate is called at the same rate as the physics system update
    void FixedUpdate()
    {
        // Set velocity to achieve the target position, very simple proportional control by clamping
        // Multiplying position offset by 10 means we use full speed unless the target position is
        // within paddleSpeed / 10 of the current. In effect we always move at full speed.
        var velocity = Mathf.Clamp((targetPosition.x - paddle.transform.position.x) * 10, -settings.paddleSpeed, settings.paddleSpeed);

        // Update position, clamping to game limits
        var newXPosition = Mathf.Clamp(paddle.transform.position.x + velocity * Time.fixedDeltaTime, settings.xMinimum, settings.xMaxmium);
        paddle.transform.position = new Vector3(newXPosition, paddle.transform.position.y,paddle.transform.position.z);

        // Rotation
        paddle.transform.Rotate(Vector3.forward, rotateInput * 2);
    }
}
