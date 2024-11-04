using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PongGameController : MonoBehaviour
{
    // Scores
    public int PlayerScore { get; private set; }
    public int OpponentScore { get; private set; }

    public UnityEvent<int> playerScoreUpdated;
    public UnityEvent<int> opponentScoredUpdated;

    // Event which alerts other things to the state of the game.
    // True means the game is over / waiting for a new one to start.
    public UnityEvent<bool> gameOverStateUpdated;
    private bool gameOver = false;

    // Controls for restarting etc.
    private PongControls controls;
    [SerializeField] private PongSettings settings;
    
    private AudioSource audioSource;
    [SerializeField] private AudioClip goalScoredClip;

    private void Awake()
    {
        controls = new PongControls();
        audioSource = GetComponent<AudioSource>();
    }

    private void OnEnable()
    {
        // Enable input actions
        controls.Menu.Enable();

        // Subscribe to the input actions
        controls.Menu.Restart.performed += RestartGame;
    }

    // Called by the UI to restart the game
    public void RestartGame(InputAction.CallbackContext context){
        // Ignore if the game isn't over
        if (!gameOver) { return; }
        
        PlayerScore = OpponentScore = 0;
        playerScoreUpdated.Invoke(PlayerScore);
        opponentScoredUpdated.Invoke(OpponentScore);

        gameOver = false;
        gameOverStateUpdated.Invoke(gameOver);
    }

    // Called when any goal is scored to check for end game
    public void CheckEndgame()
    {
        // If either player has 11+ points, change the game over state to true
        // and send an event for other gameobjects to respond to
        if (PlayerScore >= settings.winningScore || OpponentScore >= settings.winningScore){
            gameOver = true;
            gameOverStateUpdated.Invoke(gameOver);
        }
    }

    void ScoreNoise(){
        if (audioSource.enabled) { audioSource.PlayOneShot(goalScoredClip); }
    }

    // Called by the ball when entering the player goal
    public void PlayerScored()
    {
        PlayerScore++;
        playerScoreUpdated.Invoke(PlayerScore);
        ScoreNoise();
        // Debug.Log($"Player Score: {PlayerScore}");
        CheckEndgame();
    }
    
    // Called by the ball when entering the opponent goal
    public void OpponentScored()
    {
        OpponentScore++;
        opponentScoredUpdated.Invoke(OpponentScore);
        ScoreNoise();
        // Debug.Log($"Opponent Score: {OpponentScore}");
        CheckEndgame();
    }

    public void OnTimeScaleUpdated(float timeScale) {
        audioSource.enabled = timeScale < 2.0f;
    }
}
