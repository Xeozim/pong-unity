using UnityEngine;
using UnityEngine.Events;

public class PongBall : MonoBehaviour
{
    [SerializeField] private PongSettings settings;
    new Rigidbody rigidbody;

    public UnityEvent ballEnteredPlayerGoal;
    public UnityEvent ballEnteredOpponentGoal;

    private AudioSource audioSource;
    [SerializeField] private AudioClip bounceClip;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    // Start is called before the first frame update
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        ResetBall(true);
    }

    public void ResetBall(bool playerLost)
    {
        // Initialise in a random position on the y axis
        var yInitial = Random.Range(settings.yMinimum + transform.localScale.y, settings.yMaxmium - transform.localScale.y);
        transform.position = new Vector3(0,yInitial,0);

        // Initialise moving at 45 degrees either up or down
        // Assumes the player is on the left and fires towards the player that lost the last point
        var velRotation = Quaternion.Euler(0, 0, 45 * Mathf.Sign(Random.value - 0.5f));
        rigidbody.velocity = velRotation * new Vector3((playerLost ? -1 : 1) * settings.initialBallSpeed,0);
    }

    private void OnCollisionEnter(Collision collision)
    {
        audioSource.PlayOneShot(bounceClip);
    }

    private void OnTriggerEnter(Collider other)
    {
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
