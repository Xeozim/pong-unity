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
        if (waitingToReset) { return; }

        transform.position += Velocity * Time.deltaTime;
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

        // Initialise moving at 45 degrees either up or down
        // Assumes the player is on the left and fires towards the player that lost the last point
        var velRotation = Quaternion.Euler(0, 0, 45 * Mathf.Sign(Random.value - 0.5f));
        Velocity = velRotation * new Vector3((playerLost ? -1 : 1) * settings.initialBallSpeed,0);

        if (!skipWait) { StartCoroutine(ResetWait(settings.resetWait)); }
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Debug.Log("Ball collision!");
        // Bounce off in the opposite direction of the collision (average normal if two surfaces hit
        // simultaneously) with perfect energy conservation and play a noise
        Vector3 collisionNormal = Vector3.zero;
        for (int i = 0; i < collision.contactCount; i++)
        {
            collisionNormal += collision.GetContact(0).normal;
        }
        collisionNormal /= collision.contactCount;
        Velocity = Vector3.Reflect(Velocity, collisionNormal);
        audioSource.PlayOneShot(bounceClip);
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log("Ball Trigger!");
        if (other.gameObject.CompareTag("PlayerGoal"))
        {
            // Debug.Log("Ball entered the PlayerGoal trigger zone!");
            ballEnteredPlayerGoal.Invoke();
            ResetBall(true);
        } else if (other.gameObject.CompareTag("OpponentGoal"))
        {
            // Debug.Log("Ball entered the OpponentGoal trigger zone!");
            ballEnteredOpponentGoal.Invoke();
            ResetBall(false);
        }
    }

}
