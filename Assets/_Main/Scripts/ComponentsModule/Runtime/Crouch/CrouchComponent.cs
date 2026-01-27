using UnityEngine;

namespace ComponentsModule
{
    public class CrouchComponent : ICrouchComponent
    {
        private readonly CharacterController _characterController;
        private readonly Transform _cameraRoot;
        private readonly CrouchData _data;

        public CrouchComponent(CharacterController characterController, Transform cameraRoot, CrouchData data)
        {
            _characterController = characterController;
            _cameraRoot = cameraRoot;
            _data = data;
        }

        public void Crouch(bool isCrouching)
        {
            var targetHeight = isCrouching ? _data.CrouchHeight : _data.StandHeight;
            var targetEyeHeight = isCrouching ? _data.EyeHeightCrouching : _data.EyeHeightStanding;

            var height = Mathf.Lerp(_characterController.height, targetHeight, Time.deltaTime * _data.CrouchSpeed);
            _characterController.height = height;
            _characterController.center = Vector3.up * height / 2f;

            var currentPosition = _cameraRoot.localPosition;
            var targetPosition = new Vector3(0, targetEyeHeight, 0);

            var position = Vector3.Lerp(currentPosition, targetPosition, Time.deltaTime * _data.CrouchSpeed);
            _cameraRoot.localPosition = position;
        }
    }
}