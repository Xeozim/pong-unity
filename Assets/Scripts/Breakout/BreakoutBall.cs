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

        float collisionCheckDistance = Velocity.magnitude * Time.fixedDeltaTime * 1.5f;

        // Handle collisions that would occur if we travel along out current velocity vector
        var distanceChecked = 0.0f;
        var hitNoisePlayed = false;

        while (distanceChecked < collisionCheckDistance){
            var rayDistance = collisionCheckDistance - distanceChecked;
            Vector3 rayDirection = Velocity.normalized;
            if (Physics.Raycast(transform.position, rayDirection, out var hit, rayDistance, collisionLayers, QueryTriggerInteraction.Collide))
            {
                // Handle collisions
                // Debug.Log($"Ball raycast collision detected with {hit.collider.name} ({hit.collider.tag})");

                if (hit.collider.gameObject.CompareTag("PlayerPaddle"))
                {
                    PaddleCollision(hit.point, hit.transform, hit.normal);
                    // Play a noise if the game isn't over and we haven't already done so
                    if (!hitNoisePlayed) {
                        BounceNoise();
                        hitNoisePlayed = true;
                    }
                } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
                {
                    WallCollision(hit.normal);
                    // Play a noise if the game isn't over and we haven't already done so
                    if (!hitNoisePlayed) {
                        BounceNoise();
                        hitNoisePlayed = true;
                    }
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

    private void WallCollision(Vector3 collisionNormal){
        // Debug.Log($"=====================");
        // Debug.Log($"Ball wall collision");
        // Debug.Log($"Velocity (in): {Velocity}");
        // Debug.Log($"collisionNormal: {collisionNormal}");

        Velocity = Vector3.Reflect(Velocity, collisionNormal);
        // Debug.Log($"Velocity (out): {Velocity}");
    }

    // Apply angular limitation to the velocity to be within the angular limit
    // relative to the normal vector of the paddle
    void VelocityAngleLimit(Vector3 paddleNormal)
    {
        float dotProduct = Vector3.Dot(Velocity.normalized, paddleNormal.normalized);
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        if (angle > settings.maximumBallAngle)
        {
            Debug.Log($"Ball velocity angle ({angle}) greater than limit {settings.maximumBallAngle}");
            Vector3 rotationAxis = Vector3.Cross(Velocity, paddleNormal);
            float rotationAngle = angle - settings.maximumBallAngle;
            Quaternion rotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
            Velocity = rotation * Velocity;
        }
    }

    private void PaddleCollision(Vector3 collisionPosition, Transform paddle, Vector3 collisionNormal)
    {
        Debug.Log($"=====================");
        Debug.Log($"Ball paddle collision");
        /* We model collisions with the paddle as if the ball had hit an object
        with the shape of the function cos(0.25*pi*x) where x is the relative
        distance to the centre of the paddle (x = 1 at the right edge, -1 at the
        left). This gives the effect of the paddle being flat in the centre and
        curved towards the edges.

        Given the base function defining the "shape" of the paddle
        - f(x) = cos(2*pi*x)
        - The derivative f'(x) = -sin(2*pi*x)
        - Tangent vector is given by (1,f'(x)) = (1,-sin(2*pi*x))
        - Normal vector is given by (-f'(x),1) = (sin(2*pi*x),1)
        [NB: choosing the normal with a positive y value]

        So if the ball impacts in the centre of the paddle:
         - x = 0
         - f(x) = 1
         - f'(x) = 0
         - Tangent vector = (1,0)
         - Normal vector = (0,1)
         - Thus the ball bounces as if it has hit a flat paddle
         
        But if the ball impacts half way along the paddle:
         - x = 0.5
         - f(x) = 0.707
         - f'(x) = -0.707
         - Tangent vector (1,-0.707)
         - Normal vector (0.707, 1)
         - Thus the ball bounces off to the right
        */ 
        // Debug.Log($"Velocity (in): {Velocity}");
        Debug.Log($"collisionPosition: {collisionPosition}");
        Debug.Log($"Paddle Position: {paddle.position}");
        Debug.Log($"collisionNormal: {collisionNormal}");

        // Project the collision position onto the plane defined by the paddle
        // orientation, and get the length of the projected vector.
        var vectorToContact = collisionPosition - paddle.position;
        Debug.Log($"vectorToContact: {vectorToContact}");
        var projectedPoint = Vector3.ProjectOnPlane(vectorToContact, paddle.up);
        Debug.Log($"projectedPoint: {projectedPoint}");
        // var centerToProjected = projectedPoint - planeCenter;
        // Debug.Log($"centerToProjected: {centerToProjected}");
        var projectedDistance = projectedPoint.magnitude; 
        Debug.Log($"projectedDistance: {projectedDistance}");

        // If the vector from the paddle centre to the collision points to the
        // right, the collision is on the right of the paddle and the dot
        // product result will be > 0. If it isn't we want to negate the distance
        Debug.Log($"paddle.right: {paddle.right}");
        if (Vector3.Dot(vectorToContact, paddle.right) < 0)
        {
            projectedDistance = -projectedDistance;
        }
        Debug.Log($"Distance after x axis correction: {projectedDistance}");
        // If the collision normal points in the same direction as the paddle's
        // up vector, then the dot product will be > 0. If it isn't we want to
        // negate the distance.
        if (Vector3.Dot(paddle.up,collisionNormal) < 0)
        {
            projectedDistance = -projectedDistance;
        }
        Debug.Log($"Distance after rotation correction: {projectedDistance}");

        // We measure relative transform positions as impact offset ratio (IOR)
        // where IOR = 1 indicates the ball colliding with the extremity of the
        // paddle, IOR = 0 the centre, and IOR = -1 the opposite extremity.

        // var impactOffsetRatio = Mathf.Clamp((transform.position.x - collisionPosition.x) / (settings.paddleWidth * 0.5f),-1,1);
        // Debug.Log($"impactOffsetRatio: {impactOffsetRatio}");

        // Now we can calculate the collision normal to use based on the IOR
        var fakeNormalLocal = new Vector3(Mathf.Sin(0.25f*Mathf.PI*projectedDistance),1,0);
        // var fakeNormal = new Vector3(Mathf.Sin(impactOffsetRatio),1,0);
        fakeNormalLocal.Normalize();
        // Debug.Log($"fakeNormal: {fakeNormal}");

        // Fake normal is calculated as if the global up axis is the default,
        // but what we should be doing is calculating it relative to the paddle
        // orientation. To do this we calculate the rotation quaternion that 
        // transforms from world space to paddle space, then apply that
        // transformation to the normal.
        var rotation = Quaternion.FromToRotation(Vector3.up, paddle.up);
        var fakeNormalWorld = rotation * fakeNormalLocal;

        var newVelocity = Vector3.Reflect(Velocity,fakeNormalWorld);

        // Limit angle to within the settings limit around the 

        // Apply the calculated velocity
        Velocity = newVelocity;
        // Debug.Log($"Velocity (pre-angle-limit): {Velocity}");

        VelocityAngleLimit(paddleNormal: paddle.up);

        // Debug.Log($"Velocity (out): {Velocity}");
    }

    private void TriggerCollision(Collider other)
    {
    }

    public void OnTimeScaleUpdated(float timeScale) {
        audioSource.enabled = timeScale < 2.0f;
    }
}
