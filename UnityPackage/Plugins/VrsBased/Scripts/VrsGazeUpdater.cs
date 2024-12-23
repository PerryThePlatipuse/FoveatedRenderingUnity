using UnityEngine;

namespace FoveatedRenderingVRS
{
    public class VrsGazeUpdater : MonoBehaviour
    {
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

        void OnEnable()
        {
        }

        void Update()
        {
            RefreshGazeDirection();
        }

        private void RefreshGazeDirection()
        {
            Vector3 currentMousePosition = Input.mousePosition;

            Vector2 normalizedMouse = new Vector2(1 - currentMousePosition.x / Screen.width, currentMousePosition.y / Screen.height);

            Vector3 calculatedGaze = new Vector3(
                (normalizedMouse.x - 0.5f) * 2.0f, // X-axis: left (-1) to right (+1)
                (normalizedMouse.y - 0.5f) * 2.0f, // Y-axis: bottom (-1) to top (+1)
                1.0f
            ).normalized;

            VrsPluginApi.UpdateGazeDirection(calculatedGaze);

            GL.IssuePluginEvent(VrsPluginApi.GetRenderEventFunc(), (int)FoveatedEventID.UPDATE_GAZE);
        }
    }
}
