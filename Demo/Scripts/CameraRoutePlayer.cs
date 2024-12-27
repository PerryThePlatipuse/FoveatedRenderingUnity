using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using UnityEngine.SceneManagement; // For scene loading
using System.Text; // For StringBuilder
using TMPro;


public class CameraRoutePlayer : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI EndText;

    [Header("Route Settings")]
    public string routeFileName = "CameraRouteMountains.json"; // Relative path within Assets
    public float playbackSpeed = 1f;

    [Header("FPS Measurement Settings")]
    public float fpsMeasureInterval = 0.1f; // Interval in seconds to measure FPS

    [Header("UI Settings")]
    public GUIStyle guiStyle; // Style for the on-screen message

    private CameraRoute route;
    private int currentKeyframe = 0;
    private float lerpDuration = 0.2f; // Should match the recording interval

    // FPS Measurement Variables
    private float fpsTimer = 0f;
    private int frameCount = 0;
    private List<Measurement> measurements = new List<Measurement>();
    private bool playbackFinished = false;
    private string measurementsFilePath = "measurements.csv";
    private bool showExitMessage = false;

    private void Start()
    {
        LoadRoute();
        StartPlayback();
    }

    private void LoadRoute()
    {
        // If routeFileName is an absolute path, use it directly; otherwise, combine with Assets path
        string path = Path.IsPathRooted(routeFileName) ? routeFileName : Path.Combine(Application.dataPath, routeFileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            route = JsonUtility.FromJson<CameraRoute>(json);
            Debug.Log($"Route loaded from {path}");
        }
        else
        {
            Debug.LogError($"Route file not found at {path}");
        }
    }

    private void Update()
    {
        if (!playbackFinished)
        {
            MeasureFPS();
        }
        else if (showExitMessage)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                LoadSampleScene();
            }
        }
    }

    private void MeasureFPS()
    {
        frameCount++;
        fpsTimer += Time.unscaledDeltaTime;

        if (fpsTimer >= fpsMeasureInterval)
        {
            float currentFPS = frameCount / fpsTimer;
            float currentTime = Time.time;
            measurements.Add(new Measurement { time = currentTime, fps = currentFPS });

            // Reset counters
            frameCount = 0;
            fpsTimer = 0f;
        }
    }

    private void StartPlayback()
    {
        if (route != null && route.keyframes != null && route.keyframes.Count > 1)
        {
            currentKeyframe = 0;
            StartCoroutine(PlaybackRoutine());
            Debug.Log("Playback started.");
        }
        else
        {
            Debug.LogError("Invalid route data. Playback cannot start.");
        }
    }

    private IEnumerator PlaybackRoutine()
    {
        while (currentKeyframe < route.keyframes.Count - 1)
        {
            CameraKeyframe current = route.keyframes[currentKeyframe];
            CameraKeyframe next = route.keyframes[currentKeyframe + 1];
            float elapsed = 0f;

            while (elapsed < lerpDuration / playbackSpeed)
            {
                float t = elapsed / (lerpDuration / playbackSpeed);
                transform.position = Vector3.Lerp(current.position, next.position, t);
                transform.rotation = Quaternion.Lerp(current.rotation, next.rotation, t);
                elapsed += Time.deltaTime * playbackSpeed;
                yield return null;
            }

            currentKeyframe++;
        }

        // Ensure the camera is set to the last keyframe
        if (route.keyframes.Count > 0)
        {
            CameraKeyframe last = route.keyframes[route.keyframes.Count - 1];
            transform.position = last.position;
            transform.rotation = last.rotation;
        }

        Debug.Log("Playback finished.");
        playbackFinished = true;
        SaveMeasurements();
        showExitMessage = true;
    }

    private void SaveMeasurements()
    {
        try
        {
            // Determine the full path for the CSV file
            string directory = Application.dataPath; // Save in Assets folder
            string fullPath = Path.Combine(directory, measurementsFilePath);

            // Use StringBuilder for efficient string concatenation
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Time,FPS");

            foreach (var measurement in measurements)
            {
                sb.AppendLine($"{measurement.time:F3},{measurement.fps:F2}");
            }

            File.WriteAllText(fullPath, sb.ToString());
            Debug.Log($"Measurements saved to {fullPath}");

            EndText.text = $"Measurements saved to {fullPath} . Press escape to escape";
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save measurements: {ex.Message}");
        }
    }

    private void LoadSampleScene()
    {
        // Replace "SampleScene" with the exact name of your scene
        SceneManager.LoadScene("SampleScene");
    }

    private void OnGUI()
    {
        if (showExitMessage)
        {
            string message = $"Measurements are written to \"{measurementsFilePath}\", press Esc to exit.";
            // Center the text on the screen
            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 25, 400, 50), message, guiStyle);
        }
    }

    // Struct to hold measurement data
    [Serializable]
    private struct Measurement
    {
        public float time;
        public float fps;
    }

    // Assuming CameraRoute and CameraKeyframe are defined elsewhere
    [Serializable]
    public class CameraRoute
    {
        public List<CameraKeyframe> keyframes;
    }

    [Serializable]
    public class CameraKeyframe
    {
        public Vector3 position;
        public Quaternion rotation;
    }
}
