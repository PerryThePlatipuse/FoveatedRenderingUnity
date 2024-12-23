using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace FoveatedRenderingVRS
{
    public class VrsPluginApi
    {
        private const string LIBRARY_NAME = "VrsBased";

        // Unity Plugin Events
        [DllImport(LIBRARY_NAME)]
        public static extern IntPtr GetRenderEventFunc();

        // Foveated Rendering APIs
        [DllImport(LIBRARY_NAME)]
        public static extern bool InitializeFoveatedRendering(float verticalFov, float aspectRatio);

        [DllImport(LIBRARY_NAME)]
        public static extern void ReleaseFoveatedRendering();

        [DllImport(LIBRARY_NAME)]
        public static extern void SetRenderMode(RenderMode mode);

        [DllImport(LIBRARY_NAME)]
        public static extern void SetFoveationPatternPreset(ShadingPatternPreset preset);

        [DllImport(LIBRARY_NAME)]
        public static extern void SetShadingRatePreset(ShadingRatePreset preset);

        [DllImport(LIBRARY_NAME)]
        public static extern void ConfigureRegionRadii(TargetArea targetArea, Vector2 radii);

        [DllImport(LIBRARY_NAME)]
        public static extern void ConfigureShadingRate(TargetArea targetArea, ShadingRate rate);

        [DllImport(LIBRARY_NAME)]
        public static extern void UpdateGazeDirection(Vector3 gazeDir);
    }
}