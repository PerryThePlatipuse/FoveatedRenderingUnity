using UnityEngine;

public class SceneInitializer : MonoBehaviour
{
    void Start()
    {
        if (GameManager.Instance != null)
        {
            string gazeMode = GameManager.Instance.SelectedGazeMode;
            string gameMode = GameManager.Instance.SelectedGameMode;
            bool border = GameManager.Instance.IsBorderOn;
            bool vrs = GameManager.Instance.IsVRS;
            bool lod = GameManager.Instance.IsLOD;

            ApplySettings(gazeMode, gameMode, border, vrs, lod);
        }
        else
        {
            Debug.LogError("GameManager instance not found!");
        }
    }

    void ApplySettings(string gazeMode, string gameMode, bool border, bool vrs, bool lod)
    {
        switch (gazeMode)
        {
            case "Webcam":
                break;
            case "Mobile":
                break;
            case "Mouse":
                break;
        }

        switch (gameMode)
        {
            case "Benchmark":
                break;
            case "Free movement":
                break;
        }

        if (border)
        {
        }
        else
        {
        }

        if (vrs)
        {
        }

        if (lod)
        {
        }
    }
}
