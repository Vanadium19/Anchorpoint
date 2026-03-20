using UnityEngine;

namespace ComponentsModule
{
    public interface IPathMoveComponent
    {
        bool IsPathPending { get; }
        float RemainingDistance { get; }
        float StoppingDistance { get; }
        Vector3 Velocity { get; }

        void SetStoppingDistance(float value);
        void MoveTo(Vector3 position);
        void Stop();
        void ResetPath();
    }
}