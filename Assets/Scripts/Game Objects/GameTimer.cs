using UnityEngine;
using TMPro;

public class GameTimer : MonoBehaviour
{
    [Header("Time Set")]
    public float totalTime = 60f;
    private float timer;

    [Header("UI & Canvas")]
    public Canvas resultCanvas;
    public TMP_Text score1Text;        // 场景中实时显示的 Team1 分数
    public TMP_Text score2Text;        // 场景中实时显示的 Team2 分数
    public TMP_Text timerText;         // 倒计时文本
    public TMP_Text resultScoreText;   // 结果页：显示玩家分数
    public TMP_Text resultText;        // 结果页：显示胜负

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

            // ⚡ 从文本里读取分数（假设 score1Text / score2Text 的内容是数字字符串）
            int s1 = 0;
            int s2 = 0;
            if (score1Text != null) int.TryParse(score1Text.text, out s1);
            if (score2Text != null) int.TryParse(score2Text.text, out s2);

            // 在结果界面显示分数
            if (score1Text != null) score1Text.text = s1.ToString();
            if (score2Text != null) score2Text.text = s2.ToString();

            if (resultScoreText != null)
                resultScoreText.text = "Your Score: " + s1;

            if (resultText != null)
                resultText.text = (s1 > s2) ? "Victory" : "Lose";
        }
    }
}
