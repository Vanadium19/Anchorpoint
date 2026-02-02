using UnityEngine;

namespace WeaponModule
{
    public class SwayProcessor
    {
        private Vector3 _currentPos;
        private Quaternion _currentRot;

        public Vector3 OutputPosition => _currentPos;
        public Quaternion OutputRotation => _currentRot;

        public void Update(Vector2 inputDelta, SwaySettings settings, float deltaTime, bool isAiming)
        {
            //FIXME: Magic numbers
            float multiplier = isAiming ? 0.1f : 1f;

            float moveX = Mathf.Clamp(-inputDelta.x * settings.Step * multiplier, -settings.MaxStep, settings.MaxStep);
            float moveY = Mathf.Clamp(-inputDelta.y * settings.Step * multiplier, -settings.MaxStep, settings.MaxStep);

            Vector3 finalPos = new Vector3(moveX, moveY, 0);
            _currentPos = Vector3.Lerp(_currentPos, finalPos, deltaTime * settings.Smooth);

            float rotationX = Mathf.Clamp(-inputDelta.y * settings.RotationStep * multiplier, -settings.MaxRotation, settings.MaxRotation);
            float rotationY = Mathf.Clamp(inputDelta.x * settings.RotationStep * multiplier, -settings.MaxRotation, settings.MaxRotation);
            float rotationZ = Mathf.Clamp(-inputDelta.x * settings.Tilt * multiplier, -settings.MaxTilt, settings.MaxTilt);

            Quaternion finalRot = Quaternion.Euler(rotationX, rotationY, rotationZ);
            _currentRot = Quaternion.Slerp(_currentRot, finalRot, deltaTime * settings.SmoothRot);
        }
    }
}