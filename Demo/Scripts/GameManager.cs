using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string SelectedScene;
    public string SelectedGazeMode;
    public string SelectedGameMode;
    public string SelectedMethod;
    public bool IsVRS;
    public bool IsLOD;
    public bool IsBorderOn;

    void Awake()
    {
        // singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetGazeMode(string mode)
    {
        SelectedGazeMode = mode;
        Debug.Log("Gaze Mode set to: " + mode);
    }

    public void SetGameMode(string mode)
    {
        SelectedGameMode = mode;
        Debug.Log("Game Mode set to: " + mode);
    }

    public void SetBorder(bool isOn)
    {
        IsBorderOn = isOn;
        Debug.Log("Border is " + (isOn ? "On" : "Off"));
    }

    public void SetVRS(bool isOn)
    {
        IsVRS = isOn;
        Debug.Log("VRS is " + (isOn ? "On" : "Off"));
    }

    public void SetLOD(bool isOn)
    {
        IsLOD = isOn;
        Debug.Log("VRS is " + (isOn ? "On" : "Off"));
    }
}
