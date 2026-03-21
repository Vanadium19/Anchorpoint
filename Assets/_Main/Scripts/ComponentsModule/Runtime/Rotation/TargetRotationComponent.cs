using UnityEngine;

namespace ComponentsModule
{
    public class TargetRotationComponent : ITargetRotationComponent
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;

        private readonly Transform _transform;

        public TargetRotationComponent(Transform transform)
        {
            _transform = transform;
        }

        public void RotateTowards(Vector3 targetPosition, float turnSpeed)
        {
            var direction = targetPosition - _transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
                return;

            var rotation = Quaternion.LookRotation(direction, Vector3.up);
            RotateTo(rotation, turnSpeed);
        }

        public void RotateTo(Quaternion targetRotation, float turnSpeed)
        {
            var currentYaw = _transform.eulerAngles.y;
            var targetYaw = targetRotation.eulerAngles.y;
            var nextYaw = Mathf.MoveTowardsAngle(currentYaw, targetYaw, turnSpeed * Time.deltaTime);

            _transform.rotation = Quaternion.Euler(0f, nextYaw, 0f);
        }
    }
}