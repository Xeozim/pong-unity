using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BallBehaviours
{
    public static Vector3 VelocityAfterWallCollision(Vector3 velocityIn, Vector3 collisionNormal)
    {
        return Vector3.Reflect(velocityIn, collisionNormal);
    }

    // Apply angular limitation to the velocity to be within the angular limit
    // relative to the normal vector of the paddle
    public static Vector3 ApplyAngleLimitToVector(Vector3 velocityIn, float maximumAngle, Vector3 relativeTo)
    {
        float dotProduct = Vector3.Dot(velocityIn.normalized, relativeTo.normalized);
        float angle = Mathf.Acos(dotProduct) * Mathf.Rad2Deg;

        if (angle > maximumAngle)
        {
            Vector3 rotationAxis = Vector3.Cross(velocityIn, relativeTo);
            float rotationAngle = angle - maximumAngle;
            Quaternion rotation = Quaternion.AngleAxis(rotationAngle, rotationAxis);
            return rotation * velocityIn;
        } else { return velocityIn; }
    }

    public static Vector3 VelocityAfterPaddleCollision(Vector3 velocityIn, Vector3 collisionPosition, Transform paddleTransform)
    {
        // Get the collision position in the reference frame of the paddle
        // Can't use InverseTransformPoint here because we want to ignore scale
        var localCollisionPosition = paddleTransform.transform.InverseTransformPoint(collisionPosition);
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
        var fakeNormalWorld = Quaternion.AngleAxis(paddleTransform.transform.eulerAngles.z, Vector3.forward) * fakeNormalLocal;

        // Apply the new velocity reflected by the fake normal
        return Vector3.Reflect(velocityIn,fakeNormalWorld);;
    }

    // Limit a velocity vector using an absolute speed limit
    public static Vector3 VelocityAfterSpeedLimit(Vector3 velocityIn, float speedLimit)
    {
        if (velocityIn.magnitude > speedLimit) { 
            return velocityIn.normalized * speedLimit;
        } else
        {
            return velocityIn;
        }
    }
}
