using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PongPlayer : MonoBehaviour
{
    new MeshRenderer renderer;
    private List<Collider> colliders;

    private PongControls controls;
    private float moveInput;

    [SerializeField] private PongSettings settings;

    private void Awake(){
        controls = new PongControls();
        renderer = GetComponent<MeshRenderer>();
        colliders = GetComponents<Collider>().ToList();
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
        transform.localScale.Set(settings.paddleWidth,settings.paddleHeight,1);
    }

    private void OnDisable()
    {
        // Disable input actions
        controls.Player.Disable();
    }

    public void OnGameOverStateUpdated(bool isGameOver)
    {
        renderer.enabled = !isGameOver;
        colliders.Select(c => c.enabled = !isGameOver);

        if (!isGameOver) {
            // Game was restarted, reset to the centre of the screen
            transform.position = new Vector3(transform.position.x,0,transform.position.z);
        }
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
        var newYPosition = Mathf.Clamp(transform.position.y + velocity * Time.fixedDeltaTime, settings.yMinimum, settings.yMaxmium);

        transform.position = new Vector3(transform.position.x,newYPosition,transform.position.z);
    }
}
