using Palmmedia.ReportGenerator.Core;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public enum GameStage
{
    GameOver = 0,
    StageOne = 1,
    StageTwo = 2,
    StageThree = 3,
    StageFour = 4,
}

public class BreakoutGameController : MonoBehaviour
{
    // Events
    public UnityEvent<int> PlayerScoreUpdated;
    public UnityEvent<int> PlayerLivesUpdated;
    public UnityEvent<GameStage> GameStageUpdated;

    // Controls for restarting etc.
    private BreakoutControls _controls;
    [SerializeField] private BreakoutSettings _settings;

    // Block settings
    [SerializeField] private Bounds _blockBounds;
    [SerializeField] private Transform _blockParent;
    [SerializeField] private DestructibleScoringBlock _blockPrefab;

    // Game state tracking
    private GameStage _gameStage = GameStage.StageOne;
    private int _blocksHit = 0;
    private int _playerLives = 3;
    private int _playerScore = 0;
    
    private void Awake()
    {
        _controls = new BreakoutControls();
    }

    private void OnEnable()
    {
        // Enable input actions
        _controls.Menu.Enable();

        // Subscribe to the input actions
        _controls.Menu.Restart.performed += RestartGame;
        RestartGame(new InputAction.CallbackContext());
    }

    // Called by the UI to restart the game
    public void RestartGame(InputAction.CallbackContext context){
        ResetBlocks();

        _blocksHit = 0;
        SetPlayerScore(0);
        SetPlayerLives(_settings.playerLives);
        SetGameStage(GameStage.StageOne);
    }

    private void SetGameStage(GameStage stage)
    {
        _gameStage = stage;
        GameStageUpdated.Invoke(stage);
    }

    private void SetPlayerScore(int score)
    {
        _playerScore = score;
        PlayerScoreUpdated.Invoke(score);
    }

    private void SetPlayerLives(int lives)
    {
        _playerLives = lives;
        PlayerLivesUpdated.Invoke(lives);
        if (lives <= 0)
        {
            SetGameStage(GameStage.GameOver);
        }
    }

    // Called by a block when destroyed by the ball
    public void OnScoringBlockDestroyed(float blockValue, GameObject _)
    {
        SetPlayerScore((int) blockValue);

        _blocksHit++;

        if (blockValue >= _settings.blockScoreTopRows)
        {
            SetGameStage(GameStage.StageFour);
        } else if (_gameStage == GameStage.StageOne && _blocksHit >= 4)
        {
            SetGameStage(GameStage.StageTwo);
        } else if (_gameStage == GameStage.StageTwo && _blocksHit >= 12)
        {
            SetGameStage(GameStage.StageThree);
        }
    }
    
    // Called by the ball when entering the trigger below the player
    public void BallOutOfBounds()
    {
        SetPlayerLives(_playerLives - 1);
    }

    public void ResetBlocks()
    {
        // Clear old blocks
        foreach (Transform child in _blockParent)
        {
            DestructibleScoringBlock block = child.GetComponent<DestructibleScoringBlock>();
            block?.scoringBlockDestroyed.RemoveAllListeners();

            Destroy(child.gameObject);
        }

        // Calculate the number of blocks in each dimension
        var xCount = Mathf.CeilToInt(_blockBounds.size.x / _blockPrefab.transform.localScale.x);
        var yCount = Mathf.CeilToInt(_blockBounds.size.y / _blockPrefab.transform.localScale.y);

        // Loop through each block position
        for (int x = 0; x < xCount; x++)
        {
            for (int y = 0; y < yCount; y++)
            {
                // Calculate the new block's position
                var cubePos = _blockBounds.min + new Vector3(x * _blockPrefab.transform.localScale.x, y * _blockPrefab.transform.localScale.y, 0) + (_blockPrefab.transform.localScale / 2); // Offset by half the cube size to center
                cubePos.z = 0;
                
                // Instantiate the block
                var block = Instantiate(_blockPrefab, cubePos, Quaternion.identity);
                block.transform.parent = _blockParent;
                block.scoringBlockDestroyed.AddListener(OnScoringBlockDestroyed);

                switch (y)
                {
                    case 0:
                    case 1:
                        block.Score = _settings.blockScoreBottomRows;
                        break;
                    case 2:
                    case 3:
                        block.Score = _settings.blockScoreMiddleRows;
                        break;
                    case 4:
                    case 5:
                        block.Score = _settings.blockScoreTopRows;
                        break;
                    default:
                        block.Score = _settings.blockScoreBottomRows;
                        break;
                }
            }
        }
    }
}
