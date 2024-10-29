using UnityEngine;
using UnityEngine.InputSystem;

public class PongPlayer : PongPaddle
{
    private PongControls controls;
    private float moveInput;

    private void Awake(){
        GetComponentReferences();
        controls = new PongControls();
    }

    private void OnEnable()
    {
        // Enable input actions
        controls.Player.Enable();

        // Subscribe to the input actions
        controls.Player.Move.performed += OnMove;
        controls.Player.Move.canceled += OnMove;
    }

    private void Update(){
        SettingsRefresh();
    }

    private void OnDisable()
    {
        // Disable input actions
        controls.Player.Disable();
    }

    // Method for handling movement input
    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<float>();
    }

    // FixedUpdate is called at the same rate as the physics system update
    void FixedUpdate()
    {
        var velocity = moveInput * settings.paddleSpeed;
        var newYPosition = Mathf.Clamp(paddle.transform.position.y + velocity * Time.fixedDeltaTime, settings.yMinimum, settings.yMaxmium);

        paddle.transform.position = new Vector3(paddle.transform.position.x,newYPosition,paddle.transform.position.z);
    }
}
