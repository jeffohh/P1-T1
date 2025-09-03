using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Pages / UI Roots (per scene)")]
    public GameObject startPage; 
    public GameObject ingameUI;    
    public GameObject stopPage;   

    [Header("Start Page (MainScene)")]
    public Button startButton;

    [Header("In-Game UI (GameScene)")]
    public Button pauseImageButton;

    [Header("Stop Page (GameScene)")]
    public Button continueButton;
    public Button playImageButton;
    public Button homeButton;

    [Header("Scenes")]
    [Tooltip("MainScene")]
    public string mainMenuSceneName = "MainScene";
    [Tooltip(" GameScene")]
    public string gameplaySceneName = "GameScene";

    [Header("Score")]
    private int team1ScoreValue;
    private int team2ScoreValue;
    public TMP_Text team1Score;
    public TMP_Text team2Score;

    public bool IsPaused { get; private set; } = true;

    void Awake()
    {
        // button
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

        SetupForActiveScene();

        UpdateScoreUI();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetupForActiveScene();
        UpdateScoreUI();
    }

    private void SetupForActiveScene()
    {
        string active = SceneManager.GetActiveScene().name;

        bool inMainMenu = !string.IsNullOrEmpty(mainMenuSceneName) && active == mainMenuSceneName;
        bool inGameplay = !string.IsNullOrEmpty(gameplaySceneName) && active == gameplaySceneName;

        if (inMainMenu)
        {
            SafeSetActive(startPage, true);
            SafeSetActive(ingameUI, false);
            SafeSetActive(stopPage, false);
            SetPaused(true);
        }
        else if (inGameplay)
        {
            SafeSetActive(startPage, false);
            SafeSetActive(ingameUI, true);
            SafeSetActive(stopPage, false);
            SetPaused(false);
        }
        else
        {
            SafeSetActive(startPage, false);
            SafeSetActive(ingameUI, false);
            SafeSetActive(stopPage, false);
            SetPaused(false);
        }
    }

    private void SafeSetActive(GameObject go, bool on)
    {
        if (go != null) go.SetActive(on);
    }

    // ===== Score =====
    public void IncrementScore(int teamNumber)
    {
        if (teamNumber == 1) team1ScoreValue++;
        else if (teamNumber == 2) team2ScoreValue++;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (team1Score != null) team1Score.text = team1ScoreValue.ToString();
        if (team2Score != null) team2Score.text = team2ScoreValue.ToString();
    }

    // ===== State Switch =====
    public void StartGame()
    {
        if (!string.IsNullOrEmpty(gameplaySceneName))
        {
            Time.timeScale = 1f;
            AudioListener.pause = false;
            SceneManager.LoadScene(gameplaySceneName);
        }
        else
        {
            SafeSetActive(startPage, false);
            SafeSetActive(stopPage, false);
            SafeSetActive(ingameUI, true);
            SetPaused(false);
        }
    }

    public void PauseGame()
    {
        SafeSetActive(stopPage, true);
        SafeSetActive(ingameUI, false);
        SetPaused(true);
    }

    public void ResumeGame()
    {
        SafeSetActive(stopPage, false);
        SafeSetActive(ingameUI, true);
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
            SafeSetActive(startPage, true);
            SafeSetActive(ingameUI, false);
            SafeSetActive(stopPage, false);
            SetPaused(true);
        }
    }

    // ===== tools =====
    private void SetPaused(bool paused)
    {
        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;
        AudioListener.pause = paused;
    }
}
