using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BreakoutBall : MonoBehaviour
{
    [SerializeField] private BreakoutSettings settings;
    [SerializeField] private LayerMask collisionLayers;

    private AudioSource audioSource;
    [SerializeField] private AudioClip bounceClip;

    public Vector3 Velocity {get; private set;}

    new private MeshRenderer renderer;
    private bool waitingToReset = false;

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
        // Initialise in a random position on the x axis
        var xInitial = Random.Range(settings.xMinimum + transform.localScale.y, settings.xMaxmium - transform.localScale.x);
        transform.position = new Vector3(xInitial,0,0);

        // Initialise moving at 60 degrees either up or down
        // Assumes the player is on the left and fires towards the player that lost the last point
        var velRotation = Quaternion.Euler(0, 0, settings.maximumBallAngle * Mathf.Sign(Random.value - 0.5f));
        Velocity = velRotation * new Vector3((playerLost ? -1 : 1) * settings.ballSpeedStage1,0);

        if (!skipWait) { StartCoroutine(ResetWait(settings.resetWait)); }
    }

    void BounceNoise(){
        if (audioSource.enabled) { audioSource.PlayOneShot(bounceClip); }
    }

    void FixedUpdate(){
        if (waitingToReset) { return; }

        // Handle collisions with a raycast check
        float collisionCheckDistance = Velocity.magnitude * Time.fixedDeltaTime * 1.5f;
        var distanceChecked = 0.0f;
        var hitNoisePlayed = false;

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
                    if (!hitNoisePlayed) {
                        BounceNoise();
                        hitNoisePlayed = true;
                    }
                } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
                {
                    Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, hit.normal);
                    // Play a noise if the game isn't over and we haven't already done so
                    if (!hitNoisePlayed) {
                        BounceNoise();
                        hitNoisePlayed = true;
                    }
                }
                distanceChecked += hit.distance;
            } else
            {    
                distanceChecked += rayDistance;
            }
        }
        
        // Set the speed
        // TODO: adapt for game progress
        Velocity = Velocity.normalized * settings.ballSpeedStage1;
        
        // Update the position
        transform.position += Velocity * Time.fixedDeltaTime;
    }

    public void OnTimeScaleUpdated(float timeScale) {
        audioSource.enabled = timeScale < 2.0f;
    }
}
