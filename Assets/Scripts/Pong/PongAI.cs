using UnityEngine;

public class PongAI : PongPaddle
{
    // For paddle classes, update is used to set the target position. The parent class will move
    // the paddle to reach this position in it's FixedUpdate function.
    private void Update()
    {
        SettingsRefresh();

        // If the ball is moving away from us, move towards the centre of the screen
        var ballToPaddleDirection = paddle.transform.position - ball.transform.position;
        ballToPaddleDirection.Normalize();
        var ballMovementDirection = ball.Velocity;
        ballMovementDirection.Normalize();
        if (Vector3.Dot(ballMovementDirection, ballToPaddleDirection) < 0)
        {
            targetPosition.y = 0;
        } else {
            // Predict the y impact of the ball and move towards that
            // Time until the ball reaches our x position (assumes we only move in y and that
            // the ball won't lose energy en route)  
            var timeToIntercept = (paddle.transform.position.x - ball.transform.position.x) / ball.Velocity.x;

            targetPosition.y = ball.transform.position.y + (ball.Velocity.y * timeToIntercept);
        }
    }
}
