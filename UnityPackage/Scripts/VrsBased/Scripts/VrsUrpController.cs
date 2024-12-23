using UnityEngine;
using UnityEngine.Rendering;
using FoveatedRenderingVRS;
using GazeTracking;

/// <summary>
/// Equivalent controller for URP that configures the foveated rendering plugin.
/// Attach this to your main camera or a central GameObject.
/// The URP pipeline's VrsUrpFeature handles the actual plugin calls in passes.
/// </summary>
[RequireComponent(typeof(Camera))]
public class VrsUrpController : MonoBehaviour
{
    private bool renderingInitialized = false;

    [SerializeField]
    private ShadingRatePreset currentShadingPreset = ShadingRatePreset.SHADING_RATE_HIGHEST_PERFORMANCE;
    [SerializeField]
    private ShadingPatternPreset currentPatternPreset = ShadingPatternPreset.SHADING_PATTERN_NARROW;

    [SerializeField]
    private Vector2 innerRadius = new Vector2(0.25f, 0.25f);
    [SerializeField]
    private Vector2 middleRadius = new Vector2(0.33f, 0.33f);
    [SerializeField]
    private Vector2 peripheralRadius = new Vector2(1.0f, 1.0f);

    [SerializeField]
    private ShadingRate innerRate = ShadingRate.X1_PER_PIXEL;
    [SerializeField]
    private ShadingRate middleRate = ShadingRate.X1_PER_2X2_PIXELS;
    [SerializeField]
    private ShadingRate peripheralRate = ShadingRate.X1_PER_4X4_PIXELS;

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

            ConfigureShadingRatePreset(currentShadingPreset);
            ConfigureShadingPatternPreset(currentPatternPreset);

            VrsPluginApi.UpdateGazeDirection(Vector3.forward);
            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);

            if (zoneVisualizer != null)
            {
                zoneVisualizer.UpdateRadii(innerRadius, middleRadius);
            }
            else
            {
                Debug.LogWarning("ZoneVisualizer reference is not set in VrsUrpController.");
            }
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
