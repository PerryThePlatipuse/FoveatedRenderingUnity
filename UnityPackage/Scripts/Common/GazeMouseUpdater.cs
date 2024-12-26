// GazeTracking/GazeMouseUpdater.cs

using UnityEngine;

namespace GazeTracking
{
    public class GazeMouseUpdater : GazeUpdater
    {
        public override void Initialize()
        {
            // No special initialization for mouse
        }

        public override void Cleanup()
        {
            // No special cleanup for mouse
        }

        public override Vector2 GetGazeDirectionVector()
        {
            // Convert mouse position to normalized -1..1 range
            Vector3 mousePos = Input.mousePosition;
            float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
            float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;

            // Flip X if desired (example usage; you can remove or invert if needed)
            normalizedX *= -1;

            // Return the 2D direction
            return new Vector2(normalizedX, normalizedY);
        }
    }
}
