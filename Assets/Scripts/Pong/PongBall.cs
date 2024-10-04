using UnityEngine;

public class PongBall : MonoBehaviour
{
    [SerializeField] private PongSettings settings;

    private Vector3 velocity;

    // Start is called before the first frame update
    void Start()
    {
        // Initialise in a random position on the y axis
        var yInitial = UnityEngine.Random.Range(settings.yMinimum + transform.localScale.y, settings.yMaxmium - transform.localScale.y);
        transform.position = new Vector3(0,yInitial,0);

        // Initialise moving at 45 degrees either up or down
        var velRotation = Quaternion.Euler(0, 0, 45 * Mathf.Sign(Random.value - 0.5f));
        velocity = velRotation * new Vector3(-settings.initialBallSpeed,0);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.position + (velocity * Time.deltaTime);
    }
}
