using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string SelectedScene;
    public string SelectedGazeMode;
    public string SelectedGameMode;
    public string SelectedMethod;
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

    public void SetMethod(string method)
    {
        SelectedMethod = method;
        Debug.Log("Game Mode set to: " + method);
    }
}
