using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PongBall : MonoBehaviour
{
    [SerializeField] private PongSettings settings;

    public UnityEvent ballEnteredPlayerGoal;
    public UnityEvent ballEnteredOpponentGoal;

    private AudioSource audioSource;
    [SerializeField] private AudioClip bounceClip;

    public Vector3 Velocity {get; private set;}

    new MeshRenderer renderer;
    private bool waitingToReset = false;
    private bool gameOver = false;

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

    void FixedUpdate(){
        if (waitingToReset) { return; }

        transform.position += Velocity * Time.fixedDeltaTime;
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

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("Ball collision!");
        // average normal in case multiple surfaces hit simultaneously
        Vector3 collisionNormal = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            collisionNormal += collision.GetContact(i).normal;
        }
        collisionNormal /= collision.contactCount;

        var newVelocity = Vector3.Reflect(Velocity, collisionNormal);

        // We ignore paddle collisions if the axis of the collision (collision normal) has a
        // significant vertical component (i.e. the ball hit the top or bottom of a paddle).
        if (collision.gameObject.CompareTag("PlayerPaddle") || collision.gameObject.CompareTag("OpponentPaddle"))
        {
            if (Mathf.Abs(collisionNormal.y) > 0.5) { return; }

            // For paddle collisions, the angle of reflection increases the
            // further from the centre of the paddle the impact is. We increase
            // the y velocity when the impact is high on the paddle, and
            // decrease it when it's low.
            // Debug.Log("====================================");
            // Debug.Log("Ball-paddle collision.");
            // Debug.Log($"collisionNormal: {collisionNormal}");
            // Debug.Log($"newVelocity: {newVelocity}");
            var impactHeightRatio = (transform.position.y - collision.transform.position.y) / (settings.paddleHeight * 0.25f);
            // Debug.Log($"impactHeightRatio: {impactHeightRatio}");
            var velY = newVelocity.y + (impactHeightRatio * 0.5f * Mathf.Abs(newVelocity.y));
            // Debug.Log($"velY: {velY}");

            // Limit the angle of the velocity by increasing x velocity
            var signX = Mathf.Sign(newVelocity.x);
            // Debug.Log($"signX: {signX}");
            var vXMin = velY / Mathf.Tan(settings.maximumBallAngle);
            // Debug.Log($"vXMin: {vXMin}");
            var velX = Mathf.Min(Mathf.Abs(newVelocity.x),Mathf.Abs(vXMin)) * signX;
            // Debug.Log($"velX: {velX}");

            newVelocity = new Vector3(velX, velY, 0);
        }

        // Limit the speed
        if (newVelocity.magnitude > settings.maximumBallSpeed) { 
            newVelocity = newVelocity.normalized * settings.maximumBallSpeed;
        }

        // Apply the calculated velocity
        Velocity = newVelocity;

        // Play a noise if the game isn't over
        if (!gameOver) { audioSource.PlayOneShot(bounceClip); }
    }

    private void OnTriggerEnter(Collider other)
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

}
