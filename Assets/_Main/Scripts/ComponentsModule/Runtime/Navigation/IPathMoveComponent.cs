using UnityEngine;

namespace ComponentsModule
{
    public interface IPathMoveComponent
    {
        bool IsMoving { get; }
        bool IsPathPending { get; }
        float RemainingDistance { get; }
        float StoppingDistance { get; }
        Vector3 Velocity { get; }
        float BaseSpeed { get; }
        float CurrentSpeed { get; }

        void SetStoppingDistance(float value);
        void MoveTo(Vector3 position);
        void Stop();
        void ResetPath();
        void ApplySpeedMultiplier(object source, float multiplier);
        void RemoveSpeedMultiplier(object source);
    }
}
