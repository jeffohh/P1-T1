using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    private int team1ScoreText;
    private int team2ScoreText;

    public TMP_Text team1Score;
    public TMP_Text team2Score;

    public void IncrementScore(int teamNumber)
    {
        if (teamNumber == 1)
        {
            team1ScoreText++;
            team1Score.text = team1ScoreText.ToString();
        }
        else if (teamNumber == 2)
        {
            team2ScoreText++;
            team2Score.text = team2ScoreText.ToString();
        }
    }
}
