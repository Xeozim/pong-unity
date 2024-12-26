using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BreakoutBall : Ball
{
    [SerializeField] private BreakoutSettings settings;

    public override void ResetBall(bool playerLost, bool skipWait = false)
    {
        // Initialise in a random position on the x axis
        var xInitial = Random.Range(settings.xMinimum + transform.localScale.y, settings.xMaxmium - transform.localScale.x);
        transform.position = new Vector3(xInitial,0,0);

        // Initialise moving at 60 degrees either up or down
        // Assumes the player is on the left and fires towards the player that lost the last point
        var velRotation = Quaternion.Euler(0, 0, settings.maximumBallAngle * Mathf.Sign(Random.value - 0.5f));
        Velocity = velRotation * new Vector3((playerLost ? -1 : 1) * settings.ballSpeedStage1,0);

        if (!skipWait) { StartCoroutine(ResetWait(settings.resetWait)); }
    }

    protected override void ColliderHit(RaycastHit hit)
    {
        if (hit.collider.gameObject.CompareTag("PlayerPaddle") || hit.collider.gameObject.CompareTag("OpponentPaddle"))
        {
            Velocity = BallBehaviours.VelocityAfterPaddleCollision(Velocity, hit.point, hit.transform);
            Velocity = BallBehaviours.ApplyAngleLimitToVector(Velocity, settings.maximumBallAngle, hit.transform.up);
            PlayBounceNoise();
        } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
        {
            Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, hit.normal);
            PlayBounceNoise();
        }
    }
}
