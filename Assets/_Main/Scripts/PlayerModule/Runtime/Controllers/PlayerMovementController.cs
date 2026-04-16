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
            Move();
            Rotate();

            if (!_inputMap.IsBuildMode)
            {
                Crouch();
                Lean();
            }
        }

        private void Move()
        {
            var moveInput = _inputMap.IsBuildMode
                ? _inputMap.BuildMoveInput
                : _inputMap.MoveInput;

            var isJumpPressed = _inputMap.IsBuildMode
                ? _inputMap.IsBuildJumpPressed
                : _inputMap.IsJumpPressed;

            var isCrouching = !_inputMap.IsBuildMode && _inputMap.IsCrouchPressed;
            var targetSpeed = isCrouching ? _config.CrouchSpeed : _config.WalkSpeed;

            _mover.Move(moveInput, isJumpPressed && !isCrouching, targetSpeed);
        }

        private void Rotate()
        {
            var lookInput = _inputMap.IsBuildMode
                ? _inputMap.BuildLookInput
                : _inputMap.LookInput;

            _rotation.Rotate(lookInput);
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