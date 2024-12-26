// GazeTracking/GazePluginUpdater.cs

using UnityEngine;
using System.Runtime.InteropServices;

namespace GazeTracking
{
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
            // Flip X if needed
            return new Vector2(-x, y);
        }
    }
}
