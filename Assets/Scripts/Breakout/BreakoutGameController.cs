using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class BreakoutGameController : MonoBehaviour
{
    // Scores
    public int PlayerScore { get; private set; }

    public UnityEvent<int> playerScoreUpdated;

    // Event which alerts other things to the state of the game.
    // True means the game is over / waiting for a new one to start.
    public UnityEvent<bool> gameOverStateUpdated;
    private bool gameOver = false;

    // Controls for restarting etc.
    private BreakoutControls controls;
    [SerializeField] private BreakoutSettings settings;

    // Block settings
    [SerializeField] private Bounds blockBounds;
    [SerializeField] private Transform blockParent;
    [SerializeField] private DestructibleScoringBlock blockPrefab;
    
    private void Awake()
    {
        controls = new BreakoutControls();
    }

    private void OnEnable()
    {
        // Enable input actions
        controls.Menu.Enable();

        // Subscribe to the input actions
        controls.Menu.Restart.performed += RestartGame;
        ResetBlocks();
    }

    // Called by the UI to restart the game
    public void RestartGame(InputAction.CallbackContext context){
        // Ignore if the game isn't over
        if (!gameOver) { return; }
        
        PlayerScore = 0;
        playerScoreUpdated.Invoke(PlayerScore);

        gameOver = false;
        gameOverStateUpdated.Invoke(gameOver);

        ResetBlocks();
    }

    // Called when any goal is scored to check for end game
    public void CheckEndgame()
    {
        
    }

    // Called by a block when destroyed by the ball
    public void OnScoringBlockDestroyed(float blockValue, GameObject _)
    {
        PlayerScore += (int) blockValue;
        playerScoreUpdated.Invoke(PlayerScore);
        CheckEndgame();
    }
    
    // Called by the ball when entering the trigger below the player
    public void BallOutOfBounds()
    {
        CheckEndgame();
    }

    public void ResetBlocks()
    {
        // Clear old blocks
        foreach (Transform child in blockParent)
        {
            DestructibleScoringBlock block = child.GetComponent<DestructibleScoringBlock>();
            block?.scoringBlockDestroyed.RemoveAllListeners();

            Destroy(child.gameObject);
        }

        // Calculate the number of blocks in each dimension
        var xCount = Mathf.CeilToInt(blockBounds.size.x / blockPrefab.transform.localScale.x);
        var yCount = Mathf.CeilToInt(blockBounds.size.y / blockPrefab.transform.localScale.y);

        // Loop through each block position
        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                // Calculate the new block's position
                var cubePos = blockBounds.min + new Vector3(x * blockPrefab.transform.localScale.x, y * blockPrefab.transform.localScale.y, 0) + (blockPrefab.transform.localScale / 2); // Offset by half the cube size to center
                cubePos.z = 0;
                
                // Instantiate the block
                var block = Instantiate(blockPrefab, cubePos, Quaternion.identity);
                block.transform.parent = blockParent;
                block.scoringBlockDestroyed.AddListener(OnScoringBlockDestroyed);

                switch (y)
                {
                    case 0:
                    case 1:
                        block.Score = 1;
                        break;
                    case 2:
                    case 3:
                        block.Score = 4;
                        break;
                    case 4:
                    case 5:
                        block.Score = 7;
                        break;
                    default:
                        block.Score = 1;
                        break;
                }
            }
        }
    }
}
