using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerScore;
    [SerializeField] TextMeshProUGUI playerLives;
    [SerializeField] TextMeshProUGUI opponentScore;

    public void OnPlayerScoreUpdated(int score){
        if (playerScore != null)
        {
            playerScore.text = $"{score}";
        }
    }
    public void OnPlayerLivesUpdated(int lives){
        if (playerLives != null)
        {
            playerLives.text = $"{lives}";
        }
    }
    public void OnOpponentScoreUpdated(int score){
        if (opponentScore != null)
        {
            opponentScore.text = $"{score}";
        }
    }
}
