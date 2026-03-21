using ComponentsModule;
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
        private readonly PlayerInputSwitcher _inputSwitcher;
        private readonly PlayerConfig _config;

        public PlayerMovementController(
            IMoveComponent mover,
            IRotationComponent rotation,
            ICrouchComponent croucher,
            ILeanComponent leaner,
            PlayerInputSwitcher inputSwitcher,
            PlayerConfig config)
        {
            _mover = mover;
            _rotation = rotation;
            _croucher = croucher;
            _leaner = leaner;
            _inputSwitcher = inputSwitcher;
            _config = config;
        }

        public void Tick()
        {
            var input = _inputSwitcher.CurrentInput;

            Move(input);
            Rotate(input);
            Crouch(input);
            Lean(input);
        }

        private void Move(IPlayerInput input)
        {
            if (input == null)
                return;

            var isCrouching = input.IsCrouchPressed;
            var targetSpeed = isCrouching ? _config.CrouchSpeed : _config.WalkSpeed;

            _mover.SetSpeed(targetSpeed);
            _mover.Move(input.MoveDirection, input.IsJumpPressed && !isCrouching);
        }

        private void Rotate(IPlayerInput input)
        {
            if (input == null)
                return;

            _rotation.Rotate(input.LookDirection);
        }

        private void Lean(IPlayerInput input)
        {
            if (input == null)
                return;

            _leaner.Lean(input.LeanDirection);
        }

        private void Crouch(IPlayerInput input)
        {
            if (input == null)
                return;

            _croucher.Crouch(input.IsCrouchPressed);
        }
    }
}
