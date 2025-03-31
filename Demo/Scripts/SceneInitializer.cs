using UnityEngine;
using GazeTracking;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class SceneInitializer : MonoBehaviour
{
    [SerializeField]
    private Camera mainCamera;

    [SerializeField]
    private Canvas canvas;

    private VrsUrpController _vrsUrpController;


    void Awake()
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
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera is not assigned in the SceneInitializer.");
            return;
        }

        // 1) VRS Approach
        if (vrs)
        {
            EnableScript<VrsUrpController>();
            _vrsUrpController = mainCamera.GetComponent<VrsUrpController>();
            if (_vrsUrpController == null)
            {
                Debug.LogError("VrsUrpController component not found on the Main Camera.");
                return;
            }

            // Choose plugin or mouse for VRS
            switch (gazeMode)
            {
                case "Beam":
                    _vrsUrpController.gazeTrackingMethod = FoveatedRenderingVRS.GazeTrackingMethod.Plugin;
                    break;
                case "Mouse":
                    _vrsUrpController.gazeTrackingMethod = FoveatedRenderingVRS.GazeTrackingMethod.Mouse;
                    break;
                case "EyeGestures":
                    _vrsUrpController.gazeTrackingMethod = FoveatedRenderingVRS.GazeTrackingMethod.Python;
                    break;
            }

            _vrsUrpController.BorderOn = border;
        }
        else
        {
            // If not using VRS, hide the ZoneVisualizer
            if (canvas != null)
            {
                ZoneVisualizer z = canvas.GetComponent<ZoneVisualizer>();
                if (z != null) z.isVisualizationEnabled = false;
            }
            DisableScript<VrsUrpController>();
        }

        // 2) Game mode
        switch (gameMode)
        {
            case "Benchmark":
                DisableScript<CameraMover>();
                EnableScript<CameraRoutePlayer>();
                break;
            case "Free movement":
                DisableScript<CameraRoutePlayer>();
                EnableScript<CameraMover>();
                break;
        }

        // Set routeFileName based on the current scene
        CameraRoutePlayer cameraRoutePlayer = mainCamera.GetComponent<CameraRoutePlayer>();
        if (cameraRoutePlayer != null)
        {
            string sceneName = SceneManager.GetActiveScene().name;
            switch (sceneName)
            {
                case "NightCity":
                    cameraRoutePlayer.routeFileName = "CameraRoute.json";
                    break;
                case "Mountains":
                    cameraRoutePlayer.routeFileName = "CameraRouteMountains.json";
                    break;
                default:
                    Debug.LogWarning("Unknown scene: " + sceneName);
                    break;
            }
        }
        else
        {
            Debug.LogWarning("CameraRoutePlayer component not found on Main Camera.");
        }

        // 3) LOD Approach
        if (lod)
        {
            EnableScript<FoveatedLODController>();
            FoveatedLODController lodController = mainCamera.GetComponent<FoveatedLODController>();
            if (lodController != null)
            {
                if (vrs)
                {
                    lodController.overrideZoneVisualizer = false;
                    lodController.overrideGaze = false;
                }
                else
                {
                    lodController.overrideZoneVisualizer = true;
                    lodController.overrideGaze = true;
                }

                bool lodShowBorder = border;
                bool lodUsePluginGaze = (gazeMode == "Webcam");

                lodController.showBorder = lodShowBorder;
                lodController.usePluginGaze = lodUsePluginGaze;
            }
            else
            {
                Debug.LogWarning("FoveatedLODController component not found on Main Camera.");
            }
        }
        else
        {
            DisableScript<FoveatedLODController>();
        }
    }


    private void DisableScript<T>() where T : MonoBehaviour
    {
        T script = mainCamera.GetComponent<T>();
        if (script != null)
        {
            script.enabled = false;
            Debug.Log($"Disabled script: {typeof(T).Name}");
        }
        else
        {
            Debug.LogWarning($"Script {typeof(T).Name} not found on Main Camera.");
        }
    }

    private void EnableScript<T>() where T : MonoBehaviour
    {
        T script = mainCamera.GetComponent<T>();
        if (script != null)
        {
            script.enabled = true;
            Debug.Log($"Enabled script: {typeof(T).Name}");
        }
        else
        {
            Debug.LogWarning($"Script {typeof(T).Name} not found on Main Camera.");
        }
    }
}
