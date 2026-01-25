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

        public PlayerMovementController(IMoveComponent mover,
            IRotationComponent rotation,
            ICrouchComponent croucher,
            ILeanComponent leaner,
            IInputMap inputMap)
        {
            _mover = mover;
            _inputMap = inputMap;
            _rotation = rotation;
            _leaner = leaner;
            _croucher = croucher;
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
            var direction = _inputMap.MoveInput;
            var jumped = _inputMap.IsJumpPressed;
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