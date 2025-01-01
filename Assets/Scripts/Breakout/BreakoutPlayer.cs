using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class BreakoutPlayer : MonoBehaviour
{
    [SerializeField] private Transform _paddle;
    [SerializeField] private List<MeshRenderer> _renderers;
    [SerializeField] private List<Collider> _colliders;
    [SerializeField] private BreakoutSettings _settings;

    private Vector3 _targetPosition;
    private BreakoutControls _controls;
    private float _moveInput;
    private float _paddleWidth;

    private void Awake(){
        _controls = new BreakoutControls();
        _paddleWidth = _settings.paddleWidth;
    }

    private void OnEnable()
    {
        // Enable input actions
        _controls.Player.Enable();

        // Subscribe to the input actions
        _controls.Player.Move.performed += OnMoveInputPerformed;
        _controls.Player.Move.canceled += OnMoveInputCancelled;
    }

    private void OnDisable()
    {
        // Disable input actions
        _controls.Player.Disable();
    }

    public void OnGameStageUpdated(GameStage stage)
    {
        _paddleWidth = stage switch
        {
            GameStage.StageFive => _settings.paddleWidth / 2,
            _ => _settings.paddleWidth,
        };
        SettingsRefresh();
    }

    // Method for handling movement input from absolute sources e.g. we should always aim to match
    // the current position of the gamepad joystick.
    private void OnMoveInputPerformed(InputAction.CallbackContext context)
    {
        _moveInput = context.ReadValue<float>();

        // Target y in range 0 (target = yMinimum) to 1 (target = yMaximum)
        // Adjust mouse / touchscreen inputs to be relative to the screen
        var xTarget01 = context.control.device switch
        {
            Mouse or Touchscreen => _moveInput / Screen.height,
            _ => (_moveInput + 1.0f) * 0.5f,
        };

        // Set target position
        _targetPosition = new Vector3(
            _settings.xMinimum + (xTarget01 * (_settings.xMaxmium - _settings.xMinimum)),
            _paddle.transform.position.y,
            _paddle.transform.position.z
        );
    }
    // When input stops always maintain current position
    private void OnMoveInputCancelled(InputAction.CallbackContext context)
    {
        _targetPosition = new Vector3(
            _paddle.transform.position.x,
            _paddle.transform.position.y,
            _paddle.transform.position.z
        );
    }

    // Start is called before the first frame update
    void Start()
    {
        _targetPosition = _paddle.transform.position;
    }

    protected void SettingsRefresh(){
        _paddle.transform.localScale = new Vector3(_paddleWidth,_settings.paddleHeight,1);
    }

    // For paddle classes, update is used to set the target position. The parent class will move
    // the paddle to reach this position in it's FixedUpdate function.
    private void Update(){
        SettingsRefresh();
    }

    // FixedUpdate is called at the same rate as the physics system update
    void FixedUpdate()
    {
        // Set velocity to achieve the target position, very simple proportional control by clamping
        // Multiplying position offset by 10 means we use full speed unless the target position is
        // within paddleSpeed / 10 of the current. In effect we always move at full speed.
        var velocity = Mathf.Clamp((_targetPosition.x - _paddle.transform.position.x) * 10, -_settings.paddleSpeed, _settings.paddleSpeed);

        // Update position, clamping to game limits
        var newXPosition = Mathf.Clamp(_paddle.transform.position.x + velocity * Time.fixedDeltaTime, _settings.xMinimum, _settings.xMaxmium);
        _paddle.transform.position = new Vector3(newXPosition, _paddle.transform.position.y,_paddle.transform.position.z);
    }
}
