using System.Collections;
using UnityEngine;

public abstract class Ball : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers;

    private AudioSource audioSource;
    [SerializeField] private AudioClip bounceClip;

    public Vector3 Velocity {get; protected set;}

    new private MeshRenderer renderer;
    private bool waitingToReset = false;
    private bool hitNoisePlayed = false;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        renderer = GetComponent<MeshRenderer>();
        BallAwake();
    }

    protected virtual void BallAwake(){}

    // Start is called before the first frame update
    void Start()
    {
        ResetBall(true, true);
        BallStart();
    }

    protected virtual void BallStart(){}

    void Update(){
        BallPreUpdate();
        hitNoisePlayed = false;
        BallPostUpdate();
    }

    protected virtual void BallPreUpdate(){}
    protected virtual void BallPostUpdate(){}

    public abstract void ResetBall(bool playerLost, bool skipWait = false);

    protected virtual void PreFixedUpdate(){}
    protected virtual void PostFixedUpdate(){}

    void FixedUpdate(){
        PreFixedUpdate();

        if (waitingToReset) { return; }

        // Handle collisions with a raycast check
        float collisionCheckDistance = Velocity.magnitude * Time.fixedDeltaTime * 1.5f;
        var distanceChecked = 0.0f;

        while (distanceChecked < collisionCheckDistance){
            var rayDistance = collisionCheckDistance - distanceChecked;
            var rayDirection = Velocity.normalized;
            if (Physics.Raycast(transform.position, rayDirection, out var hit, rayDistance, collisionLayers, QueryTriggerInteraction.Collide))
            {
                if (hit.collider.isTrigger){
                    TriggerHit(hit);
                } else {
                    ColliderHit(hit);
                }
                distanceChecked += hit.distance;
            } else
            {    
                distanceChecked += rayDistance;
            }
        }
        transform.position += Velocity * Time.fixedDeltaTime;

        PostFixedUpdate();
    }

    protected virtual void ColliderHit(RaycastHit hit){}
    protected virtual void TriggerHit(RaycastHit hit){}

    public void OnTimeScaleUpdated(float timeScale) {
        audioSource.enabled = timeScale < 2.0f;
    }

    // Coroutine that disables the GameObject, waits for a period, and then re-enables it
    protected IEnumerator ResetWait(float seconds)
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

    // Play a noise if the game isn't over and we haven't already done so since the last update
    protected void PlayBounceNoise(){
        if (audioSource != null && audioSource.enabled && !hitNoisePlayed)
        {
            audioSource.PlayOneShot(bounceClip);
            hitNoisePlayed = true;
        }
    }
}
