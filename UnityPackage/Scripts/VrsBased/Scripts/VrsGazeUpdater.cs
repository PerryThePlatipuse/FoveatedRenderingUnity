// VrsBased/Scripts/VrsGazeUpdater.cs

using UnityEngine;
using GazeTracking;

namespace FoveatedRenderingVRS
{
    public enum GazeTrackingMethod
    {
        Plugin,
        Mouse
    }

    public class VrsGazeUpdater : MonoBehaviour
    {
        [SerializeField]
        private GazeTrackingMethod gazeTrackingMethod = GazeTrackingMethod.Plugin;

        // For adjusting final coordinates before sending to VrsPluginApi
        [SerializeField]
        float mulx = 1.02f;
        [SerializeField]
        float muly = 0.59f;

        public float x;
        public float y;

        private GazeUpdater gazeImplementation;

        private void Awake()
        {
            // Ensure the GazeUpdater is attached properly
            // This prevents multiple initializations
            if (GetComponent<VrsGazeUpdater>() == null)
            {
                gameObject.AddComponent<VrsGazeUpdater>();
            }
        }

        private void OnEnable()
        {
            InitializeGazeImplementation();
        }

        private void OnDisable()
        {
            // Cleanup Gaze
            if (gazeImplementation != null)
            {
                gazeImplementation.Cleanup();
                gazeImplementation = null;
            }
        }

        void Update()
        {
            RefreshGazeDirection();
        }

        private void InitializeGazeImplementation()
        {
            // Cleanup existing implementation if any
            if (gazeImplementation != null)
            {
                gazeImplementation.Cleanup();
                gazeImplementation = null;
            }

            // Instantiate the correct implementation
            switch (gazeTrackingMethod)
            {
                case GazeTrackingMethod.Mouse:
                    gazeImplementation = new GazeMouseUpdater();
                    break;
                case GazeTrackingMethod.Plugin:
                default:
                    gazeImplementation = new GazePluginUpdater();
                    break;
            }

            // Initialize Gaze
            gazeImplementation.Initialize();
        }

        private void RefreshGazeDirection()
        {
            if (gazeImplementation == null)
                return;

            // Retrieve the direction from the current updater
            Vector2 rawDirection = gazeImplementation.GetGazeDirectionVector();
            x = rawDirection.x;
            y = rawDirection.y;
            // Apply custom multipliers
            Vector3 calculatedGaze = new Vector3(
                x * mulx,
                y * muly,
                1.0f
            ).normalized;

            // Update plugin
            VrsPluginApi.UpdateGazeDirection(calculatedGaze);
            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
        }

        /// <summary>
        /// Sets the gaze tracking method dynamically.
        /// </summary>
        /// <param name="newMethod">The new gaze tracking method to switch to.</param>
        public void SetGazeTrackingMethod(GazeTrackingMethod newMethod)
        {
            if (gazeTrackingMethod != newMethod)
            {
                gazeTrackingMethod = newMethod;
                InitializeGazeImplementation();
                Debug.Log($"VrsGazeUpdater: Gaze tracking method set to {newMethod}.");
            }
        }

        public static bool AttachGazeUpdater(GameObject targetObject, GazeTrackingMethod method)
        {
            if (targetObject != null)
            {
                var gazeUpdater = targetObject.GetComponent<VrsGazeUpdater>();
                if (gazeUpdater == null)
                {
                    gazeUpdater = targetObject.AddComponent<VrsGazeUpdater>();
                }

                gazeUpdater.SetGazeTrackingMethod(method);
                gazeUpdater.enabled = true;

                return true;
            }

            return false;
        }
    }
}
