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
                    PaddleCollision(hit.collider.transform,hit.normal);
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
        Velocity = Vector3.Reflect(Velocity, collisionNormal);
    }

    private void PaddleCollision(Transform collisionTransform, Vector3 collisionNormal)
    {
        // For paddle collisions, the angle of reflection increases the further from the centre
        // of the paddle the impact is. We increase the angle when the impact is high on the
        // paddle, and decrease it when it's low.

        /* The base cases are as follows:
        1. The ball hits the centre of the paddle (collision transform [paddle] y position is
        the same as the position of the ball) - the angle of reflection is unchanged, we
        should just use the pure reflection velocity calculated above.
        2. The ball hits the very top of the paddle (transform of the ball is the same as the
        collision transform [paddle] position plus half the paddle height) - the reflection
        is angled up by 45 degrees.
        3. Opposite of 2 but at the bottom of the paddle the reflection is angled down.
        Between the base cases the angle adjustment scales linearly. */

        /* Calculate the incident angle of the ball impact. If travelling to the right (v.x > 0)
        then calculate from the right axis around the back vector. This will get us things in
        the typical reference frame of the angle of incidence i.e. the angles will be in the
        range -90 to 90, where negative angles indicate the ball is moving up into the paddle.

        Because we use the opposite x direction to measure from (left) and axis to measure
        around (forward) when considering impacts where the ball is moving to the left, this
        effectively flips everything around, so we can  

        When calculating the new angle, we make the same assumption about the vector from which
        we measure the angle (e.g. Vector3.right if the ball was moving right) to ensure that
        all impacts behave correctly regardless of moving right / left. But by making the
        opposite assumption about the axis around which we measure the rotation (forward if the
        ball was moving right and is now moving left), we can leave the sign of the the angle
        the same.

        For example, this diagram shows the ball impacting moving left to right (v.x > 0) and
        top to bottom. The angle of incidence is positive, the angle of reflection is also
        positive and because we impact in the center of the paddle it is the same magnitude as
        the angle of incidence.
                            *-_
                                *-_          |
                                 ↑ *-_       |
          Angle of incidence ->  |    *-_    |
                                 |       *-_ | 
          -> Vector3.right -> -> -> -> ->   #|
                                 |       _-* | 
          Angle of reflection -> |    _-*    |
                                 ↓ _-*       |
                                _-*          |
                            _-*
        */

        // between the  relative to the  for both cases so we can simplify the rest of the
        // maths. When we create the new velocity we reverse this reference frame simplification.
        var incidentAngle = Vector3.SignedAngle(Mathf.Sign(Velocity.y) * Vector3.up, Velocity, Mathf.Sign(Velocity.y) * Vector3.back);
        // Debug.Log($"incidentAngle: {incidentAngle}");

        // First set the new angle based on pure reflection
        // var newAngle = 180 - incidentAngle;
        var newAngle = incidentAngle;
        // Debug.Log($"newAngle (after reflection): {newAngle}");

        // We measure relative transform positions as impact offset ratio (IOR) where IOR = 1
        // indicates the ball colliding with the extremity of the paddle, IHR = 0 the centre, and
        // IHR = -1 the opposite extremity.
        var impactOffsetRatio = Mathf.Clamp((transform.position.x - collisionTransform.transform.position.x) / (settings.paddleWidth * 0.5f),-1,1);
        // Debug.Log($"impactOffsetRatio: {impactOffsetRatio}");

        // Change in angle is proportional to IHR. Negative proportion of change to impact
        // height in order to fit the rest of the maths
        var angleChange = impactOffsetRatio * settings.maximumBallAngleChange; // TODO: move to a setting
        // Debug.Log($"angleChange: {angleChange}");

        // Limit the angle change so that the resulting velocity vector is less than
        // maximumBallAngle from the relevant x axis (use right if travelling right [v.x > 0])
        newAngle += angleChange;
        // Debug.Log($"newAngle (after angle change, unclamped): {newAngle}");
        newAngle = Mathf.Clamp(newAngle, -settings.maximumBallAngle, settings.maximumBallAngle);
        // Debug.Log($"newAngle (final): {newAngle}");

        // Calculate new velocity, reversing the reference frame simplification
        // and increasing the ball speed a little
        var newVelocity =
            Quaternion.Euler(new Vector3(0,0,Mathf.Sign(Velocity.y) * newAngle)) *
            (
                Velocity.magnitude * // TODO: Apply current game stage velocity
                - Mathf.Sign(Velocity.y)
                * Vector3.up
            );

        // Apply the calculated velocity
        Velocity = newVelocity;
    }

    private void TriggerCollision(Collider other)
    {
    }

    public void OnTimeScaleUpdated(float timeScale) {
        audioSource.enabled = timeScale < 2.0f;
    }
}
