using UnityEngine;

namespace ComponentsModule
{
    public interface IMoveComponent
    {
        float BaseSpeed { get; }
        float CurrentSpeed { get; }

        void Move(Vector2 direction, bool jump, float baseSpeed);
        void SetSpeedMultiplier(float multiplier);
        void ResetSpeedMultiplier();
    }
}