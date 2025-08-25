using UnityEngine;

public class TrainingManager : MonoBehaviour
{
    [Header("Training Optimization")]
    public bool isTraining = true;
    public float editorTimeScale = 15f;    // For Unity Editor
    public float buildTimeScale = 20f;     // For builds (will be overridden by YAML)
    public int targetFrameRate = -1;

    void Start()
    {
        if (isTraining)
        {
            OptimizeForTraining();
        }
    }

    void OptimizeForTraining()
    {
        // Speed up time (works in Editor, overridden by YAML in builds)
        if (Application.isEditor)
        {
            Time.timeScale = editorTimeScale;
            Debug.Log($"Editor Training Mode: TimeScale set to {editorTimeScale}x");
        }
        else
        {
            Time.timeScale = buildTimeScale;
            Debug.Log($"Build Training Mode: TimeScale set to {buildTimeScale}x");
        }

        // Unlimited framerate
        Application.targetFrameRate = targetFrameRate;

        // Disable VSync
        QualitySettings.vSyncCount = 0;

        // Lowest quality settings
        QualitySettings.SetQualityLevel(0, false);

        // Disable audio
        AudioListener.volume = 0f;

        Debug.Log("Training optimizations applied.");
    }

    void Update()
    {
        // Press T to toggle training speed
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (Time.timeScale == 1f)
            {
                Time.timeScale = Application.isEditor ? editorTimeScale : buildTimeScale;
                Debug.Log($"Training speed ON: {Time.timeScale}x");
            }
            else
            {
                Time.timeScale = 1f;
                Debug.Log("Normal speed: 1x");
            }
        }

        // Show current speed in top-left corner
        if (isTraining)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(10, 10, 200, 20), $"Training Speed: {Time.timeScale:F0}x");
        }
    }

    void OnGUI()
    {
        if (isTraining)
        {
            GUI.color = Color.yellow;
            GUI.Label(new Rect(10, 10, 200, 20), $"Training Speed: {Time.timeScale:F0}x (Press T to toggle)");
        }
    }
}