using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BreakoutBall : Ball
{
    [SerializeField] private BreakoutSettings _settings;
    public UnityEvent BallEnteredPlayerGoal;
    public UnityEvent BallHitCeiling;

    private float _speed;
    private Vector3 _defaultPosition;
    private bool _blockCollisionsEnabled = true;

    protected override void BallAwake()
    {
        _speed = _settings.ballSpeedStage1;
        _defaultPosition = transform.position;
    }

    public void OnGameStageUpdated(GameStage stage)
    {
        _speed = stage switch
        {
            GameStage.GameOver => 0,
            GameStage.StageOne => _settings.ballSpeedStage1,
            GameStage.StageTwo => _settings.ballSpeedStage2,
            GameStage.StageThree => _settings.ballSpeedStage3,
            GameStage.StageFour => _settings.ballSpeedStage4,
            _ => _settings.ballSpeedStage1,
        };
        if(stage == GameStage.GameOver){
            SetVisualEnabledState(false);
            CancelReset();
        }
        if(stage == GameStage.StageOne){ ResetBall(); }
    }

    public override void ResetBall(bool playerLost = true, bool skipWait = false)
    {
        // Initialise in a random position on the x axis
        var xInitial = Random.Range(_settings.xMinimum + transform.localScale.y, _settings.xMaxmium - transform.localScale.x);
        transform.position = new Vector3(xInitial,_defaultPosition.y,_defaultPosition.z);

        // Initialise moving at maximum angle degrees left or right
        var velRotation = Quaternion.Euler(0, 0, _settings.maximumBallAngle * Mathf.Sign(Random.value - 0.5f));
        Velocity = velRotation * new Vector3(0,-_speed);

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
            _blockCollisionsEnabled = true;
        } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
        {
            Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, hit.normal);
            PlayBounceNoise();
            _blockCollisionsEnabled |= hit.collider.gameObject.CompareTag("BarrierHorizontal");
        } else if (hit.collider.gameObject.CompareTag("DestructibleBlock"))
        {
            // Disable collisions after hitting a block until the next ceiling / paddle hit
            if (!_blockCollisionsEnabled) { return; }
            _blockCollisionsEnabled = false;
            // Collisions with blocks only act in y so pretend the impact was purely along the y axis
            Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, Vector3.down);
            PlayBounceNoise();

            DestructibleScoringBlock block = hit.collider.gameObject.GetComponent<DestructibleScoringBlock>();
            if (block != null) { block.Damage(1, gameObject); }
        }
        Velocity = BallBehaviours.VelocityAfterSpeedLimit(Velocity, _speed, _speed);
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
