using UnityEngine;

namespace ComponentsModule
{
    public interface ITargetRotationComponent
    {
        void RotateTowards(Vector3 targetPosition, float turnSpeed);
        void RotateTo(Quaternion targetRotation, float turnSpeed);
    }
}