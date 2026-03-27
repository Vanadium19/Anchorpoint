using ComponentsModule;
using InputModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public class PlayerMovementController : ITickable
    {
        private readonly IMoveComponent _mover;
        private readonly IRotationComponent _rotation;
        private readonly ICrouchComponent _croucher;
        private readonly ILeanComponent _leaner;
        private readonly IInputMap _inputMap;
        private readonly PlayerConfig _config;

        public PlayerMovementController(
            IMoveComponent mover,
            IRotationComponent rotation,
            ICrouchComponent croucher,
            ILeanComponent leaner,
            IInputMap inputMap,
            PlayerConfig config)
        {
            _mover = mover;
            _rotation = rotation;
            _croucher = croucher;
            _leaner = leaner;
            _inputMap = inputMap;
            _config = config;
        }

        public void Tick()
        {
            if (_inputMap.IsBuildMode)
            {
                MoveBuild();
                RotateBuild();
            }
            else
            {
                Move();
                Rotate();
                Crouch();
                Lean();
            }
        }

        private void Move()
        {
            var isCrouching = _inputMap.IsCrouchPressed;
            var targetSpeed = isCrouching ? _config.CrouchSpeed : _config.WalkSpeed;

            _mover.SetSpeed(targetSpeed);
            _mover.Move(_inputMap.MoveInput, _inputMap.IsJumpPressed && !isCrouching);
        }

        private void MoveBuild()
        {
            _mover.SetSpeed(_config.WalkSpeed);
            _mover.Move(_inputMap.BuildMoveInput, _inputMap.IsBuildJumpPressed);
        }

        private void Rotate()
        {
            _rotation.Rotate(_inputMap.LookInput);
        }

        private void RotateBuild()
        {
            _rotation.Rotate(_inputMap.BuildLookInput);
        }

        private void Crouch()
        {
            _croucher.Crouch(_inputMap.IsCrouchPressed);
        }

        private void Lean()
        {
            _leaner.Lean(_inputMap.LeanInput);
        }
    }
}