using UnityEngine;

namespace ComponentsModule
{
    public class RotationComponent : IRotationComponent
    {
        private readonly Transform _transform;
        private readonly Transform _cameraTransform;

        private readonly float _mouseSensitivity;
        private readonly float _lookXLimit;

        private float _rotationX;
        private float _rotationY;

        public RotationComponent(Transform transform, float mouseSensitivity, float lookXLimit)
        {
            _transform = transform;
            _cameraTransform = Camera.main!.transform;

            _rotationY = _transform.eulerAngles.y;

            _mouseSensitivity = mouseSensitivity;
            _lookXLimit = lookXLimit;
        }

        public void Rotate(Vector2 direction)
        {
            SyncFromTransforms();

            _rotationX += -direction.y * _mouseSensitivity;
            _rotationX = Mathf.Clamp(_rotationX, -_lookXLimit, _lookXLimit);

            _rotationY += direction.x * _mouseSensitivity;

            _cameraTransform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            _transform.rotation = Quaternion.Euler(0, _rotationY, 0);
        }
        private void SyncFromTransforms()
        {
            _rotationX = NormalizeAngle(_cameraTransform.localEulerAngles.x);
            _rotationY = _transform.eulerAngles.y;
        }

        private float NormalizeAngle(float angle)
        {
            return angle > 180f ? angle - 360f : angle;
        }
    }
}