using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuController : MonoBehaviour
{
    // Settings
    public TMP_Dropdown sceneDropdown;
    public TMP_Dropdown gazeModeDropdown;
    public TMP_Dropdown gameModeDropdown;
    public Toggle VRSToggle;
    public Toggle LODToggle;
    public Toggle borderToggle;

    public Button startButton;

    public static string SelectedScene;
    public static string SelectedGazeMode;
    public static string SelectedGameMode;
    public static bool IsVRS;
    public static bool IsLOD;
    public static bool IsBorderOn;

    void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
    }

    void OnStartButtonClicked()
    {
        SelectedScene = sceneDropdown.options[sceneDropdown.value].text;
        SelectedGazeMode = gazeModeDropdown.options[gazeModeDropdown.value].text;
        SelectedGameMode = gameModeDropdown.options[gameModeDropdown.value].text;
        IsBorderOn = borderToggle.isOn;
        IsVRS = VRSToggle.isOn;
        IsLOD = LODToggle.isOn;

        ApplySettings();
        
        SceneManager.LoadScene(SelectedScene);
    }

    void ApplySettings()
    {
        GameManager.Instance.SetGazeMode(SelectedGazeMode);
        GameManager.Instance.SetGameMode(SelectedGameMode);
        GameManager.Instance.SetBorder(IsBorderOn);
        GameManager.Instance.SetVRS(IsVRS);
        GameManager.Instance.SetLOD(IsLOD);
    }
}
