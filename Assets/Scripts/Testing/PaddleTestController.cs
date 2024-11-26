using UnityEngine;
using UnityEngine.InputSystem;

public class PaddleTestController : MonoBehaviour
{
    [SerializeField] protected Transform paddle;
    [SerializeField] private float translationSensitivity;
    [SerializeField] private float rotationSensitivity;

    private PaddleTestControls controls;
    private float leftRightInput;
    private float upDownInput;
    private float rotateInput;

    private void Awake(){
        controls = new PaddleTestControls();
    }

    private void OnEnable()
    {
        // Enable input actions
        controls.Player.Enable();

        // Subscribe to the input actions
        controls.Player.LeftRight.performed += OnLeftRightInputPerformed;
        controls.Player.LeftRight.canceled += OnLeftRightInputCancelled;
        controls.Player.UpDown.performed += OnUpDownInputPerformed;
        controls.Player.UpDown.canceled += OnUpDownInputCancelled;
        controls.Player.Rotate.performed += OnRotateInputPerformed;
        controls.Player.Rotate.canceled += OnRotateInputCancelled;
    }

    private void OnDisable()
    {
        // Disable input actions
        controls.Player.Disable();
    }

    private void OnLeftRightInputPerformed(InputAction.CallbackContext context)
    {
        leftRightInput = context.ReadValue<float>();
    }
    private void OnLeftRightInputCancelled(InputAction.CallbackContext context)
    {
        leftRightInput = 0;
    }
    private void OnUpDownInputPerformed(InputAction.CallbackContext context)
    {
        upDownInput = context.ReadValue<float>();
    }
    private void OnUpDownInputCancelled(InputAction.CallbackContext context)
    {
        upDownInput = 0;
    }
    private void OnRotateInputPerformed(InputAction.CallbackContext context)
    {
        rotateInput = context.ReadValue<float>();
    }
    private void OnRotateInputCancelled(InputAction.CallbackContext context)
    {
        rotateInput = 0;
    }

    void FixedUpdate()
    {
        paddle.transform.Translate(x: leftRightInput * translationSensitivity, y: upDownInput * translationSensitivity, z:0, relativeTo: Space.World);
        paddle.transform.Rotate(Vector3.forward, rotateInput * rotationSensitivity);
    }
}
