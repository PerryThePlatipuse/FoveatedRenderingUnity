using UnityEngine;
using UnityEngine.Rendering;
using System;
using FoveatedRenderingVRS;

using GazeTracking;
namespace FoveatedRenderingVRS_BIRP
{
    public static class FoveatedRenderingExtensions
    {
        /// <summary>
        /// Clamps the input value between the specified minimum and maximum.
        /// </summary>
        public static T ClampValue<T>(this T input, T min, T max) where T : IComparable
        {
            if (min.CompareTo(input) > 0)
            {
                return min;
            }
            else if (max.CompareTo(input) < 0)
            {
                return max;
            }

            return input;
        }
    }

    [RequireComponent(typeof(Camera))]
    public class VrsBirpController : MonoBehaviour
    {
        private Camera mainCamera = null;
        private VrsBirpCommandBufferManager bufferManager = new VrsBirpCommandBufferManager();

        private bool renderingInitialized = false;
        private bool renderingActive = false;

        [SerializeField]
        private ShadingRatePreset currentShadingPreset = ShadingRatePreset.SHADING_RATE_CUSTOM;
        [SerializeField]
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

        [SerializeField]
        private bool enableZoneVisualizer = true;
        
        [Header("Zone Visualizer")]
        [Tooltip("Reference to the ZoneVisualizer component.")]
        [SerializeField]
        private ZoneVisualizer zoneVisualizer;

        /// <summary>
        /// Enables or disables foveated rendering.
        /// </summary>
        /// <param name="activate">True to enable; false to disable.</param>
        public void ToggleFoveatedRendering(bool activate)
        {
            if (renderingInitialized && activate != renderingActive)
            {
                renderingActive = activate;
                if (activate)
                {
                    bufferManager.EnableBuffers(mainCamera);
                }
                else
                {
                    bufferManager.DisableBuffers(mainCamera);
                }
                // Update ZoneVisualizer radii
                if (zoneVisualizer != null)
                {
                    if (enableZoneVisualizer) {
                        zoneVisualizer.UpdateRadii(new Vector2(innerRadius.x / 4, innerRadius.y / 2), new Vector2(middleRadius.x / 4, middleRadius.y / 2));
                    }

                }
                else
                {
                    Debug.LogWarning("VrsUrpController: ZoneVisualizer reference is not set.");
                }
            }
        }

        /// <summary>
        /// Sets the shading rate preset.
        /// </summary>
        public void ConfigureShadingRatePreset(ShadingRatePreset preset)
        {
            if (renderingInitialized)
            {
                currentShadingPreset = preset.ClampValue(ShadingRatePreset.SHADING_RATE_HIGHEST_PERFORMANCE, ShadingRatePreset.SHADING_RATE_MAX);
                VrsPluginApi.SetShadingRatePreset(currentShadingPreset);

                if (currentShadingPreset == ShadingRatePreset.SHADING_RATE_CUSTOM)
                {
                    AssignShadingRate(TargetArea.INNER, innerRate);
                    AssignShadingRate(TargetArea.MIDDLE, middleRate);
                    AssignShadingRate(TargetArea.PERIPHERAL, peripheralRate);
                }

                GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
            }
        }

        public ShadingRatePreset GetCurrentShadingRatePreset()
        {
            return currentShadingPreset;
        }

        public void ConfigureShadingPatternPreset(ShadingPatternPreset preset)
        {
            if (renderingInitialized)
            {
                currentPatternPreset = preset.ClampValue(ShadingPatternPreset.SHADING_PATTERN_WIDE, ShadingPatternPreset.SHADING_PATTERN_MAX);
                VrsPluginApi.SetFoveationPatternPreset(currentPatternPreset);

                if (currentPatternPreset == ShadingPatternPreset.SHADING_PATTERN_CUSTOM)
                {
                    AssignRegionRadius(TargetArea.INNER, innerRadius);
                    AssignRegionRadius(TargetArea.MIDDLE, middleRadius);
                    AssignRegionRadius(TargetArea.PERIPHERAL, peripheralRadius);
                }

                GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
            }
        }

        public ShadingPatternPreset GetCurrentShadingPatternPreset()
        {
            return currentPatternPreset;
        }

        public void AssignShadingRate(TargetArea area, ShadingRate rate)
        {
            if (renderingInitialized)
            {
                var clampedRate = rate.ClampValue(ShadingRate.CULL, ShadingRate.REDUCTION_4X4);
                switch (area)
                {
                    case TargetArea.INNER:
                        innerRate = clampedRate;
                        break;
                    case TargetArea.MIDDLE:
                        middleRate = clampedRate;
                        break;
                    case TargetArea.PERIPHERAL:
                        peripheralRate = clampedRate;
                        break;
                }

                VrsPluginApi.ConfigureShadingRate(area, clampedRate);
                GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
            }
        }

