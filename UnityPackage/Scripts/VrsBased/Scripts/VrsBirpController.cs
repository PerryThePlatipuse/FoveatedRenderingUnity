using UnityEngine;
using UnityEngine.Rendering;
using System;

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
        private CommandBufferManager bufferManager = new CommandBufferManager();

        private bool renderingInitialized = false;
        private bool renderingActive = false;

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
                VrsBirpApi.SetFoveatedRenderingShadingRatePreset(currentShadingPreset);

                if (currentShadingPreset == ShadingRatePreset.SHADING_RATE_CUSTOM)
                {
                    AssignShadingRate(TargetArea.INNER, innerRate);
                    AssignShadingRate(TargetArea.MIDDLE, middleRate);
                    AssignShadingRate(TargetArea.PERIPHERAL, peripheralRate);
                }

                GL.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
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
                VrsBirpApi.SetFoveatedRenderingPatternPreset(currentPatternPreset);

                if (currentPatternPreset == ShadingPatternPreset.SHADING_PATTERN_CUSTOM)
                {
                    AssignRegionRadius(TargetArea.INNER, innerRadius);
                    AssignRegionRadius(TargetArea.MIDDLE, middleRadius);
                    AssignRegionRadius(TargetArea.PERIPHERAL, peripheralRadius);
                }

                GL.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
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
                var clampedRate = rate.ClampValue(ShadingRate.CULL, ShadingRate.X1_PER_4X4_PIXELS);
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

                VrsBirpApi.SetShadingRate(area, clampedRate);
                GL.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
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

                VrsBirpApi.SetRegionRadii(area, clampedRadii);
                GL.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
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
            VrsBirpApi.InitializeNativeLogger(message => Debug.Log(message));

            mainCamera = GetComponent<Camera>();
            renderingInitialized = VrsBirpApi.InitializeFoveatedRendering(mainCamera.fieldOfView, mainCamera.aspect);
            if (renderingInitialized)
            {
                var currentPath = mainCamera.actualRenderingPath;
                if (currentPath == RenderingPath.Forward)
                {
                    bufferManager.AddCommandBuffer("Enable Foveated Rendering", CameraEvent.BeforeForwardOpaque,
                        cmd => cmd.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.ENABLE_FOVEATED_RENDERING),
                        cmd => cmd.ClearRenderTarget(false, true, Color.black));

                    bufferManager.AddCommandBuffer("Disable Foveated Rendering", CameraEvent.AfterForwardAlpha,
                        cmd => cmd.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.DISABLE_FOVEATED_RENDERING));
                }
                else if (currentPath == RenderingPath.DeferredShading)
                {
                    bufferManager.AddCommandBuffer("Enable Foveated Rendering - GBuffer", CameraEvent.BeforeGBuffer,
                        cmd => cmd.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.ENABLE_FOVEATED_RENDERING),
                        cmd => cmd.ClearRenderTarget(false, true, Color.black));

                    bufferManager.AddCommandBuffer("Disable Foveated Rendering - GBuffer", CameraEvent.AfterGBuffer,
                        cmd => cmd.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.DISABLE_FOVEATED_RENDERING));

                    bufferManager.AddCommandBuffer("Enable Foveated Rendering - Alpha", CameraEvent.BeforeForwardAlpha,
                        cmd => cmd.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.ENABLE_FOVEATED_RENDERING));

                    bufferManager.AddCommandBuffer("Disable Foveated Rendering - Alpha", CameraEvent.AfterForwardAlpha,
                        cmd => cmd.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.DISABLE_FOVEATED_RENDERING));
                }

                ToggleFoveatedRendering(true);
                bool isGazeAttached = VrsBirpGazeUpdater.AttachGazeUpdater(gameObject);

                ConfigureShadingRatePreset(currentShadingPreset);
                ConfigureShadingPatternPreset(currentPatternPreset);

                VrsBirpApi.SetNormalizedGazeDirection(new Vector3(0.0f, 0.0f, 1.0f), new Vector3(0.0f, 0.0f, 1.0f));
                GL.IssuePluginEvent(VrsBirpApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
            }
        }

        void OnDisable()
        {
            ToggleFoveatedRendering(false);
            bufferManager.ClearAllBuffers();

            VrsBirpApi.ReleaseFoveatedRendering();
            VrsBirpApi.ReleaseNativeLogger();

            renderingInitialized = false;

            var gazeUpdater = GetComponent<VrsBirpGazeUpdater>();
            if (gazeUpdater != null)
            {
                gazeUpdater.enabled = false;
            }
        }

        void OnPreRender()
        {
            VrsBirpApi.SetRenderMode(RenderMode.RENDER_MODE_MONO);
        }
    }
}
