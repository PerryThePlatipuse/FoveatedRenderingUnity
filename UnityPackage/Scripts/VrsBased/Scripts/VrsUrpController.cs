// Assets/Plugins/VrsBased/Scripts/VrsUrpController.cs

using UnityEngine;
using UnityEngine.Rendering;
using FoveatedRenderingVRS;
using GazeTracking; // Namespace where ZoneVisualizer resides
using UnityEngine.SceneManagement;

/// <summary>
/// Equivalent controller for URP that configures the foveated rendering plugin.
/// Attach this to your main camera or a central GameObject.
/// The URP pipeline's VrsUrpFeature handles the actual plugin calls in passes.
/// </summary>
[RequireComponent(typeof(Camera))]
public class VrsUrpController : MonoBehaviour
{
    private bool renderingInitialized = false;

    private ShadingRatePreset currentShadingPreset = ShadingRatePreset.SHADING_RATE_CUSTOM;
    private ShadingPatternPreset currentPatternPreset = ShadingPatternPreset.SHADING_PATTERN_CUSTOM;

    [SerializeField]
    private Vector2 innerRadius = new Vector2(0.7f, 0.4f);
    [SerializeField]
    private Vector2 middleRadius = new Vector2(1, 0.7f);
    [SerializeField]
    private Vector2 peripheralRadius = new Vector2(5, 5);

    [SerializeField]
    private ShadingRate innerRate = ShadingRate.NORMAL;
    [SerializeField]
    private ShadingRate middleRate = ShadingRate.REDUCTION_2X2;
    [SerializeField]
    private ShadingRate peripheralRate = ShadingRate.REDUCTION_4X4;

    private Camera mainCamera;

    [Header("Zone Visualizer")]
    [Tooltip("Reference to the ZoneVisualizer component.")]
    [SerializeField]
    private ZoneVisualizer zoneVisualizer;

    [SerializeField]
    public bool BorderOn = true;

    [Header("Gaze Tracking Settings")]
    [Tooltip("Select the gaze tracking method.")]
    [SerializeField]
    public GazeTrackingMethod gazeTrackingMethod = GazeTrackingMethod.Plugin;

    // Reference to the GazeUpdater for dynamic method switching
    private VrsGazeUpdater gazeUpdater;

    void OnEnable()
    {
        if (!BorderOn)
        {
            zoneVisualizer.isVisualizationEnabled = false;
        } else
        {
            zoneVisualizer.isVisualizationEnabled = true;
        }
        mainCamera = GetComponent<Camera>();

        // Initialize foveated rendering
        renderingInitialized = VrsPluginApi.InitializeFoveatedRendering(mainCamera.fieldOfView, mainCamera.aspect);

        if (renderingInitialized)
        {
            // Attach and configure GazeUpdater
            bool isGazeAttached = VrsGazeUpdater.AttachGazeUpdater(gameObject, gazeTrackingMethod);

            if (!isGazeAttached)
            {
                Debug.LogWarning("VrsUrpController: Failed to attach GazeUpdater.");
            }

            // Store reference for dynamic switching
            gazeUpdater = GetComponent<VrsGazeUpdater>();

            ConfigureShadingRatePreset(currentShadingPreset);
            ConfigureShadingPatternPreset(currentPatternPreset);

            VrsPluginApi.UpdateGazeDirection(Vector3.forward);
            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);

            // Update ZoneVisualizer radii
            if (zoneVisualizer != null)
            {
                if (BorderOn)
                {
                    zoneVisualizer.UpdateRadii(
                        new Vector2(innerRadius.x / 4, innerRadius.y / 2),
                        new Vector2(middleRadius.x / 4, middleRadius.y / 2)
                    );
                }
            }
            else
            {
                Debug.LogWarning("VrsUrpController: ZoneVisualizer reference is not set.");
            }
        }
        else
        {
            Debug.LogError("VrsUrpController: Failed to initialize foveated rendering.");
        }
    }

    void OnDisable()
    {
        if (renderingInitialized)
        {
            VrsPluginApi.ReleaseFoveatedRendering();
            renderingInitialized = false;
        }

        if (gazeUpdater != null)
        {
            gazeUpdater.enabled = false;
        }
    }

    void Update()
    {
        if (BorderOn && renderingInitialized && zoneVisualizer != null)
        {
            zoneVisualizer.SetCenter(new Vector2(gazeUpdater.x, gazeUpdater.y));
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SceneManager.LoadScene("SampleScene");
        }
    }

    public void ConfigureShadingRatePreset(ShadingRatePreset preset)
    {
        if (renderingInitialized)
        {
            VrsPluginApi.SetShadingRatePreset(preset);
            currentShadingPreset = preset;

            if (preset == ShadingRatePreset.SHADING_RATE_CUSTOM)
            {
                AssignShadingRate(TargetArea.INNER, innerRate);
                AssignShadingRate(TargetArea.MIDDLE, middleRate);
                AssignShadingRate(TargetArea.PERIPHERAL, peripheralRate);
            }

            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
        }
    }

    public void ConfigureShadingPatternPreset(ShadingPatternPreset preset)
    {
        if (renderingInitialized)
        {
            VrsPluginApi.SetFoveationPatternPreset(preset);
            currentPatternPreset = preset;

            if (preset == ShadingPatternPreset.SHADING_PATTERN_CUSTOM)
            {
                AssignRegionRadius(TargetArea.INNER, innerRadius);
                AssignRegionRadius(TargetArea.MIDDLE, middleRadius);
                AssignRegionRadius(TargetArea.PERIPHERAL, peripheralRadius);
            }

            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
        }
    }

    public void AssignShadingRate(TargetArea area, ShadingRate rate)
    {
        if (renderingInitialized)
        {
            VrsPluginApi.ConfigureShadingRate(area, rate);
            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
        }
    }

    public void AssignRegionRadius(TargetArea area, Vector2 radii)
    {
        if (renderingInitialized)
        {
            VrsPluginApi.ConfigureRegionRadii(area, radii);
            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);

            // Update ZoneVisualizer radii if INNER or MIDDLE
            if (BorderOn && zoneVisualizer != null)
            {
                if (area == TargetArea.INNER || area == TargetArea.MIDDLE)
                {
                    Vector2 newInnerRadius = area == TargetArea.INNER ? radii : innerRadius;
                    Vector2 newMiddleRadius = area == TargetArea.MIDDLE ? radii : middleRadius;
                    zoneVisualizer.UpdateRadii(newInnerRadius, newMiddleRadius);
                }
            }
        }
    }

    /// <summary>
    /// Switches the gaze tracking method at runtime.
    /// </summary>
    /// <param name="newMethod">The new gaze tracking method to use.</param>
    public void SwitchGazeTrackingMethod(GazeTrackingMethod newMethod)
    {
        if (gazeUpdater != null && gazeTrackingMethod != newMethod)
        {
            Debug.Log($"VrsUrpController: Switching gaze tracking method to {newMethod}.");

            gazeTrackingMethod = newMethod;
            gazeUpdater.SetGazeTrackingMethod(newMethod);
        }
    }
}