        public ShadingRate GetShadingRate(TargetArea area)
        {
            switch (area)
            {
                case TargetArea.INNER:
                    return innerRate;
                case TargetArea.MIDDLE:
                    return middleRate;
                case TargetArea.PERIPHERAL:
                    return peripheralRate;
            }

            return ShadingRate.CULL;
        }

        public void AssignRegionRadius(TargetArea area, Vector2 radii)
        {
            if (renderingInitialized)
            {
                var clampedRadii = new Vector2(
                    radii.x.ClampValue(0.01f, 10.0f),
                    radii.y.ClampValue(0.01f, 10.0f)
                );

                switch (area)
                {
                    case TargetArea.INNER:
                        innerRadius = clampedRadii;
                        break;
                    case TargetArea.MIDDLE:
                        middleRadius = clampedRadii;
                        break;
                    case TargetArea.PERIPHERAL:
                        peripheralRadius = clampedRadii;
                        break;
                }

                VrsPluginApi.ConfigureRegionRadii(area, clampedRadii);
                GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
                
                // Update ZoneVisualizer radii if INNER or MIDDLE
                if (zoneVisualizer != null)
                {
                    if (enableZoneVisualizer && area == TargetArea.INNER || area == TargetArea.MIDDLE)
                    {
                        Vector2 newInnerRadius = area == TargetArea.INNER ? radii : innerRadius;
                        Vector2 newMiddleRadius = area == TargetArea.MIDDLE ? radii : middleRadius;
                        zoneVisualizer.UpdateRadii(new Vector2(newInnerRadius.x / 4, newInnerRadius.y / 2), new Vector2(newMiddleRadius.x / 4, newMiddleRadius.y / 2));
                    }
                }
            }
        }

        public Vector2 GetRegionRadius(TargetArea area)
        {
            switch (area)
            {
                case TargetArea.INNER:
                    return innerRadius;
                case TargetArea.MIDDLE:
                    return middleRadius;
                case TargetArea.PERIPHERAL:
                    return peripheralRadius;
            }

            return Vector2.zero;
        }

        void OnEnable()
        {

            mainCamera = GetComponent<Camera>();
            renderingInitialized = VrsPluginApi.InitializeFoveatedRendering(mainCamera.fieldOfView, mainCamera.aspect);
            if (renderingInitialized)
            {
                var currentPath = mainCamera.actualRenderingPath;
                if (currentPath == RenderingPath.Forward)
                {
                    bufferManager.AddCommandBuffer("Enable Foveated Rendering", CameraEvent.BeforeForwardOpaque,
                        cmd => cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.ENABLE_FOVEATED_RENDERING),
                        cmd => cmd.ClearRenderTarget(false, true, Color.black));

                    bufferManager.AddCommandBuffer("Disable Foveated Rendering", CameraEvent.AfterForwardAlpha,
                        cmd => cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.DISABLE_FOVEATED_RENDERING));
                }
                else if (currentPath == RenderingPath.DeferredShading)
                {
                    bufferManager.AddCommandBuffer("Enable Foveated Rendering - GBuffer", CameraEvent.BeforeGBuffer,
                        cmd => cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.ENABLE_FOVEATED_RENDERING),
                        cmd => cmd.ClearRenderTarget(false, true, Color.black));

                    bufferManager.AddCommandBuffer("Disable Foveated Rendering - GBuffer", CameraEvent.AfterGBuffer,
                        cmd => cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.DISABLE_FOVEATED_RENDERING));

                    bufferManager.AddCommandBuffer("Enable Foveated Rendering - Alpha", CameraEvent.BeforeForwardAlpha,
                        cmd => cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.ENABLE_FOVEATED_RENDERING));

                    bufferManager.AddCommandBuffer("Disable Foveated Rendering - Alpha", CameraEvent.AfterForwardAlpha,
                        cmd => cmd.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.DISABLE_FOVEATED_RENDERING));
                }

                ToggleFoveatedRendering(true);
                bool isGazeAttached = VrsGazeUpdater.AttachGazeUpdater(gameObject);

                ConfigureShadingRatePreset(currentShadingPreset);
                ConfigureShadingPatternPreset(currentPatternPreset);

                VrsPluginApi.UpdateGazeDirection(new Vector3(0.0f, 0.0f, 1.0f));
                GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
            }
        }
        
        void Update()
        {
            if (renderingInitialized && if enableZoneVisualizer && zoneVisualizer != null)
            {
                // Get mouse position
                Vector2 mousePosition = Input.mousePosition;

                // Update the visualizer's center to the mouse position
                zoneVisualizer.SetCenter(mousePosition);
            }
        }

        void OnDisable()
        {
            ToggleFoveatedRendering(false);
            bufferManager.ClearAllBuffers();

            VrsPluginApi.ReleaseFoveatedRendering();

            renderingInitialized = false;

            var gazeUpdater = GetComponent<VrsGazeUpdater>();
            if (gazeUpdater != null)
            {
                gazeUpdater.enabled = false;
            }
        }

        void OnPreRender()
        {
        }
    }
}
