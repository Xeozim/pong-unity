using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BreakoutBall : Ball
{
    [SerializeField] private BreakoutSettings _settings;
    public UnityEvent BallEnteredPlayerGoal;

    private float _speed;
    private Vector3 _defaultPosition;

    protected override void BallAwake()
    {
        _speed = _settings.ballSpeedStage1;
        _defaultPosition = transform.position;
    }

    public void OnGameStageUpdated(GameStage stage)
    {
        if(stage == GameStage.GameOver){ SetVisualEnabledState(false); }
        if(stage == GameStage.StageOne){ ResetBall(); }
        _speed = stage switch
        {
            GameStage.GameOver => 0,
            GameStage.StageOne => _settings.ballSpeedStage1,
            GameStage.StageTwo => _settings.ballSpeedStage2,
            GameStage.StageThree => _settings.ballSpeedStage3,
            GameStage.StageFour => _settings.ballSpeedStage4,
            _ => _settings.ballSpeedStage1,
        };
    }

    public override void ResetBall(bool playerLost = true, bool skipWait = false)
    {
        // Initialise in a random position on the x axis
        var xInitial = Random.Range(_settings.xMinimum + transform.localScale.y, _settings.xMaxmium - transform.localScale.x);
        transform.position = new Vector3(xInitial,_defaultPosition.y,_defaultPosition.z);

        // Initialise moving at 60 degrees either up or down
        // Assumes the player is on the left and fires towards the player that lost the last point
        var velRotation = Quaternion.Euler(0, 0, _settings.maximumBallAngle * Mathf.Sign(Random.value - 0.5f));
        Velocity = velRotation * new Vector3((playerLost ? -1 : 1) * _speed,0);

        StartCoroutine(ResetWait(skipWait ? 0 : _settings.resetWait));
    }

    protected override void ColliderHit(RaycastHit hit)
    {
        // Debug.Log($"Ball hit Collider ({hit.collider.gameObject}) with tag: {hit.collider.gameObject.tag}");
        if (hit.collider.gameObject.CompareTag("PlayerPaddle"))
        {
            Velocity = BallBehaviours.VelocityAfterPaddleCollision(Velocity, hit.point, hit.transform);
            Velocity = BallBehaviours.ApplyAngleLimitToVector(Velocity, _settings.maximumBallAngle, hit.transform.up);
            PlayBounceNoise();
        } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
        {
            Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, hit.normal);
            PlayBounceNoise();
        } else if (hit.collider.gameObject.CompareTag("DestructibleBlock"))
        {
            DestructibleScoringBlock block = hit.collider.gameObject.GetComponent<DestructibleScoringBlock>();
            Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, hit.normal);
            PlayBounceNoise();
            if (block != null) { block.Damage(1, gameObject); }
        }
    }

    protected override void TriggerHit(RaycastHit hit)
    {
        // Debug.Log($"Ball hit Trigger ({hit.collider.gameObject}) with tag: {hit.collider.gameObject.tag}");
        if (hit.collider.gameObject.CompareTag("PlayerGoal"))
        {
            BallEnteredPlayerGoal.Invoke();
            ResetBall();
        }
    }
}
