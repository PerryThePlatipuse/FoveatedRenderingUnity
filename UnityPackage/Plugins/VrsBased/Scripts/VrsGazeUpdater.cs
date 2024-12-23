// VrsBased/Scripts/VrsGazeUpdater.cs
using UnityEngine;
using GazeTracking;

namespace FoveatedRenderingVRS
{
    public class VrsGazeUpdater : MonoBehaviour
    {
        private void OnEnable()
        {
            GazeUpdater.Initialize();
        }

        private void OnDisable()
        {
            GazeUpdater.Cleanup();
        }

        void Update()
        {
            RefreshGazeDirection();
        }

        private void RefreshGazeDirection()
        {
            Vector2 normalizedDir = GazeUpdater.GetGazeDirectionVector();

            Vector3 calculatedGaze = new Vector3(
                normalizedDir.x,
                normalizedDir.y,
                1.0f
            ).normalized;

            VrsPluginApi.UpdateGazeDirection(calculatedGaze);

            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
        }

        public static bool AttachGazeUpdater(GameObject targetObject)
        {
            if (targetObject != null)
            {
                var gazeUpdater = targetObject.GetComponent<VrsGazeUpdater>();
                if (gazeUpdater == null)
                {
                    gazeUpdater = targetObject.AddComponent<VrsGazeUpdater>();
                }

                gazeUpdater.enabled = true;

                return true;
            }

            return false;
        }
    }
}