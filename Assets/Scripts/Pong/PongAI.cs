using UnityEngine;

public class PongAI : PongPaddle
{
    // For paddle classes, update is used to set the target position. The parent class will move
    // the paddle to reach this position in it's FixedUpdate function.
    private void Update()
    {
        SettingsRefresh();

        // If the ball is moving away from us, move towards the centre of the screen
        if (ball.Velocity.x < 0)
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
