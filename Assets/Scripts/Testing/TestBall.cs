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

    void FixedUpdate(){
        float collisionCheckDistance = Velocity.magnitude * Time.fixedDeltaTime * 1.5f;

        // Handle collisions that would occur if we travel along out current velocity vector
        var distanceChecked = 0.0f;

        while (distanceChecked < collisionCheckDistance){
            var rayDistance = collisionCheckDistance - distanceChecked;
            Vector3 rayDirection = Velocity.normalized;
            if (Physics.Raycast(transform.position, rayDirection, out var hit, rayDistance, collisionLayers, QueryTriggerInteraction.Collide))
            {
                // Handle collisions
                // Debug.Log($"Ball raycast collision detected with {hit.collider.name} ({hit.collider.tag})");

                if (hit.collider.gameObject.CompareTag("PlayerPaddle") || hit.collider.gameObject.CompareTag("OpponentPaddle"))
                {
                    PaddleCollision(hit.point, hit.transform);
                } else if (hit.collider.gameObject.CompareTag("BarrierHorizontal") || hit.collider.gameObject.CompareTag("BarrierVertical"))
                {
                    WallCollision(hit.normal);
                } else if (hit.collider.isTrigger){
                    TriggerCollision(hit.collider);
                }

                distanceChecked += hit.distance;
            } else
            {
                distanceChecked += rayDistance;
            }
        }

        // Set the speed
        Velocity = Velocity.normalized * ballSpeed;

        transform.position += Velocity * Time.fixedDeltaTime;
    }

    private void WallCollision(Vector3 collisionNormal){
        Velocity = Vector3.Reflect(Velocity, collisionNormal);
    }

    // Apply angular limitation to the velocity to be within the angular limit
    // relative to the normal vector of the paddle
    void VelocityAngleLimit(Vector3 paddleNormal)
    {
        float dotProduct = Vector3.Dot(Velocity.normalized, paddleNormal.normalized);
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        if (angle > maximumBallAngle)
        {
            Debug.Log($"Ball velocity angle ({angle}) greater than limit {maximumBallAngle}");
            Vector3 rotationAxis = Vector3.Cross(Velocity, paddleNormal);
            float rotationAngle = angle - maximumBallAngle;
            Quaternion rotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
            Velocity = rotation * Velocity;
        }
    }

    private void PaddleCollision(Vector3 collisionPosition, Transform paddle)
    {
        // Get the collision position in the reference frame of the paddle
        // Can't use InverseTransformPoint here because we want to ignore scale
        var localCollisionPosition = paddle.transform.InverseTransformPoint(collisionPosition);
        var collisionDistance = localCollisionPosition.x;

        // Calculate the collision normal to use based on the the distance from the collision to
        // the centre of the paddle.
        /* Explanation:
        We model collisions with the paddle as if the ball had hit an object 
        with the shape of the function cos(0.5*pi*x) where x is the relative
        distance to the centre of the paddle (x = 0.5 at the right edge, -0.5 at the
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
         
        But if the ball impacts at the right edge of the paddle:
         - x = 0.5
         - f(x) = 0.707
         - f'(x) = -0.707
         - Tangent vector (1,-0.707)
         - Normal vector (0.707, 1)
         - Thus the ball bounces off to the right
        */ 
        var fakeNormalLocal = new Vector3(Mathf.Sin(0.5f*Mathf.PI*collisionDistance),1,0);
        fakeNormalLocal.Normalize();

        // The maths behind the fake normal generates a normal vector relative to the paddle, so
        // we need to convert this into global space before using it for the reflection.
        // Normals are usually relative to the position of the collision, so all we need to do is
        // adjust for rotation.
        var fakeNormalWorld = Quaternion.AngleAxis(paddle.transform.eulerAngles.z, Vector3.forward) * fakeNormalLocal;

        // Apply the new velocity reflected by the fake normal
        Velocity = Vector3.Reflect(Velocity,fakeNormalWorld);;

        // Limit the angle of the bounce relative to the paddle
        VelocityAngleLimit(paddleNormal: paddle.up);
    }

    private void TriggerCollision(Collider other)
    {
    }
}
