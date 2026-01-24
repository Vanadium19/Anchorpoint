using UnityEngine;

namespace CameraModule
{
    public class BuildingModeCameraService : IBuildingModeCameraService
    {
        private readonly Transform _transform;

        private readonly float _moveSpeed;
        private readonly float _rotationSpeed;

        private readonly float _minHeight;
        private readonly float _maxHeight;

        private readonly Vector3 _startPosition;
        private readonly Quaternion _startRotation;

        private float _angle;

        public BuildingModeCameraService(float moveSpeed, float rotationSpeed, float minHeight, float maxHeight)
        {
            _moveSpeed = moveSpeed;
            _rotationSpeed = rotationSpeed;

            _minHeight = minHeight;
            _maxHeight = maxHeight;

            _transform = Camera.main!.transform;
            _transform.GetPositionAndRotation(out _startPosition, out _startRotation);
        }

        public void Move(Vector3 direction)
        {
            var translation = direction * (_moveSpeed * Time.deltaTime);
            translation.y = 0;
            _transform.Translate(translation);
            Clamp();
        }

        public void Rotate(float delta)
        {
            var angle = delta * _rotationSpeed;
            _angle += angle;
            _transform.rotation = Quaternion.Euler(_startRotation.eulerAngles.x, _angle, 0f);
        }

        public void Reset() => _transform.SetPositionAndRotation(_startPosition, _startRotation);

        private void Clamp()
        {
            var position = _transform.position;
            position.y = Mathf.Clamp(position.y, _minHeight, _maxHeight);
            _transform.position = position;
        }
    }
}