using System;
using BaseModule;
using ComponentsModule;
using InputModule;
using UnityEngine;
using Zenject;

namespace PlayerModule
{
    public class PlayerMovementController : IInitializable, ITickable, IDisposable, IPausable
    {
        private readonly IMoveComponent _mover;
        private readonly IRotationComponent _rotation;
        private readonly ICrouchComponent _croucher;
        private readonly ILeanComponent _leaner;
        private readonly IInputMap _inputMap;
        private readonly PlayerConfig _config;
        private readonly bool _isLeanEnabled;
        private readonly IPauseManager _pauseManager;

        private bool _isPaused;

        public PlayerMovementController(
            IMoveComponent mover,
            IRotationComponent rotation,
            ICrouchComponent croucher,
            ILeanComponent leaner,
            IInputMap inputMap,
            PlayerConfig config,
            IPauseManager pauseManager,
            bool isLeanEnabled)
        {
            _mover = mover;
            _rotation = rotation;
            _croucher = croucher;
            _leaner = leaner;
            _inputMap = inputMap;
            _config = config;
            _pauseManager = pauseManager;
            _isLeanEnabled = isLeanEnabled;
        }

        public void Initialize() => _pauseManager.Register(this);

        public void Dispose() => _pauseManager.Unregister(this);

        public void SetPaused(bool isPaused) => _isPaused = isPaused;

        public void Tick()
        {
            if (_isPaused || _inputMap.IsUIMode)
                return;

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
            var leanInput = _isLeanEnabled ? _inputMap.LeanInput : 0f;
            _leaner.Lean(leanInput);
        }
    }
}