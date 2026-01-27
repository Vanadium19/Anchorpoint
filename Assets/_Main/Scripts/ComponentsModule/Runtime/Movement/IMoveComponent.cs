using UnityEngine;

namespace ComponentsModule
{
    public interface IMoveComponent
    {
        void Move(Vector2 direction, bool jumpe);
        void SetSpeed(float value);
    }
}