using UnityEngine;

public class TestBall : MonoBehaviour
{
    [SerializeField] private LayerMask collisionLayers;
    [SerializeField] private float maximumBallAngle = 60.0f;
    [SerializeField] private float ballSpeed = 8.0f;

    public Vector3 Velocity {get; private set;}

    void Start(){
        var velRotation = Quaternion.Euler(0, 0, maximumBallAngle * Mathf.Sign(Random.value - 0.5f));
        Velocity = velRotation * new Vector3(ballSpeed,0);
    }

    void OnCollisionRaycastHit(RaycastHit hit){
        // Debug.Log($"Ball raycast collision detected with {hit.collider.name} ({hit.collider.tag})");
        if (hit.collider.gameObject.CompareTag("PlayerPaddle") || hit.collider.gameObject.CompareTag("OpponentPaddle"))
        {
            Velocity = BallBehaviours.VelocityAfterPaddleCollision(Velocity, hit.point, hit.transform);
            Velocity = BallBehaviours.ApplyAngleLimitToVector(Velocity, maximumBallAngle, hit.transform.up);
        } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
        {
            Velocity = BallBehaviours.VelocityAfterWallCollision(Velocity, hit.normal);
        }
    }

    void FixedUpdate(){
        // Handle collisions with a raycast check
        float collisionCheckDistance = Velocity.magnitude * Time.fixedDeltaTime * 1.5f;
        var distanceChecked = 0.0f;
        while (distanceChecked < collisionCheckDistance){
            var rayDistance = collisionCheckDistance - distanceChecked;
            var rayDirection = Velocity.normalized;
            if (Physics.Raycast(transform.position, rayDirection, out var hit, rayDistance, collisionLayers, QueryTriggerInteraction.Collide))
            {
                OnCollisionRaycastHit(hit);
                distanceChecked += hit.distance;
            } else
            {    
                distanceChecked += rayDistance;
            }
        }
        
        // Set the speed
        Velocity = Velocity.normalized * ballSpeed;
        
        // Update the position
        transform.position += Velocity * Time.fixedDeltaTime;
    }
}
