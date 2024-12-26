using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PongBall : Ball
{
    [SerializeField] private PongSettings settings;

    public UnityEvent ballEnteredPlayerGoal;
    public UnityEvent ballEnteredOpponentGoal;

    private bool gameOver = false;

    public void OnGameOverStateUpdated(bool isGameOver)
    {
        gameOver = isGameOver;
        if (!isGameOver) {
            // Game was restarted, reset the ball
            ResetBall(true);
        }
    }

    public override void ResetBall(bool playerLost, bool skipWait = false)
    {
        // Initialise in a random position on the y axis
        var yInitial = Random.Range(settings.yMinimum + transform.localScale.y, settings.yMaxmium - transform.localScale.y);
        transform.position = new Vector3(0,yInitial,0);

        // Initialise moving at 60 degrees either up or down
        // Assumes the player is on the left and fires towards the player that lost the last point
        var velRotation = Quaternion.Euler(0, 0, settings.initialBallAngle * Mathf.Sign(Random.value - 0.5f));
        Velocity = velRotation * new Vector3((playerLost ? -1 : 1) * settings.initialBallSpeed,0);

        if (!skipWait) { StartCoroutine(ResetWait(settings.resetWait)); }
    }

    protected override void ColliderHit(RaycastHit hit)
    {
        if (hit.collider.gameObject.CompareTag("PlayerPaddle") || hit.collider.gameObject.CompareTag("OpponentPaddle"))
        {
            Velocity = BallBehaviours.VelocityAfterPaddleCollision(Velocity, hit.point, hit.transform);
            Velocity = BallBehaviours.ApplyAngleLimitToVector(Velocity, settings.maximumBallAngle, hit.transform.up);
            Velocity = BallBehaviours.VelocityAfterSpeedLimit(Velocity * (1 + settings.ballSpeedIncreaseOnPaddleHit), settings.maximumBallSpeed);
            PlayBounceNoise();
        } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
        {
            Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, hit.normal);
            PlayBounceNoise();
        }
    }

    protected override void TriggerHit(RaycastHit hit)
    {
        // Debug.Log("Ball Trigger!");
        if (hit.collider.gameObject.CompareTag("PlayerGoal"))
        {
            // Debug.Log("Ball entered the PlayerGoal trigger zone!");
            // Ignore goals if game is over
            if (gameOver) {return;}
            ballEnteredPlayerGoal.Invoke();
            ResetBall(true);
        } else if (hit.collider.gameObject.CompareTag("OpponentGoal"))
        {
            // Debug.Log("Ball entered the OpponentGoal trigger zone!");
            // Ignore goals if game is over
            if (gameOver) {return;}
            ballEnteredOpponentGoal.Invoke();
            ResetBall(false);
        }
    }
}
