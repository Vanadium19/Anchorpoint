using ComponentsModule;
using InputModule.Core;
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

        public PlayerMovementController(IMoveComponent mover,
            IRotationComponent rotation,
            ICrouchComponent croucher,
            ILeanComponent leaner,
            IInputMap inputMap,
            PlayerConfig config)
        {
            _mover = mover;
            _inputMap = inputMap;
            _rotation = rotation;
            _leaner = leaner;
            _croucher = croucher;
            _config = config;
        }

        public void Tick()
        {
            Move();
            Rotate();
            Crouch();
            Lean();
        }

        private void Move()
        {
            var isCrouching = _inputMap.IsCrouchPressed;
            var jumped = _inputMap.IsJumpPressed && !isCrouching;

            var targetSpeed = isCrouching ? _config.CrouchSpeed : _config.WalkSpeed;
            _mover.SetSpeed(targetSpeed);

            var direction = _inputMap.MoveInput;
            _mover.Move(direction, jumped);
        }

        private void Rotate()
        {
            var direction = _inputMap.LookInput;
            _rotation.Rotate(direction);
        }

        private void Lean()
        {
            var targetLean = _inputMap.LeanInput;
            _leaner.Lean(targetLean);
        }

        private void Crouch()
        {
            var isCrouching = _inputMap.IsCrouchPressed;
            _croucher.Crouch(isCrouching);
        }
    }
}