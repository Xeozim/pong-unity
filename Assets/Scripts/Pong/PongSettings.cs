using UnityEngine;

[CreateAssetMenu(fileName = "NewPongSettings", menuName = "ScriptableObjects/PongSettings")]
public class PongSettings : ScriptableObject
{
    public float initialBallSpeed;
    public float maximumBallSpeed;
    public float initialBallAngle;
    public float maximumBallAngle;
    public float maximumBallAngleChange;
    public float ballSpeedIncreaseOnPaddleHit;
    public float paddleWidth;
    public float paddleHeight;
    public float paddleSpeed;
    public float yMinimum;
    public float yMaxmium;
    public float resetWait;
    public int winningScore;
}