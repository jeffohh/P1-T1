using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Time Set")]
    public float totalTime = 60f;
    private float timer;

    [Header("UI & Canvas")]
    public Canvas resultCanvas;
    public TMP_Text score1Text; 
    public TMP_Text score2Text;     
    public TMP_Text timerText;       
    public TMP_Text resultScoreText;
    public TMP_Text resultText;     

    [Header("Score")]
    public int score1 = 0;
    public int score2 = 0;

    private bool isEnded = false;

    void Start()
    {
        timer = totalTime;

        if (resultCanvas != null)
            resultCanvas.gameObject.SetActive(false);
    }

    void Update()
    {
        if (isEnded) return;

        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            EndGame();
            return;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(timer / 60f);
            int seconds = Mathf.FloorToInt(timer % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    private void EndGame()
    {
        isEnded = true;
        timer = 0;

        UpdateTimerUI();

        Time.timeScale = 0f;

        if (resultCanvas != null)
        {
            resultCanvas.gameObject.SetActive(true);


            if (score1Text != null) score1Text.text = "Team 1: " + score1;
            if (score2Text != null) score2Text.text = "Team 2: " + score2;


            if (resultScoreText != null)
                resultScoreText.text = "Your Score: " + score1;

            if (resultText != null)
                resultText.text = (score1 > score2) ? "Victory" : "Lose";
        }
    }

    public void SetScores(int s1, int s2)
    {
        score1 = s1;
        score2 = s2;
    }
}
