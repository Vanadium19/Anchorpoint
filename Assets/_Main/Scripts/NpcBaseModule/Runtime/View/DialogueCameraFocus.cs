using UnityEngine;

namespace NpcBaseModule
{
    public sealed class DialogueCameraFocus : MonoBehaviour
    {
        private const float MinDirectionSqrMagnitude = 0.0001f;

        [SerializeField] private Transform playerRoot;
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float focusSpeed = 720f;
        [SerializeField] private float fallbackTargetHeight = 1.6f;
        [SerializeField] private float minPitch = -65f;
        [SerializeField] private float maxPitch = 65f;

        private Transform _target;
        private Vector3 _targetPosition;
        private bool _hasCustomTargetPosition;
        private bool _isActive;

        private void Awake()
        {
            playerRoot ??= transform;
            playerCamera ??= Camera.main;
        }

        private void LateUpdate()
        {
            if (!_isActive || playerRoot == null || playerCamera == null)
                return;

            var targetPosition = GetTargetPosition();

            RotatePlayerToTarget(targetPosition);
            RotateCameraToTarget(targetPosition);
        }

        public void Focus(Transform target)
        {
            _target = target;
            _hasCustomTargetPosition = false;
            _isActive = target != null;
        }

        public void Focus(Vector3 targetPosition)
        {
            _target = null;
            _targetPosition = targetPosition;
            _hasCustomTargetPosition = true;
            _isActive = true;
        }

        public void Clear()
        {
            _target = null;
            _hasCustomTargetPosition = false;
            _isActive = false;
        }

        private Vector3 GetTargetPosition()
        {
            if (_hasCustomTargetPosition)
                return _targetPosition;

            if (_target == null)
                return playerRoot.position + Vector3.up * fallbackTargetHeight;

            return _target.position;
        }

        private void RotatePlayerToTarget(Vector3 targetPosition)
        {
            var direction = targetPosition - playerRoot.position;
            direction.y = 0f;

            if (direction.sqrMagnitude < MinDirectionSqrMagnitude)
                return;

            var targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            playerRoot.rotation = Quaternion.RotateTowards(
                playerRoot.rotation,
                targetRotation,
                focusSpeed * Time.deltaTime
            );
        }

        private void RotateCameraToTarget(Vector3 targetPosition)
        {
            var cameraTransform = playerCamera.transform;
            var cameraParent = cameraTransform.parent;

            if (cameraParent == null)
                return;

            var localDirection = cameraParent.InverseTransformDirection(targetPosition - cameraTransform.position);

            if (localDirection.sqrMagnitude < MinDirectionSqrMagnitude)
                return;

            var pitch = Mathf.Atan2(-localDirection.y, localDirection.z) * Mathf.Rad2Deg;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            var targetRotation = Quaternion.Euler(pitch, 0f, 0f);
            cameraTransform.localRotation = Quaternion.RotateTowards(
                cameraTransform.localRotation,
                targetRotation,
                focusSpeed * Time.deltaTime
            );
        }
    }
}