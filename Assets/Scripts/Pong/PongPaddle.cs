using System.Collections.Generic;
using UnityEngine;

// Base class for paddles (player and AI)
public class PongPaddle : MonoBehaviour
{
    [SerializeField] protected Transform paddle;
    [SerializeField] protected List<MeshRenderer> renderers;
    [SerializeField] protected List<Collider> colliders;
    [SerializeField] protected PongSettings settings;

    protected PongBall ball;
    protected Vector3 targetPosition;

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<PongBall>();
        targetPosition = paddle.transform.position;
    }

    protected void SettingsRefresh(){
        paddle.transform.localScale = new Vector3(settings.paddleWidth,settings.paddleHeight,1);
    }

    public void OnGameOverStateUpdated(bool isGameOver)
    {
        Debug.Log($"OnGameOverStateUpdated ({isGameOver}) called on {transform.name}");
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
            paddle.transform.position = new Vector3(paddle.transform.position.x,0,paddle.transform.position.z);
        }
    }

    // FixedUpdate is called at the same rate as the physics system update
    void FixedUpdate()
    {
        // Set velocity to achieve the target position, very simple proportional control by clamping
        // Multiplying position offset by 10 means we use full speed unless the target position is
        // within paddleSpeed / 10 of the current. In effect we always move at full speed.
        var velocity = Mathf.Clamp((targetPosition.y - paddle.transform.position.y) * 10, -settings.paddleSpeed, settings.paddleSpeed);

        // Update position, clamping to game limits
        var newYPosition = Mathf.Clamp(paddle.transform.position.y + velocity * Time.fixedDeltaTime, settings.yMinimum, settings.yMaxmium);
        paddle.transform.position = new Vector3(paddle.transform.position.x,newYPosition,paddle.transform.position.z);
    }
}
