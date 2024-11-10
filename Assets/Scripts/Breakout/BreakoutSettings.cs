using UnityEngine;

[CreateAssetMenu(fileName = "NewBreakoutSettings", menuName = "ScriptableObjects/BreakoutSettings")]
public class BreakoutSettings : ScriptableObject
{
    public float ballSpeedStage1;
    public float ballSpeedStage2;
    public float ballSpeedStage3;
    public float ballSpeedStage4;
    public float maximumBallAngle;
    public float maximumBallAngleChange;
    public float paddleWidth;
    public float paddleHeight;
    public float paddleSpeed;
    public float xMinimum;
    public float xMaxmium;
    public float resetWait;
}