using System;
using UnityEngine;

namespace GazeTracking
{
    public abstract class GazeUpdater
    {
        public abstract void Initialize();
        public abstract void Cleanup();
        public abstract Vector2 GetGazeDirectionVector();
    }

    public class GazeMouseUpdater : GazeUpdater
    {
        public override void Initialize()
        {
        }

        public override void Cleanup()
        {
        }

        public override Vector2 GetGazeDirectionVector()
        {
            Vector3 mousePos = Input.mousePosition;
            float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
            float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;
            normalizedX *= -1;
            return new Vector2(-normalizedX, normalizedY);
        }
    }

    public class GazePluginUpdater : GazeUpdater
    {
        private const string DllName = "GazeTracking";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitializeGazeTracking();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CleanupGazeTracking();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetGazeDirection(out float x, out float y);

        private static bool isInitialized = false;

        public override void Initialize()
        {
            if (!isInitialized)
            {
                InitializeGazeTracking();
                isInitialized = true;
            }
        }

        public override void Cleanup()
        {
            if (isInitialized)
            {
                CleanupGazeTracking();
                isInitialized = false;
            }
        }

        public override Vector2 GetGazeDirectionVector()
        {
            GetGazeDirection(out float x, out float y);
            return new Vector2(-x, y);
        }
    }
}
