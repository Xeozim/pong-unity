using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI playerScore;
    [SerializeField] TextMeshProUGUI opponentScore;

    public void OnPlayerScoreUpdated(int score){
        playerScore.text = $"{score}";
    }
    public void OnOpponentScoreUpdated(int score){
        opponentScore.text = $"{score}";
    }
}
