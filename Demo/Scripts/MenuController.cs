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
    public TMP_Dropdown methodDropdown;
    public Toggle borderToggle;

    public Button startButton;

    public static string SelectedScene;
    public static string SelectedGazeMode;
    public static string SelectedGameMode;
    public static string SelectedMethod;
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
        SelectedMethod = methodDropdown.options[methodDropdown.value].text;
        IsBorderOn = borderToggle.isOn;

        ApplySettings();
        
        if ()

        SceneManager.LoadScene(SelectedScene);
    }

    void ApplySettings()
    {
        GameManager.Instance.SetGazeMode(SelectedGazeMode);
        GameManager.Instance.SetGameMode(SelectedGameMode);
        GameManager.Instance.SetBorder(IsBorderOn);
        GameManager.Instance.SetMethod(SelectedMethod);
    }
}
