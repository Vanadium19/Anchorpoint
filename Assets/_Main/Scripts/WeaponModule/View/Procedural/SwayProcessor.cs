using UnityEngine;
using WeaponModule.Configs;

namespace WeaponModule.View.Procedural
{
    public class SwayProcessor
    {
        private Vector3 _currentPos;
        private Quaternion _currentRot;

        public Vector3 OutputPosition => _currentPos;
        public Quaternion OutputRotation => _currentRot;

        public void Update(Vector2 inputDelta, WeaponConfig.SwaySettings settings, float deltaTime, bool isAiming)
        {
            float multiplier = isAiming ? 0.1f : 1f;

            float moveX = Mathf.Clamp(-inputDelta.x * settings.Step * multiplier, -settings.MaxStep, settings.MaxStep);
            float moveY = Mathf.Clamp(-inputDelta.y * settings.Step * multiplier, -settings.MaxStep, settings.MaxStep);

            Vector3 finalPos = new Vector3(moveX, moveY, 0);
            _currentPos = Vector3.Lerp(_currentPos, finalPos, deltaTime * settings.Smooth);

            float rotX = Mathf.Clamp(-inputDelta.y * settings.RotationStep * multiplier, -settings.MaxRotation, settings.MaxRotation);
            float rotY = Mathf.Clamp(inputDelta.x * settings.RotationStep * multiplier, -settings.MaxRotation, settings.MaxRotation);
            float rotZ = Mathf.Clamp(-inputDelta.x * settings.Tilt * multiplier, -settings.MaxTilt, settings.MaxTilt);

            Quaternion finalRot = Quaternion.Euler(rotX, rotY, rotZ);
            _currentRot = Quaternion.Slerp(_currentRot, finalRot, deltaTime * settings.SmoothRot);
        }
    }
}