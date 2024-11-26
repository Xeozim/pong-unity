using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PongBall : MonoBehaviour
{
    [SerializeField] private PongSettings settings;

    public UnityEvent ballEnteredPlayerGoal;
    public UnityEvent ballEnteredOpponentGoal;

    [SerializeField] private LayerMask collisionLayers;

    private AudioSource audioSource;
    [SerializeField] private AudioClip bounceClip;

    public Vector3 Velocity {get; private set;}

    new private MeshRenderer renderer;
    private bool waitingToReset = false;
    private bool gameOver = false;
    private bool hitNoisePlayed = false;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        renderer = GetComponent<MeshRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        ResetBall(true, true);
    }

    void Update(){
        hitNoisePlayed = false;
    }

    public void OnGameOverStateUpdated(bool isGameOver)
    {
        gameOver = isGameOver;
        if (!isGameOver) {
            // Game was restarted, reset the ball
            ResetBall(true);
        }
    }

    // Coroutine that disables the GameObject, waits for a period, and then re-enables it
    private IEnumerator ResetWait(float seconds)
    {
        // Disable the GameObject visuals and set the wait flag
        renderer.enabled = false;
        waitingToReset = true;

        // Wait for the specified duration
        yield return new WaitForSeconds(seconds);

        // Re-enable
        renderer.enabled = true;
        waitingToReset = false;
    }

    public void ResetBall(bool playerLost, bool skipWait = false)
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

    // Play a noise if the game isn't over and we haven't already done so since the last update
    void BounceNoise(){
        if (audioSource.enabled && !hitNoisePlayed)
        {
            audioSource.PlayOneShot(bounceClip);
            hitNoisePlayed = true;
        }
    }

    void FixedUpdate(){
        if (waitingToReset) { return; }

        // Handle collisions with a raycast check
        float collisionCheckDistance = Velocity.magnitude * Time.fixedDeltaTime * 1.5f;
        var distanceChecked = 0.0f;

        while (distanceChecked < collisionCheckDistance){
            var rayDistance = collisionCheckDistance - distanceChecked;
            var rayDirection = Velocity.normalized;
            if (Physics.Raycast(transform.position, rayDirection, out var hit, rayDistance, collisionLayers, QueryTriggerInteraction.Collide))
            {
                // Debug.Log($"Ball raycast collision detected with {hit.collider.name} ({hit.collider.tag})");
                if (hit.collider.gameObject.CompareTag("PlayerPaddle") || hit.collider.gameObject.CompareTag("OpponentPaddle"))
                {
                    Velocity = BallBehaviours.VelocityAfterPaddleCollision(Velocity, hit.point, hit.transform);
                    Velocity = BallBehaviours.ApplyAngleLimitToVector(Velocity, settings.maximumBallAngle, hit.transform.up);
                    Velocity = BallBehaviours.VelocityAfterSpeedLimit(Velocity * (1 + settings.ballSpeedIncreaseOnPaddleHit), settings.maximumBallSpeed);
                    BounceNoise();
                } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
                {
                    Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, hit.normal);
                    BounceNoise();
                } else if (hit.collider.isTrigger){
                    TriggerCollision(hit.collider);
                }
                distanceChecked += hit.distance;
            } else
            {    
                distanceChecked += rayDistance;
            }
        }
        transform.position += Velocity * Time.fixedDeltaTime;
    }

    private void TriggerCollision(Collider other)
    {
        // Debug.Log("Ball Trigger!");
        if (other.gameObject.CompareTag("PlayerGoal"))
        {
            // Debug.Log("Ball entered the PlayerGoal trigger zone!");
            // Ignore goals if game is over
            if (gameOver) {return;}
            ballEnteredPlayerGoal.Invoke();
            ResetBall(true);
        } else if (other.gameObject.CompareTag("OpponentGoal"))
        {
            // Debug.Log("Ball entered the OpponentGoal trigger zone!");
            // Ignore goals if game is over
            if (gameOver) {return;}
            ballEnteredOpponentGoal.Invoke();
            ResetBall(false);
        }
    }

    public void OnTimeScaleUpdated(float timeScale) {
        audioSource.enabled = timeScale < 2.0f;
    }
}
