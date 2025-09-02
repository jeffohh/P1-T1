using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class GameManager : MonoBehaviour
{
    [Header("Pages / UI Roots")]
    public GameObject startPage;  
    public GameObject ingameUI;    
    public GameObject stopPage;  

    [Header("Start Page")]
    public Button startButton;   

    [Header("In-Game UI")]
    public Button pauseImageButton; 

    [Header("Stop Page (Pause)")]
    public Button continueButton;   
    public Button playImageButton;   
    public Button homeButton;        

    [Header("Optional: Load Main Menu Scene")]
    public string mainMenuSceneName = "";  

    [Header("Score")]
    private int team1ScoreValue;
    private int team2ScoreValue;
    public TMP_Text team1Score;
    public TMP_Text team2Score;

    public bool IsPaused { get; private set; } = true;

    void Awake()
    {
        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(StartGame);
        }
        if (pauseImageButton != null)
        {
            pauseImageButton.onClick.RemoveAllListeners();
            pauseImageButton.onClick.AddListener(PauseGame);
        }
        if (continueButton != null)
        {
            continueButton.onClick.RemoveAllListeners();
            continueButton.onClick.AddListener(ResumeGame);
        }
        if (playImageButton != null)
        {
            playImageButton.onClick.RemoveAllListeners();
            playImageButton.onClick.AddListener(ResumeGame);
        }
        if (homeButton != null)
        {
            homeButton.onClick.RemoveAllListeners();
            homeButton.onClick.AddListener(ReturnHome);
        }

        ShowStartPage();
        UpdateScoreUI();
    }

    void Update()
    {
    }

    // Score
    public void IncrementScore(int teamNumber)
    {
        if (teamNumber == 1)
        {
            team1ScoreValue++;
        }
        else if (teamNumber == 2)
        {
            team2ScoreValue++;
        }
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (team1Score != null) team1Score.text = team1ScoreValue.ToString();
        if (team2Score != null) team2Score.text = team2ScoreValue.ToString();
    }

    // Status Switch
    public void StartGame()
    {
        if (startPage) startPage.SetActive(false);
        if (stopPage) stopPage.SetActive(false);
        if (ingameUI) ingameUI.SetActive(true);

        SetPaused(false);
    }

    public void PauseGame()
    {
        if (stopPage) stopPage.SetActive(true);
        if (ingameUI) ingameUI.SetActive(false);
        SetPaused(true);
    }

    public void ResumeGame()
    {
        if (stopPage) stopPage.SetActive(false);
        if (ingameUI) ingameUI.SetActive(true);
        SetPaused(false);
    }

    public void ReturnHome()
    {
        if (!string.IsNullOrEmpty(mainMenuSceneName))
        {
            Time.timeScale = 1f;         
            AudioListener.pause = false;
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            ShowStartPage();
        }
    }

    // tools
    private void ShowStartPage()
    {
        if (startPage) startPage.SetActive(true);
        if (ingameUI) ingameUI.SetActive(false);
        if (stopPage) stopPage.SetActive(false);
        SetPaused(true); 
    }

    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;

    }
}
