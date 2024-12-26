////////////////////////////////////////////
// GazeTracking/GazeUpdater.cs
////////////////////////////////////////////

using UnityEngine;

namespace GazeTracking
{
    public abstract class GazeUpdater
    {
        public abstract void Initialize();
        public abstract void Cleanup();
        public abstract Vector2 GetGazeDirectionVector();
    }
}
