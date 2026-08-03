using InputModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public class PlayerFootstepController : ITickable
    {
        private readonly CharacterController _characterController;
        private readonly IInputMap _inputMap;
        private readonly PlayerConfig _config;
        private readonly PlayerFootstepView _view;

        private float _elapsedTime;

        public PlayerFootstepController(
            CharacterController characterController,
            IInputMap inputMap,
            PlayerConfig config,
            PlayerFootstepView view)
        {
            _characterController = characterController;
            _inputMap = inputMap;
            _config = config;
            _view = view;
        }

        public void Tick()
        {
            if (_inputMap.IsUIMode || !_characterController.isGrounded || !IsMoving())
            {
                _elapsedTime = 0f;
                return;
            }

            _elapsedTime += Time.deltaTime;

            var interval = _inputMap.IsCrouchPressed
                ? _config.CrouchFootstepInterval
                : _config.WalkFootstepInterval;

            if (_elapsedTime < interval)
                return;

            _elapsedTime = 0f;
            _view.PlayFootstep();
        }

        private bool IsMoving()
        {
            var velocity = _characterController.velocity;
            velocity.y = 0f;
            return velocity.sqrMagnitude >= _config.MinimumFootstepSpeed * _config.MinimumFootstepSpeed;
        }
    }
}
