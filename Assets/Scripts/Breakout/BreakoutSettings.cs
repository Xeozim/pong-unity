using UnityEngine;

[CreateAssetMenu(fileName = "NewBreakoutSettings", menuName = "ScriptableObjects/BreakoutSettings")]
public class BreakoutSettings : ScriptableObject
{
    public float ballSpeedStage1;
    public float ballSpeedStage2;
    public float ballSpeedStage3;
    public float ballSpeedStage4;
    public int blockScoreBottomRows = 1;
    public int blockScoreMiddleRows = 4;
    public int blockScoreTopRows = 7;
    public int playerLives = 3;
    public float maximumBallAngle;
    public float paddleWidth;
    public float paddleHeight;
    public float paddleSpeed;
    public float xMinimum;
    public float xMaxmium;
    public float resetWait;
}