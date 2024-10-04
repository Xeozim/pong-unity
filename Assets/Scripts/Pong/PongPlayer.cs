using UnityEngine;
using UnityEngine.InputSystem;

public class PongPlayer : MonoBehaviour
{
    private PongControls controls;
    private float moveInput;

    [SerializeField] private PongSettings settings;

    private void Awake(){
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

    // Update is called once per frame
    void Update()
    {
        var velocity = moveInput * settings.paddleSpeed;
        var newYPosition = Mathf.Clamp(transform.position.y + velocity * Time.deltaTime, settings.yMinimum, settings.yMaxmium);

        transform.position = new Vector3(transform.position.x,newYPosition,transform.position.z);
    }
}
