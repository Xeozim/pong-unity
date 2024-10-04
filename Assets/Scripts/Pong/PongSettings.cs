using UnityEngine;

[CreateAssetMenu(fileName = "NewPongSettings", menuName = "ScriptableObjects/PongSettings")]
public class PongSettings : ScriptableObject
{
    public float initialBallSpeed;
    public float maximumBallSpeed;
    public float paddleSpeed;
    public float yMinimum;
    public float yMaxmium;
    public int winningScore;
}