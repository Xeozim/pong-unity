using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PongAI : MonoBehaviour
{
    [SerializeField] private PongSettings settings;

    private PongBall ball;
    private Vector3 targetPosition;

    new MeshRenderer renderer;
    private List<Collider> colliders;

    private void Awake()
    {
        renderer = GetComponent<MeshRenderer>();
        colliders = GetComponents<Collider>().ToList();
    }

    void Start()
    {
        ball = GameObject.FindGameObjectWithTag("Ball").GetComponent<PongBall>();
        targetPosition = transform.position;
    }
    
    private void Update()
    {
        transform.localScale.Set(settings.paddleWidth,settings.paddleHeight,1);
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

    // FixedUpdate is called at the same rate as the physics system update
    void FixedUpdate()
    {
        // If the ball is moving away from us, move towards the centre of the screen
        if (ball.Velocity.x < 0)
        {
            targetPosition.y = 0;
        } else {
            // Predict the y impact of the ball and move towards that
            // Time until the ball reaches our x position (assumes we only move in y and that
            // the ball won't lose energy en route)  
            var timeToIntercept = (transform.position.x - ball.transform.position.x) / ball.Velocity.x;

            targetPosition.y = ball.transform.position.y + (ball.Velocity.y * timeToIntercept);
        }

        // Set velocity to achieve the target position, very simple proportional control by clamping
        // Multiplying position offset by 2 means we use full speed unless the target position is
        // within paddleSpeed / 2 of the current. Really we should use the actual dimensions of the
        // paddle to control this because this behaviour will change with paddle speed.
        var velocity = Mathf.Clamp((targetPosition.y - transform.position.y) * 2, -settings.paddleSpeed, settings.paddleSpeed);

        // Update position, clamping to game limits
        var newYPosition = Mathf.Clamp(transform.position.y + velocity * Time.fixedDeltaTime, settings.yMinimum, settings.yMaxmium);
        transform.position = new Vector3(transform.position.x,newYPosition,transform.position.z);
    }
}
