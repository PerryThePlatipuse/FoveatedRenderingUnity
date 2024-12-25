// Assets/Plugins/VrsBased/Scripts/VrsUrpController.cs

using UnityEngine;
using UnityEngine.Rendering;
using FoveatedRenderingVRS;
using GazeTracking; // Namespace where ZoneVisualizer resides

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
    private Vector2 innerRadius = new Vector2(0.25f, 0.25f);
    [SerializeField]
    private Vector2 middleRadius = new Vector2(0.33f, 0.33f);
    [SerializeField]
    private Vector2 peripheralRadius = new Vector2(1.0f, 1.0f);

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

    void OnEnable()
    {
        mainCamera = GetComponent<Camera>();

        // Initialize foveated rendering
        renderingInitialized = VrsPluginApi.InitializeFoveatedRendering(mainCamera.fieldOfView, mainCamera.aspect);

        if (renderingInitialized)
        {
            bool isGazeAttached = VrsGazeUpdater.AttachGazeUpdater(gameObject);

            if (!isGazeAttached)
            {
                Debug.LogWarning("VrsUrpController: Failed to attach GazeUpdater.");
            }

            ConfigureShadingRatePreset(currentShadingPreset);
            ConfigureShadingPatternPreset(currentPatternPreset);

            VrsPluginApi.UpdateGazeDirection(Vector3.forward);
            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);

            // Update ZoneVisualizer radii
            if (zoneVisualizer != null)
            {
                zoneVisualizer.UpdateRadii(new Vector2(innerRadius.x / 4, innerRadius.y / 2), new Vector2(middleRadius.x / 4, middleRadius.y / 2));
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

        var gazeUpdater = GetComponent<VrsGazeUpdater>();
        if (gazeUpdater != null)
        {
            gazeUpdater.enabled = false;
        }
    }

    void Update()
    {
        if (renderingInitialized && zoneVisualizer != null)
        {
            // Get mouse position
            Vector2 mousePosition = Input.mousePosition;

            // Update the visualizer's center to the mouse position
            zoneVisualizer.SetCenter(mousePosition);
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
            if (zoneVisualizer != null)
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
}
