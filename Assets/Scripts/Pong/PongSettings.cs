using UnityEngine;

[CreateAssetMenu(fileName = "NewPongSettings", menuName = "ScriptableObjects/PongSettings")]
public class PongSettings : ScriptableObject
{
    public float initialBallSpeed;
    public float maximumBallSpeed;
    public float paddleWidth;
    public float paddleHeight;
    public float paddleSpeed;
    public float yMinimum;
    public float yMaxmium;
    public float resetWait;
    public int winningScore;
}