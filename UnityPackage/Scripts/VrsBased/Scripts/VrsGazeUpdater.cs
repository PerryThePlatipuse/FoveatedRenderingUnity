// VrsBased/Scripts/VrsGazeUpdater.cs
using UnityEngine;
using GazeTracking;

namespace FoveatedRenderingVRS
{
    public class VrsGazeUpdater : MonoBehaviour
    {
        [SerializeField]
        float mulx = 1.02f; // for some reason nvapi's positions are shifted from needed, so you have to prescale coordinates.
        // I cannot establish any dependence between these coeficients and the size of a display, so you have to setup the coefficients manualy
        [SerializeField]
        float muly = 0.59f;

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
            // Vector2 normalizedDir = GazeUpdater.GetGazeDirectionVector();
            Vector3 mousePos = Input.mousePosition;

            // Normalize X and Y coordinates to the range -1 to 1
            float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
            float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;
            normalizedX *= -1;
            Debug.Log(new Vector2(normalizedX, normalizedY));

            Vector3 calculatedGaze = new Vector3(
                normalizedX * mulx,
                normalizedY * muly,
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