using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GazeTracking
{
    public static class GazeUpdater
    {
        private const string DllName = "GazeTracking";

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void InitializeGazeTracking();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void CleanupGazeTracking();

        [DllImport(DllName, CallingConvention = CallingConvention.Cdecl)]
        private static extern void GetGazeDirection(out float x, out float y);

        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (!isInitialized)
            {
                InitializeGazeTracking();
                isInitialized = true;
            }
        }

        public static void Cleanup()
        {
            if (isInitialized)
            {
                CleanupGazeTracking();
                isInitialized = false;
            }
        }

        public static Vector2 GetGazeDirectionVector()
        {
            GetGazeDirection(out float x, out float y);
            return new Vector2(-x, y);
        }
    }
}