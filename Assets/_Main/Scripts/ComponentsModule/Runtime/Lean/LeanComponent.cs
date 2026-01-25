using UnityEngine;

namespace ComponentsModule
{
    public class LeanComponent : ILeanComponent
    {
        private readonly Transform _cameraRoot;

        private readonly float _leanAngle;
        private readonly float _leanOffset;
        private readonly float _leanSpeed;

        private float _currentLean;

        public LeanComponent(Transform cameraRoot, float leanAngle, float leanOffset, float leanSpeed)
        {
            _cameraRoot = cameraRoot;
            _leanAngle = leanAngle;
            _leanOffset = leanOffset;
            _leanSpeed = leanSpeed;
        }

        public void Lean(float targetLean)
        {
            _currentLean = Mathf.Lerp(_currentLean, targetLean, Time.deltaTime * _leanSpeed);

            var rotation = Quaternion.Euler(0, 0, -_currentLean * _leanAngle);
            _cameraRoot.localRotation = rotation;

            var position = _cameraRoot.localPosition;
            position.x = _currentLean * _leanOffset;
            _cameraRoot.localPosition = position;
        }
    }
}