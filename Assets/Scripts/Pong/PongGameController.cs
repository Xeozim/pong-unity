using UnityEngine;
using UnityEngine.Events;

public class PongGameController : MonoBehaviour
{
    // Scores
    public int PlayerScore { get; private set; }
    public int OpponentScore { get; private set; }

    public UnityEvent<int> playerScoreUpdated;
    public UnityEvent<int> opponentScoredUpdated;

    // Called when any goal is scored to check for end game
    public void CheckEndgame()
    {
        // TODO
    }

    // Called by the ball when entering the player goal
    public void PlayerScored()
    {
        PlayerScore++;
        playerScoreUpdated.Invoke(PlayerScore);
        // Debug.Log($"Player Score: {PlayerScore}");
        CheckEndgame();
    }
    
    // Called by the ball when entering the opponent goal
    public void OpponentScored()
    {
        OpponentScore++;
        opponentScoredUpdated.Invoke(OpponentScore);
        // Debug.Log($"Opponent Score: {OpponentScore}");
        CheckEndgame();
    }
}
